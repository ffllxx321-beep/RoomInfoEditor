using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.Rules;

namespace RoomIntelligencePro.Addin.RevitAdapter.Rules;

public sealed class RevitRoomRuleSubjectProvider : IRoomRuleSubjectProvider
{
    private readonly Document _document;

    public RevitRoomRuleSubjectProvider(Document document)
    {
        _document = document;
    }

    public IReadOnlyList<RoomRuleSubject> LoadRooms()
    {
        var phase = GetLatestPhaseOrNull();
        var roomDoors = BuildRoomDoorWidthMap(phase);

        var rooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>()
            .Select(room =>
            {
                var number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString() ?? string.Empty;
                var name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? string.Empty;
                var area = UnitUtils.ConvertFromInternalUnits(room.Area, UnitTypeId.SquareMeters);
                var height = ResolveRoomHeightMeters(room);
                roomDoors.TryGetValue(ToIntElementIdValue(room.Id), out var doorWidths);

                return new RoomRuleSubject
                {
                    RoomElementId = ToIntElementIdValue(room.Id),
                    RoomNumber = number,
                    RoomName = name,
                    AreaSquareMeters = area,
                    HeightMeters = height,
                    DoorWidthsMeters = doorWidths ?? []
                };
            })
            .OrderBy(r => r.RoomNumber, StringComparer.Ordinal)
            .ToList();

        return rooms;
    }

    private Dictionary<int, IReadOnlyList<double>> BuildRoomDoorWidthMap(Phase? phase)
    {
        var map = new Dictionary<int, List<double>>();

        var doors = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>();

        foreach (var door in doors)
        {
            var width = ResolveDoorWidthMeters(door);
            if (width <= 0)
            {
                continue;
            }

            AddDoorToRoom(map, ResolveDoorToRoom(door, phase), width);
            AddDoorToRoom(map, ResolveDoorFromRoom(door, phase), width);
        }

        return map.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<double>)kvp.Value);
    }

    private static void AddDoorToRoom(Dictionary<int, List<double>> map, Room? room, double width)
    {
        if (room is null)
        {
            return;
        }

        var roomElementId = ToIntElementIdValue(room.Id);
        if (!map.TryGetValue(roomElementId, out var widths))
        {
            widths = [];
            map[roomElementId] = widths;
        }

        widths.Add(width);
    }

    private static double ResolveDoorWidthMeters(FamilyInstance door)
    {
        var widthParameter = door.Symbol.get_Parameter(BuiltInParameter.DOOR_WIDTH)
            ?? door.get_Parameter(BuiltInParameter.DOOR_WIDTH)
            ?? door.Symbol.LookupParameter("Width")
            ?? door.LookupParameter("Width");

        if (widthParameter is null || widthParameter.StorageType != StorageType.Double)
        {
            return 0;
        }

        return UnitUtils.ConvertFromInternalUnits(widthParameter.AsDouble(), UnitTypeId.Meters);
    }

    private static double ResolveRoomHeightMeters(Room room)
    {
        var upperOffset = room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET)?.AsDouble() ?? 0;

        var limit = room.UpperLimit;
        var level = room.Level;
        if (limit is null || level is null)
        {
            return 0;
        }

        var heightInternal = Math.Max(0, (limit.Elevation - level.Elevation) + upperOffset);
        return UnitUtils.ConvertFromInternalUnits(heightInternal, UnitTypeId.Meters);
    }

    private Phase? GetLatestPhaseOrNull()
    {
        return _document.Phases.Size == 0
            ? null
            : _document.Phases.Cast<Phase>().Last();
    }

    private static Room? ResolveDoorToRoom(FamilyInstance door, Phase? phase)
    {
        return phase is null ? door.ToRoom : door.get_ToRoom(phase);
    }

    private static Room? ResolveDoorFromRoom(FamilyInstance door, Phase? phase)
    {
        return phase is null ? door.FromRoom : door.get_FromRoom(phase);
    }

    private static int ToIntElementIdValue(ElementId elementId)
    {
        return checked((int)elementId.Value);
    }
}
