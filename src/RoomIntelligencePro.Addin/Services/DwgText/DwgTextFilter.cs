using System.Text.RegularExpressions;
using RoomIntelligencePro.Addin.Core.DwgText;

namespace RoomIntelligencePro.Addin.Services.DwgText;

public sealed class DwgTextFilter
{
    private static readonly Regex DimensionPattern = new(@"^\d+(\.\d+)?(mm|cm|m)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AxisPattern = new(@"^[A-Z]{1,2}-?\d{1,3}$|^\d{1,3}-?[A-Z]{1,2}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DoorWindowPattern = new(@"^(D|W|M|C)[-_]?\d{1,4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string[] NoteKeywords =
    {
        "说明", "做法", "备注", "详图", "索引", "参见", "详见", "大样", "节点", "比例", "NTS"
    };

    public IReadOnlyList<DwgTextCandidate> Filter(IReadOnlyList<DwgTextCandidate> source)
    {
        if (source.Count == 0)
        {
            return source;
        }

        var filtered = new List<DwgTextCandidate>(source.Count);

        foreach (var item in source)
        {
            if (ShouldExclude(item.Text))
            {
                continue;
            }

            filtered.Add(item);
        }

        return filtered;
    }

    public bool ShouldExclude(string text)
    {
        var normalized = (text ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            return true;
        }

        if (DimensionPattern.IsMatch(normalized))
        {
            return true;
        }

        if (AxisPattern.IsMatch(normalized))
        {
            return true;
        }

        if (DoorWindowPattern.IsMatch(normalized))
        {
            return true;
        }

        return NoteKeywords.Any(k => normalized.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
