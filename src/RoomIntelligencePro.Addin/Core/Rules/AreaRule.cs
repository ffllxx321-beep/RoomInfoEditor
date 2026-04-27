namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class AreaRule : IRule
{
    public const string Id = "RULE-AREA-001";

    private readonly double _minimumAreaSquareMeters;

    public AreaRule(double minimumAreaSquareMeters = 1.0)
    {
        _minimumAreaSquareMeters = minimumAreaSquareMeters;
    }

    public string RuleId => Id;
    public string RuleName => "Minimum Room Area";
    public RuleSeverity Severity => RuleSeverity.Error;

    public RuleResult Evaluate(IReadOnlyList<RoomRuleSubject> rooms)
    {
        var violations = rooms
            .Where(r => r.AreaSquareMeters < _minimumAreaSquareMeters)
            .Select(r => new RuleViolation
            {
                RuleId = RuleId,
                RuleName = RuleName,
                Severity = Severity,
                RoomElementId = r.RoomElementId,
                Message = $"Room '{r.RoomNumber}-{r.RoomName}' area is {r.AreaSquareMeters:0.##}㎡, below {_minimumAreaSquareMeters:0.##}㎡.",
                SuggestedFix = "Check room boundaries and area parameters."
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
