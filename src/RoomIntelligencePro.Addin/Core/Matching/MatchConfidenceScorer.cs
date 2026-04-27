namespace RoomIntelligencePro.Addin.Core.Matching;

public sealed class MatchConfidenceScorer
{
    public double Score(
        bool insidePolygon,
        double distanceToCentroid,
        int duplicateCount,
        int candidateRoomCount,
        bool onBoundary = false,
        bool transformApplied = false)
    {
        var score = 1.0;

        if (!insidePolygon)
        {
            score -= 0.35;
        }

        var distancePenalty = Math.Min(0.35, distanceToCentroid / 50.0);
        score -= distancePenalty;

        if (duplicateCount > 1)
        {
            score -= Math.Min(0.2, (duplicateCount - 1) * 0.08);
        }

        if (candidateRoomCount > 1)
        {
            score -= Math.Min(0.25, (candidateRoomCount - 1) * 0.1);
        }

        if (onBoundary)
        {
            score -= 0.08;
        }

        if (transformApplied)
        {
            score -= 0.05;
        }

        return Math.Clamp(score, 0.0, 1.0);
    }
}
