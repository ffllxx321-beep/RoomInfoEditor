using System.Collections.ObjectModel;
using RoomIntelligencePro.Addin.Core.Rules;
using RoomIntelligencePro.Addin.Services.ComplianceCheck;

namespace RoomIntelligencePro.Addin.UI.ComplianceCheck;

public sealed class ComplianceCheckViewModel
{
    private readonly ComplianceCheckService _service;

    public ComplianceCheckViewModel(ComplianceCheckService service)
    {
        _service = service;
    }

    public ObservableCollection<ComplianceViolationItem> Violations { get; } = [];

    public int TotalViolations => Violations.Count;

    public int ErrorCount => Violations.Count(v => v.SeverityText == "Error");

    public int WarningCount => Violations.Count(v => v.SeverityText == "Warning");

    public void Load()
    {
        Violations.Clear();

        var result = _service.Run();
        foreach (var violation in result.Violations
                     .OrderByDescending(v => v.Severity)
                     .ThenBy(v => v.RuleName, StringComparer.Ordinal)
                     .ThenBy(v => v.RoomElementId))
        {
            Violations.Add(new ComplianceViolationItem
            {
                RuleName = violation.RuleName,
                SeverityText = MapSeverity(violation.Severity),
                Message = violation.Message,
                SuggestedFix = violation.SuggestedFix
            });
        }
    }

    private static string MapSeverity(RuleSeverity severity)
    {
        return severity switch
        {
            RuleSeverity.Error => "Error",
            RuleSeverity.Warning => "Warning",
            _ => "Info"
        };
    }
}
