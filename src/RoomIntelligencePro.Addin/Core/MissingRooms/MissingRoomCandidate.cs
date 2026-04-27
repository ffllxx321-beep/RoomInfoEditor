namespace RoomIntelligencePro.Addin.Core.MissingRooms;

public sealed class MissingRoomCandidate
{
    public required string LevelName { get; init; }
    public required double AreaSquareMeters { get; init; }
    public required int CircuitIndex { get; init; }
}
