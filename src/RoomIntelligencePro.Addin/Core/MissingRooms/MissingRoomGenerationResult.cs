namespace RoomIntelligencePro.Addin.Core.MissingRooms;

public sealed class MissingRoomGenerationResult
{
    public int ExistingRoomSkippedCount { get; init; }
    public int SmallAreaSkippedCount { get; init; }
    public int UnclosedWarningCount { get; init; }
    public int CreatedRoomCount { get; init; }
    public required IReadOnlyList<MissingRoomCandidate> PreviewCandidates { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
}
