using System.Globalization;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class RequestDocxReportWriter(ImageSizeReader imageSizeReader)
{
    public byte[] Build(
        DamageRequest request,
        AppLanguage lang,
        IReadOnlyList<string> images,
        ILogger logger,
        out bool hasZipSignature
    )
    {
        using var memoryStream = new MemoryStream();
        using (
            var document = WordprocessingDocument.Create(
                memoryStream,
                WordprocessingDocumentType.Document,
                true
            )
        )
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            var body = mainPart.Document.Body!;

            body.Append(CreateHeading(ExportLocalization.DocTitle(lang)));
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.ClientInfo(lang)));
            body.Append(
                CreateKeyValueTable(
                    new Dictionary<string, string>
                    {
                        [ExportLocalization.Name(lang)] = FormatClientFullName(request),
                        [ExportLocalization.Email(lang)] = request.Email,
                        [ExportLocalization.Phone(lang)] = request.Phone,
                        [ExportLocalization.CreatedAt(lang)] = request.CreatedAt.ToString(
                            "yyyy-MM-dd HH:mm",
                            CultureInfo.InvariantCulture
                        ),
                    }
                )
            );
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.CarInfo(lang)));
            body.Append(
                CreateKeyValueTable(
                    new Dictionary<string, string>
                    {
                        [ExportLocalization.Brand(lang)] = request.CarBrand,
                        [ExportLocalization.Model(lang)] = request.CarModel,
                        [ExportLocalization.Year(lang)] = request.CarYear.ToString(
                            CultureInfo.InvariantCulture
                        ),
                    }
                )
            );
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.AiSummary(lang)));
            body.Append(CreateParagraph(request.AiSummary));
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.EstimatedCost(lang)));
            body.Append(
                CreateParagraph(
                    $"{request.AiEstimatedTotalCost.ToString("N0", CultureInfo.InvariantCulture)} {ExportLocalization.RubLabel(lang)}"
                )
            );
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.DamageItems(lang)));
            body.Append(CreateDamageItemsTable(request.EstimateItems, lang));
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.AdminComment(lang)));
            body.Append(
                CreateParagraph(
                    string.IsNullOrWhiteSpace(request.AdminDecisionComment)
                        ? ExportLocalization.NoComment(lang)
                        : request.AdminDecisionComment
                )
            );
            body.Append(CreateSpacer());

            body.Append(CreateSectionTitle(ExportLocalization.Status(lang)));
            body.Append(CreateParagraph(ExportLocalization.StatusValue(request.Status, lang)));
            body.Append(CreateSpacer());

            if (images.Count > 0)
            {
                body.Append(CreateSectionTitle(ExportLocalization.Photos(lang)));
                for (var index = 0; index < images.Count; index++)
                {
                    var imageAdded = AddImageToBody(
                        mainPart,
                        body,
                        images[index],
                        (uint)(index + 1)
                    );
                    if (imageAdded)
                    {
                        body.Append(CreateSpacer());
                    }
                    else
                    {
                        logger.LogWarning(
                            "Skipped unsupported image format in DOCX export. RequestId={RequestId}, ImagePath={ImagePath}",
                            request.Id,
                            images[index]
                        );
                    }
                }
            }

            mainPart.Document.Save();
        }

        var fileBytes = memoryStream.ToArray();
        hasZipSignature = fileBytes.Length >= 2 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B;
        return fileBytes;
    }

    public string BuildWordFileName(DamageRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.LastName)
            || string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.MiddleName)
        )
        {
            return $"request_{request.Id}.docx";
        }

        var baseName =
            $"{request.LastName.Trim()}_{request.FirstName.Trim()}_{request.MiddleName.Trim()}";
        var invalidChars = Path.GetInvalidFileNameChars();

        foreach (var invalidChar in invalidChars)
        {
            baseName = baseName.Replace(invalidChar, '_');
        }

        while (baseName.Contains(" ", StringComparison.Ordinal))
        {
            baseName = baseName.Replace(" ", "_", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(baseName)
            ? $"request_{request.Id}.docx"
            : $"{baseName}.docx";
    }

    private static string FormatClientFullName(DamageRequest request)
    {
        var parts = new[] { request.LastName, request.FirstName, request.MiddleName }.Where(part =>
            !string.IsNullOrWhiteSpace(part)
        );

        return string.Join(' ', parts);
    }

    private static Paragraph CreateHeading(string text)
    {
        return new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { After = "240" }),
            new Run(new RunProperties(new Bold(), new FontSize { Val = "36" }), new Text(text))
        );
    }

    private static Paragraph CreateSectionTitle(string text)
    {
        return new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "120", After = "120" }),
            new Run(new RunProperties(new Bold(), new FontSize { Val = "28" }), new Text(text))
        );
    }

    private static Paragraph CreateParagraph(string text)
    {
        return new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { After = "100" }),
            new Run(
                new RunProperties(new RunFonts { Ascii = "Calibri" }, new FontSize { Val = "22" }),
                new Text(text) { Space = SpaceProcessingModeValues.Preserve }
            )
        );
    }

    private static Paragraph CreateSpacer()
    {
        return new(new Run(new Text(string.Empty)));
    }

    private static Table CreateKeyValueTable(Dictionary<string, string> values)
    {
        var table = new Table();
        table.AppendChild(CreateTableProperties());

        foreach (var pair in values)
        {
            var row = new TableRow();
            row.Append(CreateCell(pair.Key, true));
            row.Append(CreateCell(pair.Value, false));
            table.Append(row);
        }

        return table;
    }

    private static Table CreateDamageItemsTable(
        IEnumerable<DamageEstimateItem> items,
        AppLanguage lang
    )
    {
        var table = new Table();
        table.AppendChild(CreateTableProperties());

        var header = new TableRow();
        header.Append(CreateCell(ExportLocalization.DamagePart(lang), true));
        header.Append(CreateCell(ExportLocalization.DamageDescription(lang), true));
        header.Append(CreateCell(ExportLocalization.DamageSeverity(lang), true));
        header.Append(CreateCell(ExportLocalization.DamageCost(lang), true));
        table.Append(header);

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            var row = new TableRow();
            row.Append(CreateCell(ExportLocalization.NoDamageItems(lang), false));
            row.Append(CreateCell(string.Empty, false));
            row.Append(CreateCell(string.Empty, false));
            row.Append(CreateCell(string.Empty, false));
            table.Append(row);
            return table;
        }

        foreach (var item in itemList)
        {
            var row = new TableRow();
            row.Append(CreateCell(item.PartName, false));
            row.Append(CreateCell(item.DamageDescription, false));
            row.Append(CreateCell(ExportLocalization.SeverityValue(item.Severity, lang), false));
            row.Append(
                CreateCell(
                    $"{item.EstimatedCost.ToString("N0", CultureInfo.InvariantCulture)} {ExportLocalization.RubLabel(lang)}",
                    false
                )
            );
            table.Append(row);
        }

        return table;
    }

    private static TableProperties CreateTableProperties()
    {
        return new TableProperties(
            new TableBorders(
                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new BottomBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 8,
                },
                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new RightBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 8,
                },
                new InsideHorizontalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 8,
                },
                new InsideVerticalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 8,
                }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        );
    }

    private static TableCell CreateCell(string text, bool isHeader)
    {
        var runProperties = isHeader ? new RunProperties(new Bold()) : new RunProperties();

        var paragraph = new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "40", After = "40" }),
            new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve })
        );

        return new TableCell(
            paragraph,
            new TableCellProperties(
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            )
        );
    }

    private bool AddImageToBody(
        MainDocumentPart mainPart,
        Body body,
        string imagePath,
        uint imageId
    )
    {
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        if (!imageSizeReader.TryResolveImagePartType(extension, out var imagePartType))
        {
            return false;
        }

        var imagePart = mainPart.AddImagePart(imagePartType);
        using (var stream = System.IO.File.OpenRead(imagePath))
        {
            imagePart.FeedData(stream);
        }

        var relationshipId = mainPart.GetIdOfPart(imagePart);
        var imageName = Path.GetFileName(imagePath);

        const long maxWidthEmu = 5_700_000L;
        const long maxHeightEmu = 7_200_000L;

        if (
            !imageSizeReader.TryCalculateImageSize(
                imagePath,
                maxWidthEmu,
                maxHeightEmu,
                out var finalWidth,
                out var finalHeight
            )
        )
        {
            return false;
        }

        var drawing = new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = finalWidth, Cy = finalHeight },
                new DW.EffectExtent
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 0L,
                    BottomEdge = 0L,
                },
                new DW.DocProperties { Id = imageId, Name = imageName },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }
                ),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties
                                {
                                    Id = imageId,
                                    Name = imageName,
                                },
                                new PIC.NonVisualPictureDrawingProperties()
                            ),
                            new PIC.BlipFill(
                                new A.Blip
                                {
                                    Embed = relationshipId,
                                    CompressionState = A.BlipCompressionValues.Print,
                                },
                                new A.Stretch(new A.FillRectangle())
                            ),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = finalWidth, Cy = finalHeight }
                                ),
                                new A.PresetGeometry(new A.AdjustValueList())
                                {
                                    Preset = A.ShapeTypeValues.Rectangle,
                                }
                            )
                        )
                    )
                    {
                        Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture",
                    }
                )
            )
            {
                DistanceFromTop = (UInt32Value)0U,
                DistanceFromBottom = (UInt32Value)0U,
                DistanceFromLeft = (UInt32Value)0U,
                DistanceFromRight = (UInt32Value)0U,
            }
        );

        body.AppendChild(
            new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "120", After = "180" }
                ),
                new Run(drawing)
            )
        );

        return true;
    }
}
