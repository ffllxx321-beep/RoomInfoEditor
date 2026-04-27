namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RuleEngineResult
{
    public required IReadOnlyList<RuleResult> RuleResults { get; init; }

    public IReadOnlyList<RuleViolation> Violations => RuleResults
        .SelectMany(r => r.Violations)
        .ToList();
}
