using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoomIntelligencePro.Addin.Core.RoomEditor;

namespace RoomIntelligencePro.Addin.UI.RoomEditor;

public sealed class RoomCardViewModel : INotifyPropertyChanged
{
    private readonly RoomEditorRoom _model;
    private string _name;
    private string _number;

    public RoomCardViewModel(RoomEditorRoom model)
    {
        _model = model;
        _name = model.Name;
        _number = model.Number;
    }

    public int ElementId => _model.ElementId;
    public string LevelName => _model.LevelName;
    public string AreaSquareMetersText => _model.AreaSquareMetersText;

    public string Name
    {
        get => _name;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_name, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _name = normalized;
            _model.Name = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasChanges));
        }
    }

    public string Number
    {
        get => _number;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_number, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _number = normalized;
            _model.Number = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasChanges));
        }
    }

    public bool HasChanges => _model.HasChanges;

    public RoomEditorRoom Model => _model;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshFromModel()
    {
        _name = _model.Name;
        _number = _model.Number;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Number));
        OnPropertyChanged(nameof(HasChanges));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
