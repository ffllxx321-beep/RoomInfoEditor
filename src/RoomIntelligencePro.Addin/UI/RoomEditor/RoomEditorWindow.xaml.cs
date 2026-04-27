using System.ComponentModel;
using System.Windows;

namespace RoomIntelligencePro.Addin.UI.RoomEditor;

public partial class RoomEditorWindow : Window
{
    public RoomEditorWindow(RoomEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += Close;
        Closing += OnWindowClosing;
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is not RoomEditorViewModel viewModel)
        {
            return;
        }

        if (!viewModel.HasPendingChanges())
        {
            return;
        }

        var result = MessageBox.Show(
            "检测到未保存的房间修改，是否放弃修改并关闭？",
            "Room Intelligence Pro",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
        {
            e.Cancel = true;
        }
    }
}
