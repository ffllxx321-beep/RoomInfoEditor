using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RoomIntelligencePro.Addin.UI.Workflow;

namespace RoomIntelligencePro.Addin.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class RoomToolsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument?.Document;
            if (document is null)
            {
                message = "当前没有打开的项目文档。";
                return Result.Failed;
            }

            var window = new WorkflowHubWindow(commandData.Application.ActiveUIDocument!);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Room Intelligence Pro", $"插件执行失败，请重试。\n详细信息：{ex.Message}");
            message = $"插件执行失败：{ex.Message}";
            return Result.Failed;
        }
    }
}
