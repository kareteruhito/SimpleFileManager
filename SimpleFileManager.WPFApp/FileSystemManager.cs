using System.IO;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace SimpleFileManager.WPFApp;

/// <summary>
/// ファイルシステムマネージャ
/// </summary>
class FileSystemManager
{
    // 画像をロード
    const string _imgPattern = @"\.(png|bmp|jpg|jpeg)$";
    public static BitmapImage? LoadImage(string path)
    {
        BitmapImage? bi = null;
        if (!File.Exists(path))
        {
            return bi;
        }
        if (!Regex.IsMatch(path, _imgPattern, RegexOptions.IgnoreCase))
        {
            return bi;
        }
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        using(var ms = new MemoryStream())
        {
            fs.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
        }

        return bi;
    }
    
}// class