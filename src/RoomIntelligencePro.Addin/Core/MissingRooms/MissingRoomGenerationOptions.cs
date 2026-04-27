namespace RoomIntelligencePro.Addin.Core.MissingRooms;

public sealed class MissingRoomGenerationOptions
{
    public double MinimumAreaSquareMeters { get; init; } = 1.0;
    public bool PreviewOnly { get; init; } = true;
}
