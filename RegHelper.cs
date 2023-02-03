using Microsoft.Win32;

namespace Clock;

internal static class RegHelper
{
    public const string StartupKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    public const string AppKey = @"SOFTWARE\SomeDudeFromGitHub\Clock";

    public static void SetKeyValue(string keyName, string name, string value)
        => Registry.CurrentUser.CreateSubKey(keyName, true).SetValue(name, value);

    public static void DeleteKeyValue(string keyName, string name)
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyName, true);

        if (key is null 
            || key.GetValue(name) is null)
            return;

        key.DeleteValue(name);
    }

    public static string? GetKeyValue(string keyName, string name)
        => (string?)Registry.CurrentUser.OpenSubKey(keyName, true)?.GetValue(name);
}