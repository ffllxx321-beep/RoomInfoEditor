using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RoomIntelligencePro.Addin.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class SettingsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        TaskDialog.Show("Room Intelligence Pro", "设置模块入口已就绪。");
        return Result.Succeeded;
    }
}
