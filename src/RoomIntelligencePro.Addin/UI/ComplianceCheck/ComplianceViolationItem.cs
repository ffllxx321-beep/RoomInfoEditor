namespace RoomIntelligencePro.Addin.UI.ComplianceCheck;

public sealed class ComplianceViolationItem
{
    public required string RuleName { get; init; }
    public required string SeverityText { get; init; }
    public required string Message { get; init; }
    public required string SuggestedFix { get; init; }
}
