using RoomIntelligencePro.Addin.Core.MissingRooms;

namespace RoomIntelligencePro.Addin.Core.Contracts;

public interface IMissingRoomRepository
{
    MissingRoomGenerationResult GenerateMissingRooms(MissingRoomGenerationOptions options);
}
