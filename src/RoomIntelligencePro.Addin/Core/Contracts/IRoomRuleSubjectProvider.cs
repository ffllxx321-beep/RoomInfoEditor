using RoomIntelligencePro.Addin.Core.Rules;

namespace RoomIntelligencePro.Addin.Core.Contracts;

public interface IRoomRuleSubjectProvider
{
    IReadOnlyList<RoomRuleSubject> LoadRooms();
}
