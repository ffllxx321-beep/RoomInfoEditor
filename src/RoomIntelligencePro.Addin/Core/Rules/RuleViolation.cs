namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RuleViolation
{
    public required string RuleId { get; init; }
    public required string RuleName { get; init; }
    public required RuleSeverity Severity { get; init; }
    public required int RoomElementId { get; init; }
    public required string Message { get; init; }
    public required string SuggestedFix { get; init; }
}
