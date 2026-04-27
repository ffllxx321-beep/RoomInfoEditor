using RoomIntelligencePro.Addin.Core.DwgText;

namespace RoomIntelligencePro.Addin.Core.Matching;

public sealed class RoomNameMatcher
{
    private readonly PointInPolygonService _pointInPolygonService;
    private readonly MatchConfidenceScorer _scorer;

    public RoomNameMatcher(PointInPolygonService pointInPolygonService, MatchConfidenceScorer scorer)
    {
        _pointInPolygonService = pointInPolygonService;
        _scorer = scorer;
    }

    public IReadOnlyList<RoomNameMatchResult> Match(
        IReadOnlyList<RoomMatchCandidate> rooms,
        IReadOnlyList<DwgTextCandidate> texts,
        DwgCoordinateTransform? coordinateTransform = null,
        Action<int, int>? progressCallback = null)
    {
        if (rooms.Count == 0 || texts.Count == 0)
        {
            return texts.Select(t => new RoomNameMatchResult
            {
                Status = MatchStatus.Unmatched,
                TextCandidate = t,
                RoomElementId = null,
                Confidence = 0,
                Reason = "当前模型无可匹配房间。"
            }).ToList();
        }

        var results = new List<RoomNameMatchResult>(texts.Count);
        var textGroups = texts
            .GroupBy(t => Normalize(t.Text), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        var roomSpatialIndex = rooms
            .Select(r => new
            {
                Room = r,
                Box = BuildBoundingBox(r.Polygon),
                Area = _pointInPolygonService.PolygonArea(r.Polygon)
            })
            .ToList();
        var roomAreaMap = roomSpatialIndex.ToDictionary(x => x.Room.RoomElementId, x => x.Area);

        var roomBestScore = new Dictionary<int, double>();
        var roomBestResultIndex = new Dictionary<int, int>();

        foreach (var text in texts)
        {
            progressCallback?.Invoke(results.Count, texts.Count);

            var point = coordinateTransform is null
                ? new GeometryPoint(text.X, text.Y)
                : coordinateTransform.Apply(text.X, text.Y);
            var insideCandidates = roomSpatialIndex
                .Where(r => PointInBox(r.Box, point))
                .Select(r => r.Room)
                .Where(r => _pointInPolygonService.IsInside(r.Polygon, point))
                .ToList();

            var duplicateCount = textGroups.TryGetValue(Normalize(text.Text), out var count) ? count : 1;
            var transformApplied = coordinateTransform is not null;

            if (insideCandidates.Count == 0)
            {
                var nearest = rooms
                    .Select(r => new
                    {
                        Room = r,
                        Distance = _pointInPolygonService.DistanceToPolygon(r.Polygon, point),
                        CentroidDistance = _pointInPolygonService.DistanceToCentroid(r.Polygon, point)
                    })
                    .OrderBy(x => x.Distance)
                    .ThenBy(x => x.CentroidDistance)
                    .First();

                var nearestScore = _scorer.Score(
                    insidePolygon: false,
                    distanceToCentroid: nearest.CentroidDistance,
                    duplicateCount: duplicateCount,
                    candidateRoomCount: 0,
                    transformApplied: transformApplied);

                results.Add(new RoomNameMatchResult
                {
                    Status = MatchStatus.Unmatched,
                    TextCandidate = text,
                    RoomElementId = nearest.Room.RoomElementId,
                    Confidence = nearestScore,
                    Reason = "文字点未落入任何房间边界，已给出最近邻建议。"
                });
                continue;
            }

            if (insideCandidates.Count > 1)
            {
                var top = insideCandidates
                    .Select(r => new
                    {
                        Room = r,
                        Area = roomAreaMap[r.RoomElementId],
                        Distance = _pointInPolygonService.DistanceToCentroid(r.Polygon, point)
                    })
                    .OrderBy(x => x.Area)
                    .ThenBy(x => x.Distance)
                    .First();

                var conflictScore = _scorer.Score(
                    insidePolygon: true,
                    distanceToCentroid: top.Distance,
                    duplicateCount: duplicateCount,
                    candidateRoomCount: insideCandidates.Count,
                    onBoundary: insideCandidates.Any(c => _pointInPolygonService.IsOnBoundary(c.Polygon, point)),
                    transformApplied: transformApplied);

                results.Add(new RoomNameMatchResult
                {
                    Status = MatchStatus.Conflict,
                    TextCandidate = text,
                    RoomElementId = top.Room.RoomElementId,
                    Confidence = conflictScore,
                    Reason = $"文字点落入 {insideCandidates.Count} 个房间边界，存在冲突。"
                });
                continue;
            }

            var matchedRoom = insideCandidates[0];
            var distance = _pointInPolygonService.DistanceToCentroid(matchedRoom.Polygon, point);
            var score = _scorer.Score(
                insidePolygon: true,
                distanceToCentroid: distance,
                duplicateCount: duplicateCount,
                candidateRoomCount: 1,
                onBoundary: _pointInPolygonService.IsOnBoundary(matchedRoom.Polygon, point),
                transformApplied: transformApplied);

            if (roomBestScore.TryGetValue(matchedRoom.RoomElementId, out var bestScore))
            {
                if (score <= bestScore)
                {
                    results.Add(new RoomNameMatchResult
                    {
                        Status = MatchStatus.Conflict,
                        TextCandidate = text,
                        RoomElementId = matchedRoom.RoomElementId,
                        Confidence = score,
                        Reason = "该房间已有更高置信度的候选文字。"
                    });
                    continue;
                }

                var previousIndex = roomBestResultIndex[matchedRoom.RoomElementId];
                var previous = results[previousIndex];
                results[previousIndex] = new RoomNameMatchResult
                {
                    Status = MatchStatus.Conflict,
                    TextCandidate = previous.TextCandidate,
                    RoomElementId = previous.RoomElementId,
                    Confidence = previous.Confidence,
                    Reason = "已被更高置信度候选覆盖。"
                };
            }

            roomBestScore[matchedRoom.RoomElementId] = score;
            roomBestResultIndex[matchedRoom.RoomElementId] = results.Count;
            results.Add(new RoomNameMatchResult
            {
                Status = MatchStatus.Matched,
                TextCandidate = text,
                RoomElementId = matchedRoom.RoomElementId,
                Confidence = score,
                Reason = "已通过点在多边形内算法完成匹配。"
            });
        }

        progressCallback?.Invoke(texts.Count, texts.Count);
        return results;
    }

    private static string Normalize(string text)
    {
        return (text ?? string.Empty).Trim();
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) BuildBoundingBox(IReadOnlyList<GeometryPoint> polygon)
    {
        if (polygon.Count == 0)
        {
            return (double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var point in polygon)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }

        return (minX, minY, maxX, maxY);
    }

    private static bool PointInBox((double MinX, double MinY, double MaxX, double MaxY) box, GeometryPoint point)
    {
        return point.X >= box.MinX
               && point.X <= box.MaxX
               && point.Y >= box.MinY
               && point.Y <= box.MaxY;
    }
}
