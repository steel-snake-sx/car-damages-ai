using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Models;
using ClosedXML.Excel;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class RequestListExcelWriter
{
    public byte[] BuildAll(IReadOnlyList<DamageRequest> requests, AppLanguage lang)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(ExportLocalization.ExcelSheetName(lang));

        var headers = ExportLocalization.ExcelHeaders(lang);

        for (var column = 0; column < headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEFF2");
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        for (var index = 0; index < requests.Count; index++)
        {
            var row = index + 2;
            var request = requests[index];

            sheet.Cell(row, 1).Value = request.Id.ToString();
            sheet.Cell(row, 2).Value = request.CreatedAt;
            sheet.Cell(row, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            sheet.Cell(row, 3).Value = ExportLocalization.StatusValue(request.Status, lang);
            sheet.Cell(row, 4).Value = FormatClientFullName(request);
            sheet.Cell(row, 5).Value = request.Email;
            sheet.Cell(row, 6).Value = request.Phone;
            sheet.Cell(row, 7).Value = $"{request.CarBrand} {request.CarModel} ({request.CarYear})";
            sheet.Cell(row, 8).Value = (double)request.AiEstimatedTotalCost;
            sheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 9).Value = request.ApprovedByUser?.Email ?? string.Empty;

            for (var column = 1; column <= headers.Length; column++)
            {
                sheet.Cell(row, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string FormatClientFullName(DamageRequest request)
    {
        return FormatFullName(request.LastName, request.FirstName, request.MiddleName);
    }
    private static string FormatFullName(string? lastName, string? firstName, string? middleName)
    {
        var parts = new[] { lastName, firstName, middleName }.Where(part =>
            !string.IsNullOrWhiteSpace(part)
        );

        return string.Join(' ', parts);
    }
}
