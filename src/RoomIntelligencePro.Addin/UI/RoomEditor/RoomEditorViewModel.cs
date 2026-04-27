using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RoomIntelligencePro.Addin.Services.RoomEditor;
using RoomIntelligencePro.Addin.UI.Common;

namespace RoomIntelligencePro.Addin.UI.RoomEditor;

public sealed class RoomEditorViewModel : INotifyPropertyChanged
{
    private readonly RoomEditorService _service;
    private readonly RelayCommand _saveCommand;
    private string _statusText = "就绪";

    public RoomEditorViewModel(RoomEditorService service)
    {
        _service = service;
        Rooms = new ObservableCollection<RoomCardViewModel>();

        RefreshCommand = new RelayCommand(RefreshRooms);
        _saveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
        SaveCommand = _saveCommand;
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
    }

    public ObservableCollection<RoomCardViewModel> Rooms { get; }

    public ICommand RefreshCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (string.Equals(_statusText, value, StringComparison.Ordinal))
            {
                return;
            }

            _statusText = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? RequestClose;

    public void Initialize()
    {
        RefreshRooms();
    }

    private void RefreshRooms()
    {
        foreach (var room in Rooms)
        {
            room.PropertyChanged -= OnRoomPropertyChanged;
        }

        var rooms = _service.LoadRooms();

        Rooms.Clear();
        foreach (var room in rooms)
        {
            var roomViewModel = new RoomCardViewModel(room);
            roomViewModel.PropertyChanged += OnRoomPropertyChanged;
            Rooms.Add(roomViewModel);
        }

        StatusText = $"已加载 {Rooms.Count} 个房间";
        _saveCommand.RaiseCanExecuteChanged();
    }

    private bool CanSaveChanges()
    {
        return Rooms.Any(r => r.HasChanges);
    }

    private void SaveChanges()
    {
        var changed = Rooms.Where(r => r.HasChanges).ToList();
        var saveResult = _service.SaveRooms(changed.Select(c => c.Model).ToList());

        foreach (var room in changed)
        {
            room.RefreshFromModel();
        }

        if (saveResult.ValidationErrors.Count > 0)
        {
            StatusText = $"保存被阻止：{saveResult.ValidationErrors[0]}";
        }
        else if (saveResult.UpdatedCount == 0)
        {
            StatusText = "没有可保存的变更";
        }
        else
        {
            StatusText = $"已保存 {saveResult.UpdatedCount} 个房间，跳过 {saveResult.SkippedCount} 个";
        }

        _saveCommand.RaiseCanExecuteChanged();
    }

    public bool HasPendingChanges()
    {
        return Rooms.Any(r => r.HasChanges);
    }

    private void OnRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!string.Equals(e.PropertyName, nameof(RoomCardViewModel.HasChanges), StringComparison.Ordinal))
        {
            return;
        }

        _saveCommand.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
