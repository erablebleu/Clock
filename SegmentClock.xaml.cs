using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Clock;

/// <summary>
/// Interaction logic for SegmentClock.xaml
/// </summary>
public partial class SegmentClock : UserControl
{
    public static readonly DependencyProperty SegmentLengthProperty =
                       DependencyProperty.RegisterAttached("SegmentLength", typeof(double), typeof(SegmentClock), new FrameworkPropertyMetadata(100d, OnSizeChanged));

    public static readonly DependencyProperty SegmentWidthProperty =
                       DependencyProperty.RegisterAttached("SegmentWidth", typeof(double), typeof(SegmentClock), new FrameworkPropertyMetadata(20d, OnSizeChanged));

    public static readonly DependencyProperty SegmentGapProperty =
                       DependencyProperty.RegisterAttached("SegmentGap", typeof(double), typeof(SegmentClock), new FrameworkPropertyMetadata(4d, OnSizeChanged));

    private static readonly Dictionary<int, int> Map = new()
    {
        { 0, 0b0111111 },
        { 1, 0b0000110 },
        { 2, 0b1011011 },
        { 3, 0b1001111 },
        { 4, 0b1100110 },
        { 5, 0b1101101 },
        { 6, 0b1111101 },
        { 7, 0b0000111 },
        { 8, 0b1111111 },
        { 9, 0b1101111 },
    };

    private Path[][] _d = Array.Empty<Path[]>();
    private Path[] _points = Array.Empty<Path>();

    public SegmentClock()
    {
        InitializeComponent();
        ReDraw();
        Task.Run(ClockWork);
    }

    public double SegmentLength { get => (double)GetValue(SegmentLengthProperty); set => SetValue(SegmentLengthProperty, value); }
    public double SegmentWidth { get => (double)GetValue(SegmentWidthProperty); set => SetValue(SegmentWidthProperty, value); }
    public double SegmentGap { get => (double)GetValue(SegmentGapProperty); set => SetValue(SegmentGapProperty, value); }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SegmentClock clock)
            return;

        clock.ReDraw();
    }

    private static void SetDigit(Path[] d, int value)
    {
        for (int i = 0; i < d.Length; i++, value >>= 1)
            d[i].Visibility = GetVisibility((value & 1) != 0);
    }

    private static Visibility GetVisibility(bool v) => v ? Visibility.Visible : Visibility.Hidden;

    private static Path[] GetPoints(ref double x, double y, double w, double l, double g)
    {
        string data = $"M 0 0 {w / 2} {-w / 2} {w} 0 {w / 2} {w / 2}";
        Path[] result = new Path[]
        {
            new Path { Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x - w, y)  },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x - w, y + l + 2 * g) },
        };
        x += g;
        return result;
    }

    private static Path[] GetDigit(ref double x, double y, double w, double l, double g)
    {
        string data = $"M 0 0 {w / 2} {-w / 2} H {l - w / 2} L {l} 0 {l - w / 2} {w / 2} H {w / 2} Z";

        /*    a
         * f     b
         *    g
         * e     c
         *    d
         */
        Path[] result = new Path[]
        {
            new Path { Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y) },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x + l + 2 * g, y + g) } } },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x + l + 2 * g, y + l + 3 * g) } } },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y + 2 * l + 4 * g) },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x, y + l + 3 * g) } } },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x, y + g) } } },
            new Path { Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y + l + 2 * g) },
        };
        x += l + w + 3 * g;
        return result;
    }

    private void ReDraw()
    {
        double w = SegmentWidth;
        double l = SegmentLength;
        double g = SegmentGap;

        canvas.Width = 4 * w + 4 * l + 12 * g;
        canvas.Height = 2 * l + w + 4 * g;

        List<Path[]> digits = new();
        List<Path[]> points = new();
        double x = w / 2;

        digits.Add(GetDigit(ref x, w / 2, w, l, g));
        digits.Add(GetDigit(ref x, w / 2, w, l, g));

        points.Add(GetPoints(ref x, w / 2 + g + l + g, w, l, g));

        digits.Add(GetDigit(ref x, w / 2, w, l, g));
        digits.Add(GetDigit(ref x, w / 2, w, l, g));

        Binding binding = new("Foreground")
        {
            Source = this
        };

        canvas.Children.Clear();
        foreach (Path path in digits.SelectMany(d => d).Concat(points.SelectMany(p => p)))
        {
            canvas.Children.Add(path);
            path.SetBinding(Path.FillProperty, binding);
        }

        _d = digits.ToArray();
        _points = points.SelectMany(p => p).ToArray();
    }

    private void ClockWork()
    {
        while (true)
        {
            Thread.Sleep(100);
            DateTime dt = DateTime.Now;
            Dispatcher.Invoke(() => Update(dt.Hour * 100 + dt.Minute));
        }
    }

    private void Update(int value)
    {
        for (int i = _d.Length - 1; i >= 0; i--, value /= 10)
            SetDigit(_d[i], Map[value % 10]);

        foreach (Path p in _points)
            p.Visibility = GetVisibility(DateTime.Now.Millisecond > 500);
    }
}