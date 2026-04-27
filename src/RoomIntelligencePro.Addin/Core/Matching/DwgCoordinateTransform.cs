namespace RoomIntelligencePro.Addin.Core.Matching;

public sealed class DwgCoordinateTransform
{
    public double OffsetX { get; init; }
    public double OffsetY { get; init; }
    public double RotationDegrees { get; init; }

    public GeometryPoint Apply(double x, double y)
    {
        var radians = RotationDegrees * (Math.PI / 180.0);
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var rotatedX = (x * cos) - (y * sin);
        var rotatedY = (x * sin) + (y * cos);

        return new GeometryPoint(rotatedX + OffsetX, rotatedY + OffsetY);
    }
}
