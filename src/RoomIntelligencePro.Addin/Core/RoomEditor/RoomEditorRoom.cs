namespace RoomIntelligencePro.Addin.Core.RoomEditor;

public sealed class RoomEditorRoom
{
    public required int ElementId { get; init; }
    public required string LevelName { get; init; }
    public required string AreaSquareMetersText { get; init; }

    public required string Name { get; set; }
    public required string Number { get; set; }

    public string OriginalName { get; private set; } = string.Empty;
    public string OriginalNumber { get; private set; } = string.Empty;

    public bool HasChanges => !string.Equals(Name, OriginalName, StringComparison.Ordinal)
                              || !string.Equals(Number, OriginalNumber, StringComparison.Ordinal);

    public void CaptureOriginal()
    {
        OriginalName = Name;
        OriginalNumber = Number;
    }

    public void MarkSaved()
    {
        OriginalName = Name;
        OriginalNumber = Number;
    }
}
