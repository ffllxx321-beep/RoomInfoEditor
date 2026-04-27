namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RoomHeightRule : IRule
{
    public const string Id = "RULE-HEIGHT-001";

    private readonly double _minimumHeightMeters;

    public RoomHeightRule(double minimumHeightMeters = 2.2)
    {
        _minimumHeightMeters = minimumHeightMeters;
    }

    public string RuleId => Id;
    public string RuleName => "Minimum Room Height";
    public RuleSeverity Severity => RuleSeverity.Error;

    public RuleResult Evaluate(IReadOnlyList<RoomRuleSubject> rooms)
    {
        var violations = rooms
            .Where(r => r.HeightMeters < _minimumHeightMeters)
            .Select(r => new RuleViolation
            {
                RuleId = RuleId,
                RuleName = RuleName,
                Severity = Severity,
                RoomElementId = r.RoomElementId,
                Message = $"Room '{r.RoomNumber}-{r.RoomName}' height is {r.HeightMeters:0.##}m, below {_minimumHeightMeters:0.##}m.",
                SuggestedFix = "Review upper limit / computation height settings."
            })
            .ToList();

        return new RuleResult
        {
            RuleId = RuleId,
            RuleName = RuleName,
            Violations = violations
        };
    }
}
