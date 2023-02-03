using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Clock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static string AppName = Assembly.GetExecutingAssembly().GetName().Name!;
    private const int GWL_EX_STYLE = -20;
    private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

    public MainWindow()
    {
        InitializeComponent();

        try
        {           
            Settings = JsonSerializer.Deserialize<Settings>(RegHelper.GetKeyValue(RegHelper.AppKey, "Settings")!)!;
        }
        catch { }
        UpdateStartup(); // update startup, in case path changes
        Settings.IsDirty = false;
        DataContext = Settings;
        clock.MouseLeftButtonDown += OnMouseLeftButtonDown;
        Deactivated += OnDeactivated;
        Loaded += OnLoaded;
        DragLeave += (s, e) =>
        {
            Save();
        };
    }

    public Settings Settings { get; private set; } = new Settings();

    [DllImport("user32.dll", SetLastError = true)] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Settings.IsPositionLocked)
            return;

        DragMove();
        Save();
    }

    private void UpdateStartup()
    {        
        if (Settings.StartWithWindows)
            RegHelper.SetKeyValue(RegHelper.StartupKey, AppName, Process.GetCurrentProcess().MainModule!.FileName);
        else
            RegHelper.DeleteKeyValue(RegHelper.StartupKey, AppName);
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (sender is not Window window)
            return;

        window.Topmost = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Hide_1(object sender, RoutedEventArgs e) => Hide(TimeSpan.FromMinutes(1));

    private void Hide_5(object sender, RoutedEventArgs e) => Hide(TimeSpan.FromMinutes(5));

    private void Hide_15(object sender, RoutedEventArgs e) => Hide(TimeSpan.FromMinutes(15));

    private void Hide_30(object sender, RoutedEventArgs e) => Hide(TimeSpan.FromMinutes(30));

    private void Hide_60(object sender, RoutedEventArgs e) => Hide(TimeSpan.FromMinutes(60));

    private void Hide(TimeSpan timeSpan)
    {
        Visibility = Visibility.Hidden;
        Task.Delay(timeSpan).ContinueWith(t => Dispatcher.Invoke(() => Visibility = Visibility.Visible));
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (!Settings.IsDirty)
            return;

        UpdateStartup();
        Save();

        Settings.IsDirty = false;
    }

    private void Save()
    {
        RegHelper.SetKeyValue(RegHelper.AppKey, "Settings", JsonSerializer.Serialize(Settings));
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        //Variable to hold the handle for the form
        var helper = new WindowInteropHelper(this).Handle;
        //Performing some magic to hide the form from Alt+Tab
        SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
    }
}