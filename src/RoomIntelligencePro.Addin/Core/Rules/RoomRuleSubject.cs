namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RoomRuleSubject
{
    public required int RoomElementId { get; init; }
    public required string RoomNumber { get; init; }
    public required string RoomName { get; init; }
    public required double AreaSquareMeters { get; init; }
    public required double HeightMeters { get; init; }
    public required IReadOnlyList<double> DoorWidthsMeters { get; init; }
}
