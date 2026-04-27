namespace RoomIntelligencePro.Addin.Core.Rules;

public interface IRule
{
    string RuleId { get; }
    string RuleName { get; }
    RuleSeverity Severity { get; }

    RuleResult Evaluate(IReadOnlyList<RoomRuleSubject> rooms);
}
