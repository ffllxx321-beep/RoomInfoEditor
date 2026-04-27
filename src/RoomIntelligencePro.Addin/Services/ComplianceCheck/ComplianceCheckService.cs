using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.Rules;

namespace RoomIntelligencePro.Addin.Services.ComplianceCheck;

public sealed class ComplianceCheckService
{
    private readonly IRoomRuleSubjectProvider _roomProvider;

    public ComplianceCheckService(IRoomRuleSubjectProvider roomProvider)
    {
        _roomProvider = roomProvider;
    }

    public RuleEngineResult Run()
    {
        var rooms = _roomProvider.LoadRooms();

        var engine = new RuleEngine([
            new AreaRule(),
            new RoomHeightRule(),
            new DoorWidthRule()
        ]);

        return engine.Run(rooms);
    }
}
