namespace RoomIntelligencePro.Addin.Core.Matching;

public sealed class RoomMatchCandidate
{
    public required int RoomElementId { get; init; }
    public required IReadOnlyList<GeometryPoint> Polygon { get; init; }

    public GeometryPoint Centroid => ComputeCentroid(Polygon);

    private static GeometryPoint ComputeCentroid(IReadOnlyList<GeometryPoint> polygon)
    {
        if (polygon.Count == 0)
        {
            return new GeometryPoint(0, 0);
        }

        var sumX = 0d;
        var sumY = 0d;
        foreach (var p in polygon)
        {
            sumX += p.X;
            sumY += p.Y;
        }

        return new GeometryPoint(sumX / polygon.Count, sumY / polygon.Count);
    }
}
