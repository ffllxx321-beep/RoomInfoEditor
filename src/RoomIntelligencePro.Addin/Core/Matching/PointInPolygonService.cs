namespace RoomIntelligencePro.Addin.Core.Matching;

public sealed class PointInPolygonService
{
    public bool IsInside(
        IReadOnlyList<GeometryPoint> polygon,
        GeometryPoint point,
        double tolerance = 1e-6,
        bool includeBoundary = true)
    {
        var normalized = NormalizePolygon(polygon);
        if (normalized.Count < 3)
        {
            return false;
        }

        if (includeBoundary && IsOnBoundary(normalized, point, tolerance))
        {
            return true;
        }

        var inside = false;
        var j = normalized.Count - 1;

        for (var i = 0; i < normalized.Count; i++)
        {
            var pi = normalized[i];
            var pj = normalized[j];

            var intersects = ((pi.Y > point.Y) != (pj.Y > point.Y))
                             && (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X);

            if (intersects)
            {
                inside = !inside;
            }

            j = i;
        }

        return inside;
    }

    public bool IsOnBoundary(IReadOnlyList<GeometryPoint> polygon, GeometryPoint point, double tolerance = 1e-6)
    {
        var normalized = NormalizePolygon(polygon);
        if (normalized.Count < 2)
        {
            return false;
        }

        for (var i = 0; i < normalized.Count; i++)
        {
            var a = normalized[i];
            var b = normalized[(i + 1) % normalized.Count];
            if (DistancePointToSegment(point, a, b) <= tolerance)
            {
                return true;
            }
        }

        return false;
    }

    public double PolygonArea(IReadOnlyList<GeometryPoint> polygon)
    {
        var normalized = NormalizePolygon(polygon);
        if (normalized.Count < 3)
        {
            return 0;
        }

        var area = 0d;
        for (var i = 0; i < normalized.Count; i++)
        {
            var p1 = normalized[i];
            var p2 = normalized[(i + 1) % normalized.Count];
            area += (p1.X * p2.Y) - (p2.X * p1.Y);
        }

        return Math.Abs(area) * 0.5;
    }

    public double DistanceToCentroid(IReadOnlyList<GeometryPoint> polygon, GeometryPoint point)
    {
        var normalized = NormalizePolygon(polygon);
        if (normalized.Count == 0)
        {
            return double.MaxValue;
        }

        var centroid = GetCentroid(normalized);
        var dx = point.X - centroid.X;
        var dy = point.Y - centroid.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double DistanceToPolygon(IReadOnlyList<GeometryPoint> polygon, GeometryPoint point)
    {
        var normalized = NormalizePolygon(polygon);
        if (normalized.Count < 2)
        {
            return double.MaxValue;
        }

        var min = double.MaxValue;
        for (var i = 0; i < normalized.Count; i++)
        {
            var a = normalized[i];
            var b = normalized[(i + 1) % normalized.Count];
            var d = DistancePointToSegment(point, a, b);
            if (d < min)
            {
                min = d;
            }
        }

        return min;
    }

    private static GeometryPoint GetCentroid(IReadOnlyList<GeometryPoint> polygon)
    {
        if (polygon.Count == 0)
        {
            return new GeometryPoint(0, 0);
        }

        var signedArea = 0d;
        var cx = 0d;
        var cy = 0d;

        for (var i = 0; i < polygon.Count; i++)
        {
            var p0 = polygon[i];
            var p1 = polygon[(i + 1) % polygon.Count];
            var cross = (p0.X * p1.Y) - (p1.X * p0.Y);
            signedArea += cross;
            cx += (p0.X + p1.X) * cross;
            cy += (p0.Y + p1.Y) * cross;
        }

        if (Math.Abs(signedArea) < 1e-10)
        {
            var avgX = polygon.Average(p => p.X);
            var avgY = polygon.Average(p => p.Y);
            return new GeometryPoint(avgX, avgY);
        }

        var areaFactor = 1d / (3d * signedArea);
        return new GeometryPoint(cx * areaFactor, cy * areaFactor);
    }

    private static IReadOnlyList<GeometryPoint> NormalizePolygon(IReadOnlyList<GeometryPoint> polygon)
    {
        if (polygon.Count == 0)
        {
            return polygon;
        }

        if (polygon.Count > 1 && polygon[0].Equals(polygon[^1]))
        {
            return polygon.Take(polygon.Count - 1).ToList();
        }

        return polygon;
    }

    private static double DistancePointToSegment(GeometryPoint p, GeometryPoint a, GeometryPoint b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lengthSquared = (dx * dx) + (dy * dy);
        if (lengthSquared <= 1e-12)
        {
            var px = p.X - a.X;
            var py = p.Y - a.Y;
            return Math.Sqrt((px * px) + (py * py));
        }

        var t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
        t = Math.Clamp(t, 0, 1);

        var projX = a.X + (t * dx);
        var projY = a.Y + (t * dy);
        var distX = p.X - projX;
        var distY = p.Y - projY;
        return Math.Sqrt((distX * distX) + (distY * distY));
    }
}
