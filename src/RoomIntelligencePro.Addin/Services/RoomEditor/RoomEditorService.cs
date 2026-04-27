using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.RoomEditor;

namespace RoomIntelligencePro.Addin.Services.RoomEditor;

public sealed class RoomEditorService
{
    private readonly IRoomEditorRepository _repository;

    public RoomEditorService(IRoomEditorRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<RoomEditorRoom> LoadRooms()
    {
        return _repository.LoadRooms();
    }

    public SaveRoomsResult SaveRooms(IReadOnlyList<RoomEditorRoom> rooms)
    {
        var changedRooms = rooms.Where(r => r.HasChanges).ToList();
        if (changedRooms.Count == 0)
        {
            return SaveRoomsResult.None;
        }

        return _repository.SaveRooms(changedRooms);
    }
}
