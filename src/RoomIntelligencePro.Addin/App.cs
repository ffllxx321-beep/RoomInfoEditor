using Autodesk.Revit.UI;
using System.Reflection;

namespace RoomIntelligencePro.Addin;

public sealed class App : IExternalApplication
{
    private const string TabName = "Premium Revit Utility";
    private const string RoomToolsButtonId = "RIP.RoomTools";
    private const string ComplianceCheckButtonId = "RIP.ComplianceCheck";
    private const string DataManagementButtonId = "RIP.DataManagement";
    private const string SettingsButtonId = "RIP.Settings";

    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            TryCreateRibbonTab(application, TabName);

            CreatePanelWithButton(
                application,
                TabName,
                panelName: "房间工具",
                buttonId: RoomToolsButtonId,
                buttonName: "房间工具",
                commandType: typeof(Commands.RoomToolsCommand),
                tooltip: "打开房间工具入口。");

            CreatePanelWithButton(
                application,
                TabName,
                panelName: "规范检查",
                buttonId: ComplianceCheckButtonId,
                buttonName: "规范检查",
                commandType: typeof(Commands.ComplianceCheckCommand),
                tooltip: "打开规范检查入口。");

            CreatePanelWithButton(
                application,
                TabName,
                panelName: "数据管理",
                buttonId: DataManagementButtonId,
                buttonName: "数据管理",
                commandType: typeof(Commands.DataManagementCommand),
                tooltip: "打开数据管理入口。");

            CreatePanelWithButton(
                application,
                TabName,
                panelName: "设置",
                buttonId: SettingsButtonId,
                buttonName: "设置",
                commandType: typeof(Commands.SettingsCommand),
                tooltip: "打开插件设置入口。");

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Room Intelligence Pro", $"Ribbon 初始化失败: {ex.Message}");
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    private static void TryCreateRibbonTab(UIControlledApplication application, string tabName)
    {
        try
        {
            application.CreateRibbonTab(tabName);
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException)
        {
            // Tab already exists, safe to ignore.
        }
    }

    private static void CreatePanelWithButton(
        UIControlledApplication application,
        string tabName,
        string panelName,
        string buttonId,
        string buttonName,
        Type commandType,
        string tooltip)
    {
        var panel = GetOrCreateRibbonPanel(application, tabName, panelName);
        if (HasRibbonItem(panel, buttonId))
        {
            return;
        }

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var buttonData = new PushButtonData(
            name: buttonId,
            text: buttonName,
            assemblyName: assemblyPath,
            className: commandType.FullName ?? throw new InvalidOperationException("Command type full name is required."));

        var button = panel.AddItem(buttonData) as PushButton;
        if (button is null)
        {
            throw new InvalidOperationException($"Failed to create button '{buttonName}'.");
        }

        button.ToolTip = tooltip;
    }

    private static RibbonPanel GetOrCreateRibbonPanel(UIControlledApplication application, string tabName, string panelName)
    {
        var panel = application.GetRibbonPanels(tabName)
            .FirstOrDefault(p => string.Equals(p.Name, panelName, StringComparison.Ordinal));
        return panel ?? application.CreateRibbonPanel(tabName, panelName);
    }

    private static bool HasRibbonItem(RibbonPanel panel, string itemName)
    {
        return panel.GetItems().Any(i => string.Equals(i.Name, itemName, StringComparison.Ordinal));
    }
}
