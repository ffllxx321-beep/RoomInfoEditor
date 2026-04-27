namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RuleResult
{
    public required string RuleId { get; init; }
    public required string RuleName { get; init; }
    public required IReadOnlyList<RuleViolation> Violations { get; init; }

    public bool HasViolations => Violations.Count > 0;
}
