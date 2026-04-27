using RoomIntelligencePro.Addin.Core.RoomEditor;

namespace RoomIntelligencePro.Addin.Core.Contracts;

public interface IRoomEditorRepository
{
    IReadOnlyList<RoomEditorRoom> LoadRooms();
    SaveRoomsResult SaveRooms(IReadOnlyList<RoomEditorRoom> rooms);
}
