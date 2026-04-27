namespace RoomIntelligencePro.Addin.Core.RoomEditor;

public sealed class SaveRoomsResult
{
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public static SaveRoomsResult None => new();
}
