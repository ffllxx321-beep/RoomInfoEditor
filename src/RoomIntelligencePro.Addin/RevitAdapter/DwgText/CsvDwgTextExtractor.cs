using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.DwgText;

namespace RoomIntelligencePro.Addin.RevitAdapter.DwgText;

public sealed class CsvDwgTextExtractor : IDwgTextExtractor
{
    public IReadOnlyList<DwgTextCandidate> Extract(string csvFilePath)
    {
        if (string.IsNullOrWhiteSpace(csvFilePath))
        {
            throw new ArgumentException("请选择有效的 CSV 文件路径。", nameof(csvFilePath));
        }

        if (!System.IO.File.Exists(csvFilePath))
        {
            throw new System.IO.FileNotFoundException("未找到 CSV 文件，请检查文件路径。", csvFilePath);
        }

        using var parser = new TextFieldParser(csvFilePath)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            return Array.Empty<DwgTextCandidate>();
        }

        var header = parser.ReadFields();
        ValidateHeader(header);

        var results = new List<DwgTextCandidate>();
        var rowNumber = 1;

        while (!parser.EndOfData)
        {
            rowNumber++;
            var fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            if (fields.Length < 4)
            {
                throw new FormatException($"第 {rowNumber} 行格式错误：应包含 4 列（Text,X,Y,Layer）。");
            }

            var text = (fields[0] ?? string.Empty).Trim();
            var layer = (fields[3] ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!double.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
            {
                throw new FormatException($"第 {rowNumber} 行 X 坐标无效：'{fields[1]}'。");
            }

            if (!double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                throw new FormatException($"第 {rowNumber} 行 Y 坐标无效：'{fields[2]}'。");
            }

            results.Add(new DwgTextCandidate
            {
                Text = text,
                X = x,
                Y = y,
                Layer = layer
            });
        }

        return results;
    }

    private static void ValidateHeader(string[]? header)
    {
        if (header is null || header.Length < 4)
        {
            throw new FormatException("CSV 表头必须为：Text,X,Y,Layer。");
        }

        var expected = new[] { "Text", "X", "Y", "Layer" };
        for (var i = 0; i < expected.Length; i++)
        {
            if (!string.Equals(header[i]?.Trim(), expected[i], StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("CSV 表头必须为：Text,X,Y,Layer。");
            }
        }
    }
}
