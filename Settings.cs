using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Clock;

public class Settings : INotifyPropertyChanged
{
    private double _top;
    private double _left;
    private bool _isPositionLocked;
    private Color _color = Colors.Red;
    private double _opacity = 0.5;
    private double _height = 200;
    private bool _startWithWindows;
    private double _segmentWidth = 20;
    private double _segmentLength = 100;
    private double _segmentGap = 4;
    private bool _isDirty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double Top { get => _top; set => Set(ref _top, value); }
    public double Left { get => _left; set => Set(ref _left, value); }
    public bool IsPositionLocked { get => _isPositionLocked; set => Set(ref _isPositionLocked, value); }
    [JsonConverter(typeof(JsonColorConverter))] public Color Color { get => _color; set => Set(ref _color, value); }
    public double Opacity { get => _opacity; set => Set(ref _opacity, value); }
    public double Height { get => _height; set => Set(ref _height, value); }
    public bool StartWithWindows { get => _startWithWindows; set => Set(ref _startWithWindows, value); }
    [JsonIgnore] public bool IsDirty { get => _isDirty; set => Set(ref _isDirty, value); }
    public double SegmentWidth { get => _segmentWidth; set => Set(ref _segmentWidth, value); }
    public double SegmentLength { get => _segmentLength; set => Set(ref _segmentLength, value); }
    public double SegmentGap { get => _segmentGap; set => Set(ref _segmentGap, value); }


    public void RaisePropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public bool Set<T>(ref T obj, T value, [CallerMemberName] string? propertyName = null)
    {
        if (obj != null && obj.Equals(value))
            return false;

        obj = value;
        RaisePropertyChanged(propertyName);
        if (propertyName != nameof(IsDirty))
            IsDirty = true;

        return true;
    }
}