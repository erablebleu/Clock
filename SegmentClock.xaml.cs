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
    private Path[][] _d;
    private Path[] _points;

    public double SegmentThickness = 20;
    public double SegmentLength = 100;
    public double SegmentGap = 4;

    public SegmentClock()
    {
        InitializeComponent();

        Task.Run(ClockWork);

        double w = SegmentThickness;
        double l = SegmentLength;
        double g = SegmentGap;

        canvas.Width = 4 * w + 4 * l + 12 * g;
        canvas.Height = 2 * l + w + 4 * g;

        _d = new Path[][]
        {
            GetDigit(w/2, w/2, w, l, g),
            GetDigit(3 * w / 2 + l + 3 * g, w / 2, w, l, g),
            GetDigit(5*w/2+2*l+7*g, w/2, w, l, g),            
            GetDigit(7*w/2+3*l+10*g, w/2, w, l, g),            
        };
        _points = GetPoints(w / 2 + g + l + g + w / 2 + g + w / 2 + g + l + g + g, w/2 + g + l + g, w, l, g);

        Binding binding = new("Foreground")
        {
            Source = this
        };

        foreach (Path path in _d.SelectMany(d => d).Concat(_points))
        {
            canvas.Children.Add(path);
            path.SetBinding(Path.FillProperty, binding);
        }
    }

    private void ClockWork()
    {
        while(true)
        {
            Thread.Sleep(100);
            DateTime dt = DateTime.Now;
            Dispatcher.Invoke(() => Update(dt.Hour*100+dt.Minute));
        }
    }

    private void Update(int value)
    {
        for(int i = _d.Length - 1; i >= 0; i--, value/=10)
            SetDigit(_d[i], Map[value%10]);

        foreach (Path p in _points)
            p.Visibility = GetVisibility(DateTime.Now.Millisecond > 500);
    }

    private static Dictionary<int, int> Map = new()
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
    private void SetDigit(Path[] d, int value)
    {
        for (int i = 0; i < d.Length; i++, value >>= 1)
            d[i].Visibility = GetVisibility((value & 1) != 0);
    }

    private static Visibility GetVisibility(bool v) => v ? Visibility.Visible : Visibility.Hidden;

    private Path[] GetPoints(double x, double y, double w, double l, double g)
    {
        string data = $"M 0 0 {w / 2} {-w / 2} {w} 0 {w/2} {w/2}";
        return new Path[]
        {
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x, y)  },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x, y + l + 2 * g) },
        };
    }

    private Path[] GetDigit(double x, double y, double w, double l, double g)
    {
        string data = $"M 0 0 {w / 2} {-w / 2} H {l - w / 2} L {l} 0 {l - w / 2} {w / 2} H {w / 2} Z";

        /*    a
         * f     b
         *    g
         * e     c
         *    d
         */
        return new Path[]
        {
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y) },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x + l + 2 * g, y + g) } } },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x + l + 2 * g, y + l + 3 * g) } } },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y + 2 * l + 4 * g) },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x, y + l + 3 * g) } } },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TransformGroup(){ Children = new TransformCollection{ new RotateTransform(90), new TranslateTransform(x, y + g) } } },
            new Path { Fill = Foreground, Data = Geometry.Parse(data), RenderTransform = new TranslateTransform(x + g, y + l + 2 * g) },
        };
    }
}