using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RoomIntelligencePro.Addin.RevitAdapter.MissingRooms;
using RoomIntelligencePro.Addin.RevitAdapter.RoomEditor;
using RoomIntelligencePro.Addin.Services.RoomEditor;
using RoomIntelligencePro.Addin.UI.RoomEditor;
using System.Windows;

namespace RoomIntelligencePro.Addin.UI.Workflow;

public partial class WorkflowHubWindow : Window
{
    private readonly UIDocument _uiDocument;

    public WorkflowHubWindow(UIDocument uiDocument)
    {
        InitializeComponent();
        _uiDocument = uiDocument;
    }

    private Document Document => _uiDocument.Document;

    private void OnImportGuideClick(object sender, RoutedEventArgs e)
    {
        TaskDialog.Show("Workflow Hub", "请在“数据管理”中导入 CSV（Text,X,Y,Layer）。\n建议先检查表头和坐标单位。\n\n下一步：完成导入后执行匹配。");
    }

    private void OnOpenRoomEditorClick(object sender, RoutedEventArgs e)
    {
        var repository = new RevitRoomEditorRepository(Document);
        var service = new RoomEditorService(repository);
        var viewModel = new RoomEditorViewModel(service);
        viewModel.Initialize();

        var editor = new RoomEditorWindow(viewModel);
        editor.ShowDialog();
    }

    private void OnPreviewMissingRoomsClick(object sender, RoutedEventArgs e)
    {
        var repository = new RevitMissingRoomRepository(Document);
        var result = repository.GenerateMissingRooms(new Core.MissingRooms.MissingRoomGenerationOptions
        {
            PreviewOnly = true,
            MinimumAreaSquareMeters = 1.0
        });

        TaskDialog.Show(
            "Workflow Hub",
            $"Preview 完成：\n可创建房间：{result.PreviewCandidates.Count}\n已存在跳过：{result.ExistingRoomSkippedCount}\n小面积跳过：{result.SmallAreaSkippedCount}\n未闭合警告：{result.UnclosedWarningCount}");
    }

    private void OnMatchingGuideClick(object sender, RoutedEventArgs e)
    {
        TaskDialog.Show("Workflow Hub", "匹配策略：点在多边形优先，最近邻兜底，并输出置信度。\n建议先处理冲突项，再批量写回。\n\n下一步：执行规范审核。");
    }

    private void OnRuleGuideClick(object sender, RoutedEventArgs e)
    {
        TaskDialog.Show("Workflow Hub", "当前已包含面积、高度、门宽规则。\n建议先修复 Error，再处理 Warning。\n\n下一步：导出报告归档。");
    }

    private void OnExportGuideClick(object sender, RoutedEventArgs e)
    {
        TaskDialog.Show("Workflow Hub", "报告导出模块建议输出：\n1) 审核结果\n2) 匹配冲突\n3) 缺失房间生成记录。\n\n可作为项目交付附件。");
    }
}
