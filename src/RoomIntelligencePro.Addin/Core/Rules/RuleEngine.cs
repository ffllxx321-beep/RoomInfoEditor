namespace RoomIntelligencePro.Addin.Core.Rules;

public sealed class RuleEngine
{
    private readonly IReadOnlyList<IRule> _rules;

    public RuleEngine(IReadOnlyList<IRule> rules)
    {
        _rules = rules;
    }

    public RuleEngineResult Run(IReadOnlyList<RoomRuleSubject> rooms)
    {
        var results = _rules
            .Select(rule => rule.Evaluate(rooms))
            .ToList();

        return new RuleEngineResult
        {
            RuleResults = results
        };
    }
}
