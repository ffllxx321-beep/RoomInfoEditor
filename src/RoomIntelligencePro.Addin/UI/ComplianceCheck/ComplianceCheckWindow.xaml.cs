using System.Windows;

namespace RoomIntelligencePro.Addin.UI.ComplianceCheck;

public partial class ComplianceCheckWindow : Window
{
    public ComplianceCheckWindow(ComplianceCheckViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
