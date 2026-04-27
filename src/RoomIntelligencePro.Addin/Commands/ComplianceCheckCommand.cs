using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RoomIntelligencePro.Addin.RevitAdapter.Rules;
using RoomIntelligencePro.Addin.Services.ComplianceCheck;
using RoomIntelligencePro.Addin.UI.ComplianceCheck;

namespace RoomIntelligencePro.Addin.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class ComplianceCheckCommand : IExternalCommand
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

            var provider = new RevitRoomRuleSubjectProvider(document);
            var service = new ComplianceCheckService(provider);
            var viewModel = new ComplianceCheckViewModel(service);
            viewModel.Load();

            var window = new ComplianceCheckWindow(viewModel);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Room Intelligence Pro", $"规范检查执行失败，请重试。\n详细信息：{ex.Message}");
            message = $"规范检查执行失败：{ex.Message}";
            return Result.Failed;
        }
    }
}
