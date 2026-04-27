using RoomIntelligencePro.Addin.Core.DwgText;

namespace RoomIntelligencePro.Addin.Core.Matching;

public enum MatchStatus
{
    Matched,
    Conflict,
    Unmatched
}

public sealed class RoomNameMatchResult
{
    public required MatchStatus Status { get; init; }
    public required DwgTextCandidate TextCandidate { get; init; }
    public int? RoomElementId { get; init; }
    public required double Confidence { get; init; }
    public required string Reason { get; init; }
}
