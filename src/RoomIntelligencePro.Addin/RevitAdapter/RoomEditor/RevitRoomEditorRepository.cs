using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.RoomEditor;

namespace RoomIntelligencePro.Addin.RevitAdapter.RoomEditor;

public sealed class RevitRoomEditorRepository : IRoomEditorRepository
{
    private const int SaveBatchSize = 200;
    private readonly Document _document;

    public RevitRoomEditorRepository(Document document)
    {
        _document = document;
    }

    public IReadOnlyList<RoomEditorRoom> LoadRooms()
    {
        var collector = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

        var levelNames = new Dictionary<ElementId, string>();
        var rooms = new List<RoomEditorRoom>();

        foreach (var element in collector)
        {
            if (element is not Room room)
            {
                continue;
            }

            if (!levelNames.TryGetValue(room.LevelId, out var levelName))
            {
                var level = _document.GetElement(room.LevelId) as Level;
                levelName = level?.Name ?? "(无楼层)";
                levelNames[room.LevelId] = levelName;
            }

            var areaText = UnitUtils.ConvertFromInternalUnits(room.Area, UnitTypeId.SquareMeters)
                .ToString("0.##");

            var name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? string.Empty;
            var number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString() ?? string.Empty;

            rooms.Add(new RoomEditorRoom
            {
                ElementId = ToIntElementIdValue(room.Id),
                LevelName = levelName,
                AreaSquareMetersText = areaText,
                Name = name,
                Number = number
            });
        }

        foreach (var room in rooms)
        {
            room.CaptureOriginal();
        }

        return rooms
            .OrderBy(r => r.LevelName, StringComparer.Ordinal)
            .ThenBy(r => r.Number, StringComparer.Ordinal)
            .ToList();
    }

    public SaveRoomsResult SaveRooms(IReadOnlyList<RoomEditorRoom> rooms)
    {
        if (rooms.Count == 0)
        {
            return SaveRoomsResult.None;
        }

        var validationErrors = ValidateRooms(rooms);
        if (validationErrors.Count > 0)
        {
            return new SaveRoomsResult
            {
                UpdatedCount = 0,
                SkippedCount = rooms.Count,
                ValidationErrors = validationErrors
            };
        }

        var updatedCount = 0;
        var skippedCount = 0;

        try
        {
            var roomIds = rooms.Select(r => new ElementId(r.ElementId)).ToList();
            var roomMap = new FilteredElementCollector(_document, roomIds)
                .WhereElementIsNotElementType()
                .OfType<Room>()
                .ToDictionary(r => ToIntElementIdValue(r.Id), r => r);

            foreach (var batch in Batch(rooms, SaveBatchSize))
            {
                using var transaction = new Transaction(_document, "Room Intelligence Pro - Save Room Editor Batch");
                if (transaction.Start() != TransactionStatus.Started)
                {
                    return new SaveRoomsResult
                    {
                        UpdatedCount = updatedCount,
                        SkippedCount = skippedCount + batch.Count,
                        ValidationErrors = ["无法启动保存事务，请检查模型编辑状态。"]
                    };
                }

                foreach (var row in batch)
                {
                    if (!roomMap.TryGetValue(row.ElementId, out var element))
                    {
                        skippedCount++;
                        continue;
                    }

                    var nameParameter = element.get_Parameter(BuiltInParameter.ROOM_NAME);
                    var numberParameter = element.get_Parameter(BuiltInParameter.ROOM_NUMBER);

                    var rowUpdated = false;

                    if (nameParameter is not null &&
                        !nameParameter.IsReadOnly &&
                        !string.Equals(row.Name, row.OriginalName, StringComparison.Ordinal))
                    {
                        nameParameter.Set(row.Name.Trim());
                        rowUpdated = true;
                    }

                    if (numberParameter is not null &&
                        !numberParameter.IsReadOnly &&
                        !string.Equals(row.Number, row.OriginalNumber, StringComparison.Ordinal))
                    {
                        numberParameter.Set(row.Number.Trim());
                        rowUpdated = true;
                    }

                    if (!rowUpdated)
                    {
                        skippedCount++;
                        continue;
                    }

                    updatedCount++;
                    row.MarkSaved();
                }

                if (transaction.Commit() != TransactionStatus.Committed)
                {
                    if (transaction.GetStatus() == TransactionStatus.Started)
                    {
                        transaction.RollBack();
                    }

                    return new SaveRoomsResult
                    {
                        UpdatedCount = updatedCount,
                        SkippedCount = skippedCount,
                        ValidationErrors = ["保存事务提交失败，已回滚该批次修改。"]
                    };
                }
            }

            return new SaveRoomsResult
            {
                UpdatedCount = updatedCount,
                SkippedCount = skippedCount
            };
        }
        catch (Exception ex)
        {
            return new SaveRoomsResult
            {
                UpdatedCount = 0,
                SkippedCount = rooms.Count,
                ValidationErrors = [$"保存失败：{ex.Message}"]
            };
        }
    }

    private List<string> ValidateRooms(IReadOnlyList<RoomEditorRoom> rooms)
    {
        var errors = new List<string>();
        var existingNumbers = GetCurrentRoomNumbers();

        foreach (var room in rooms)
        {
            var newName = (room.Name ?? string.Empty).Trim();
            var newNumber = (room.Number ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                errors.Add($"房间 {room.ElementId} 名称不能为空。");
            }

            if (string.IsNullOrWhiteSpace(newNumber))
            {
                errors.Add($"房间 {room.ElementId} 编号不能为空。");
                continue;
            }

            var duplicateCount = existingNumbers.TryGetValue(newNumber, out var count) ? count : 0;
            if (!string.Equals(room.OriginalNumber, newNumber, StringComparison.Ordinal))
            {
                if (duplicateCount > 0)
                {
                    errors.Add($"房间 {room.ElementId} 编号 '{newNumber}' 已存在。");
                }
            }
        }

        return errors;
    }

    private Dictionary<string, int> GetCurrentRoomNumbers()
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);

        var collector = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

        foreach (var element in collector)
        {
            if (element is not Room room)
            {
                continue;
            }

            var number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString();
            if (string.IsNullOrWhiteSpace(number))
            {
                continue;
            }

            var normalized = number.Trim();
            map.TryGetValue(normalized, out var count);
            map[normalized] = count + 1;
        }

        return map;
    }

    private static IReadOnlyList<IReadOnlyList<RoomEditorRoom>> Batch(IReadOnlyList<RoomEditorRoom> source, int size)
    {
        var batches = new List<IReadOnlyList<RoomEditorRoom>>();
        for (var i = 0; i < source.Count; i += size)
        {
            var count = Math.Min(size, source.Count - i);
            batches.Add(source.Skip(i).Take(count).ToList());
        }

        return batches;
    }

    private static int ToIntElementIdValue(ElementId elementId)
    {
        // Revit 2024+ uses ElementId.Value (long). Keep explicit checked cast for existing int-based domain model.
        return checked((int)elementId.Value);
    }
}
