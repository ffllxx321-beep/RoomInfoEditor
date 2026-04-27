namespace RoomIntelligencePro.Addin.Core.DwgText;

public sealed class DwgTextCandidate
{
    public required string Text { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required string Layer { get; init; }

    public string Source => "CSV";
}
