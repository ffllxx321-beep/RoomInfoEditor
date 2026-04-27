using RoomIntelligencePro.Addin.Core.DwgText;

namespace RoomIntelligencePro.Addin.Core.Contracts;

public interface IDwgTextExtractor
{
    IReadOnlyList<DwgTextCandidate> Extract(string csvFilePath);
}
