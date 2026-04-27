using Autodesk.Revit.DB;
using RoomIntelligencePro.Addin.Core.Contracts;
using RoomIntelligencePro.Addin.Core.MissingRooms;

namespace RoomIntelligencePro.Addin.RevitAdapter.MissingRooms;

public sealed class RevitMissingRoomRepository : IMissingRoomRepository
{
    private readonly Document _document;

    public RevitMissingRoomRepository(Document document)
    {
        _document = document;
    }

    public MissingRoomGenerationResult GenerateMissingRooms(MissingRoomGenerationOptions options)
    {
        var warnings = new List<string>();
        var previewCandidates = new List<MissingRoomCandidate>();

        var existingRoomSkippedCount = 0;
        var smallAreaSkippedCount = 0;
        var unclosedWarningCount = 0;

        var levels = new FilteredElementCollector(_document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        var circuitRefs = new List<(Level Level, PlanCircuit Circuit, MissingRoomCandidate Candidate)>();

        foreach (var level in levels)
        {
            var topology = _document.get_PlanTopology(level);
            if (topology is null)
            {
                continue;
            }

            var circuitIndex = 0;
            foreach (PlanCircuit circuit in topology.Circuits)
            {
                circuitIndex++;

                if (circuit.IsRoomLocated)
                {
                    existingRoomSkippedCount++;
                    continue;
                }

                var areaSquareMeters = UnitUtils.ConvertFromInternalUnits(circuit.Area, UnitTypeId.SquareMeters);

                if (areaSquareMeters <= 0)
                {
                    unclosedWarningCount++;
                    warnings.Add($"楼层“{level.Name}”回路 #{circuitIndex} 可能未闭合或无效（面积 {areaSquareMeters:0.###}㎡）。");
                    continue;
                }

                if (areaSquareMeters < options.MinimumAreaSquareMeters)
                {
                    smallAreaSkippedCount++;
                    continue;
                }

                var candidate = new MissingRoomCandidate
                {
                    LevelName = level.Name,
                    AreaSquareMeters = areaSquareMeters,
                    CircuitIndex = circuitIndex
                };

                previewCandidates.Add(candidate);
                circuitRefs.Add((level, circuit, candidate));
            }
        }

        if (options.PreviewOnly || circuitRefs.Count == 0)
        {
            return new MissingRoomGenerationResult
            {
                ExistingRoomSkippedCount = existingRoomSkippedCount,
                SmallAreaSkippedCount = smallAreaSkippedCount,
                UnclosedWarningCount = unclosedWarningCount,
                CreatedRoomCount = 0,
                PreviewCandidates = previewCandidates,
                Warnings = warnings
            };
        }

        var createdCount = 0;
        using var transaction = new Transaction(_document, "Room Intelligence Pro - Generate Missing Rooms");
        var status = transaction.Start();
        if (status != TransactionStatus.Started)
        {
            warnings.Add("无法启动缺失房间创建事务，请检查当前文档状态。");
            return new MissingRoomGenerationResult
            {
                ExistingRoomSkippedCount = existingRoomSkippedCount,
                SmallAreaSkippedCount = smallAreaSkippedCount,
                UnclosedWarningCount = unclosedWarningCount,
                CreatedRoomCount = 0,
                PreviewCandidates = previewCandidates,
                Warnings = warnings
            };
        }

        try
        {
            var phase = GetLatestPhase();

            foreach (var (_, circuit, candidate) in circuitRefs)
            {
                try
                {
                    var unplaced = _document.Create.NewRoom(phase);
                    _document.Create.NewRoom(unplaced, circuit);
                    createdCount++;
                }
                catch (Exception ex)
                {
                    warnings.Add($"楼层“{candidate.LevelName}”回路 #{candidate.CircuitIndex} 创建失败：{ex.Message}");
                }
            }

            var commitStatus = transaction.Commit();
            if (commitStatus != TransactionStatus.Committed)
            {
                warnings.Add("缺失房间创建事务未成功提交。");
            }
        }
        catch (Exception ex)
        {
            if (transaction.GetStatus() == TransactionStatus.Started)
            {
                transaction.RollBack();
            }

            warnings.Add($"缺失房间创建已中止：{ex.Message}");
        }

        return new MissingRoomGenerationResult
        {
            ExistingRoomSkippedCount = existingRoomSkippedCount,
            SmallAreaSkippedCount = smallAreaSkippedCount,
            UnclosedWarningCount = unclosedWarningCount,
            CreatedRoomCount = createdCount,
            PreviewCandidates = previewCandidates,
            Warnings = warnings
        };
    }

    private Phase GetLatestPhase()
    {
        if (_document.Phases.Size == 0)
        {
            throw new InvalidOperationException("当前文档缺少可用阶段（Phase），无法创建房间。");
        }

        return _document.Phases.Cast<Phase>().Last();
    }
}
