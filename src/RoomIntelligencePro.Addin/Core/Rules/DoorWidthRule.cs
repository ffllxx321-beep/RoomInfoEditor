namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class DoorWidthRule : IRule
{
    public const string Id = "RULE-DOOR-001";

    private readonly double _minimumDoorWidthMeters;

    public DoorWidthRule(double minimumDoorWidthMeters = 0.8)
    {
        _minimumDoorWidthMeters = minimumDoorWidthMeters;
    }

    public string RuleId => Id;
    public string RuleName => "Minimum Door Width";
    public RuleSeverity Severity => RuleSeverity.Warning;

    public RuleResult Evaluate(IReadOnlyList<RoomRuleSubject> rooms)
    {
        var violations = new List<RuleViolation>();

        foreach (var room in rooms)
        {
            if (room.DoorWidthsMeters.Count == 0)
            {
                violations.Add(new RuleViolation
                {
                    RuleId = RuleId,
                    RuleName = RuleName,
                    Severity = Severity,
                    RoomElementId = room.RoomElementId,
                    Message = $"Room '{room.RoomNumber}-{room.RoomName}' has no door width data.",
                    SuggestedFix = "Verify room door association and width parameters."
                });
                continue;
            }

            if (room.DoorWidthsMeters.Any(w => w < _minimumDoorWidthMeters))
            {
                violations.Add(new RuleViolation
                {
                    RuleId = RuleId,
                    RuleName = RuleName,
                    Severity = Severity,
                    RoomElementId = room.RoomElementId,
                    Message = $"Room '{room.RoomNumber}-{room.RoomName}' has door width below {_minimumDoorWidthMeters:0.##}m.",
                    SuggestedFix = "Check related door types and clear width values."
                });
            }
        }

        return new RuleResult
        {
            RuleId = RuleId,
            RuleName = RuleName,
            Violations = violations
        };
    }
}
