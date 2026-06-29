using DocumentFormat.OpenXml.Packaging;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class ImageSizeReader
{
    public bool TryCalculateImageSize(
        string imagePath,
        long maxWidthEmu,
        long maxHeightEmu,
        out long widthEmu,
        out long heightEmu
    )
    {
        const float fallbackDpi = 96f;
        const long emusPerInch = 914400L;

        widthEmu = 0;
        heightEmu = 0;

        if (TryReadImageSizeFromHeader(imagePath, out var pixelWidth, out var pixelHeight))
        {
            var originalWidthEmu = (long)Math.Round(pixelWidth / fallbackDpi * emusPerInch);
            var originalHeightEmu = (long)Math.Round(pixelHeight / fallbackDpi * emusPerInch);

            if (originalWidthEmu <= 0 || originalHeightEmu <= 0)
            {
                return false;
            }

            var scaleByWidth = (double)maxWidthEmu / originalWidthEmu;
            var scaleByHeight = (double)maxHeightEmu / originalHeightEmu;
            var scale = Math.Min(1d, Math.Min(scaleByWidth, scaleByHeight));

            widthEmu = Math.Max(1L, (long)Math.Round(originalWidthEmu * scale));
            heightEmu = Math.Max(1L, (long)Math.Round(originalHeightEmu * scale));
            return true;
        }

        return false;
    }

    public bool TryReadImageSizeFromHeader(string imagePath, out int width, out int height)
    {
        width = 0;
        height = 0;

        try
        {
            var bytes = System.IO.File.ReadAllBytes(imagePath);
            if (bytes.Length < 24)
            {
                return false;
            }

            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                width = ReadInt32BigEndian(bytes, 16);
                height = ReadInt32BigEndian(bytes, 20);
                return width > 0 && height > 0;
            }

            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                return TryReadJpegSize(bytes, out width, out height);
            }

            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
            {
                width = bytes[6] | (bytes[7] << 8);
                height = bytes[8] | (bytes[9] << 8);
                return width > 0 && height > 0;
            }

            if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            {
                width = BitConverter.ToInt32(bytes, 18);
                height = Math.Abs(BitConverter.ToInt32(bytes, 22));
                return width > 0 && height > 0;
            }

            if (
                bytes[0] == 0x52
                && bytes[1] == 0x49
                && bytes[2] == 0x46
                && bytes[3] == 0x46
                && bytes[8] == 0x57
                && bytes[9] == 0x45
                && bytes[10] == 0x42
                && bytes[11] == 0x50
            )
            {
                return TryReadWebpSize(bytes, out width, out height);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool TryReadJpegSize(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        var index = 2;
        while (index + 8 < bytes.Length)
        {
            while (index < bytes.Length && bytes[index] != 0xFF)
            {
                index++;
            }

            if (index + 1 >= bytes.Length)
            {
                return false;
            }

            var marker = bytes[index + 1];
            index += 2;

            if (marker == 0xD8 || marker == 0xD9)
            {
                continue;
            }

            if (index + 1 >= bytes.Length)
            {
                return false;
            }

            var segmentLength = ReadInt16BigEndian(bytes, index);
            if (segmentLength < 2 || index + segmentLength > bytes.Length)
            {
                return false;
            }

            if (
                (marker >= 0xC0 && marker <= 0xC3)
                || (marker >= 0xC5 && marker <= 0xC7)
                || (marker >= 0xC9 && marker <= 0xCB)
                || (marker >= 0xCD && marker <= 0xCF)
            )
            {
                if (index + 7 >= bytes.Length)
                {
                    return false;
                }

                height = ReadInt16BigEndian(bytes, index + 3);
                width = ReadInt16BigEndian(bytes, index + 5);
                return width > 0 && height > 0;
            }

            index += segmentLength;
        }

        return false;
    }

    public bool TryReadWebpSize(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (bytes.Length < 30)
        {
            return false;
        }

        if (bytes[12] == 0x56 && bytes[13] == 0x50 && bytes[14] == 0x38 && bytes[15] == 0x58)
        {
            width = 1 + bytes[24] + (bytes[25] << 8) + (bytes[26] << 16);
            height = 1 + bytes[27] + (bytes[28] << 8) + (bytes[29] << 16);
            return width > 0 && height > 0;
        }

        if (bytes[12] == 0x56 && bytes[13] == 0x50 && bytes[14] == 0x38 && bytes[15] == 0x20)
        {
            if (bytes.Length < 30)
            {
                return false;
            }

            width = ReadInt16LittleEndian(bytes, 26);
            height = ReadInt16LittleEndian(bytes, 28);
            return width > 0 && height > 0;
        }

        if (bytes[12] == 0x56 && bytes[13] == 0x50 && bytes[14] == 0x38 && bytes[15] == 0x4C)
        {
            if (bytes.Length < 25)
            {
                return false;
            }

            var bits = bytes[21] | (bytes[22] << 8) | (bytes[23] << 16) | (bytes[24] << 24);
            width = (bits & 0x3FFF) + 1;
            height = ((bits >> 14) & 0x3FFF) + 1;
            return width > 0 && height > 0;
        }

        return false;
    }

    public int ReadInt16BigEndian(byte[] bytes, int offset)
    {
        return (bytes[offset] << 8) | bytes[offset + 1];
    }

    public int ReadInt16LittleEndian(byte[] bytes, int offset)
    {
        return bytes[offset] | (bytes[offset + 1] << 8);
    }

    public int ReadInt32BigEndian(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24)
            | (bytes[offset + 1] << 16)
            | (bytes[offset + 2] << 8)
            | bytes[offset + 3];
    }

    public bool TryResolveImagePartType(string extension, out PartTypeInfo imagePartType)
    {
        switch (extension)
        {
            case ".png":
                imagePartType = ImagePartType.Png;
                return true;
            case ".gif":
                imagePartType = ImagePartType.Gif;
                return true;
            case ".bmp":
                imagePartType = ImagePartType.Bmp;
                return true;
            case ".tiff":
                imagePartType = ImagePartType.Tiff;
                return true;
            case ".jpeg":
            case ".jpg":
                imagePartType = ImagePartType.Jpeg;
                return true;
            case ".webp":
                var webpProperty = typeof(ImagePartType).GetProperty(
                    "Webp",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                );
                if (webpProperty?.GetValue(null) is PartTypeInfo webpType)
                {
                    imagePartType = webpType;
                    return true;
                }

                break;
        }

        imagePartType = default;
        return false;
    }
}
