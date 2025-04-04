using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;
using System.Windows.Markup;

namespace SimpleFileManager.WPFApp;
public class IconManager
{
    static ConcurrentDictionary<string, BitmapImage> IconDict = [];

    static public BitmapImage GetIcon(string path)
    {

        string extKey = Path.GetExtension(path).ToUpper();
        if (path == "DRIVE")
        {
            extKey = ".DRIVE";
        }
        if (extKey == "")
        {
            if (File.Exists(path))
            {
                extKey = ".DIR";
            }
            else
            {
                extKey = ".FILE";
            }
        }
        if (IconDict.ContainsKey(extKey))
        {
            return IconDict[extKey];
        }

        string systemRoot = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
        string dllPath = Path.Join(systemRoot, @"System32\SHELL32.dll");

        BitmapImage bi = new();
        System.Drawing.Icon? icon;
        if (path == "DRIVE")
        {
            icon = Icon.ExtractIcon(dllPath, 7, 64);
        }
        else
        {
            if (File.Exists(path))
            {
                icon = Icon.ExtractAssociatedIcon(path);                
            }
            else
            {
                icon = Icon.ExtractIcon(dllPath, 3, 64);
            }
        }
        if (icon is null) return bi;

        var ms = new MemoryStream();
        icon.ToBitmap().Save(ms, ImageFormat.Png);
        icon.Dispose();
        ms.Seek(0, SeekOrigin.Begin);

        bi.BeginInit();
        bi.StreamSource = ms;
        bi.CacheOption = BitmapCacheOption.OnLoad;
        bi.CreateOptions = BitmapCreateOptions.None;
        bi.EndInit();
        bi.Freeze();

        ms.SetLength(0);

        IconDict[extKey] = bi;

        return IconDict[extKey];
    }
}