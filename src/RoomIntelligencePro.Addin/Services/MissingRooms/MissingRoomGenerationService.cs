using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.MissingRooms;

namespace RoomIntelligencePro.Addin.Services.MissingRooms;

public sealed class MissingRoomGenerationService
{
    private readonly IMissingRoomRepository _repository;

    public MissingRoomGenerationService(IMissingRoomRepository repository)
    {
        _repository = repository;
    }

    public MissingRoomGenerationResult Execute(MissingRoomGenerationOptions options)
    {
        return _repository.GenerateMissingRooms(options);
    }
}
