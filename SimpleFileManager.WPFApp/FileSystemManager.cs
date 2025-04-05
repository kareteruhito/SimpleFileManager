using System.IO;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using ImageMagick;
using System.DirectoryServices;

namespace SimpleFileManager.WPFApp;

/// <summary>
/// ファイルシステムマネージャ
/// </summary>
class FileSystemManager
{
    private const long MaxFileSizeInBytes = 1 * 1024 * 1024; // 1MB
    private const int BytesPerLine = 16; // 1行に表示するバイト数
    // 拡張子タイプ
    public enum ExtensionsType
    {
        TEXT,
        IMAGE,
        IMAGIC,
        BINARY,
        AUDIO,
        DIRECTORY,
        NONE,
    };
    const string _imgPattern = @"\.(png|bmp|jpg|jpeg)$";
    const string _txtPattern = @"\.(txt|html|cs|xaml)$";
    const string _wavPattern = @"\.(wav|ogg|mp3|mid)$";
    const string _imagicPattern = @"\.(psd|xcf|webp|webm|avi|svg|mp4|gif)$";
    // 拡張子タイプを取得
    public static ExtensionsType GetExtensionsType(string path)
    {
        if (Directory.Exists(path))
        {
            return ExtensionsType.DIRECTORY;
        }
        if (Regex.IsMatch(path, _imgPattern, RegexOptions.IgnoreCase))
        {
            return ExtensionsType.IMAGE;
        }
        if (Regex.IsMatch(path, _txtPattern, RegexOptions.IgnoreCase))
        {
            return ExtensionsType.TEXT;
        }
        if (Regex.IsMatch(path, _wavPattern, RegexOptions.IgnoreCase))
        {
            return ExtensionsType.AUDIO;
        }
        if (Regex.IsMatch(path, _imagicPattern, RegexOptions.IgnoreCase))
        {
            return ExtensionsType.IMAGIC;
        }
        return ExtensionsType.BINARY;
    }
    // 画像をロード
    public static async Task<BitmapImage> LoadImageAsync(string path)
    {
        BitmapImage bi = new ();

        try
        {

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var ms = new MemoryStream();
            
            await fs.CopyToAsync(ms);
            bi = await Task.Run(()=>
            {
                ms.Seek(0, SeekOrigin.Begin);
                bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();

                return bi;
            });
        }
        catch (IOException ex)
        {
            Debug.Print($"ファイルの読み込み中にエラーが発生しました: {ex.Message}");
        }

        return bi;
    }
    // テキストのロード
    public static async Task<string> LoadTextAsync(string path)
    {
        string result = "";
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fs);

            char[] buffer = new char[4096];
            var contentBuilder = new System.Text.StringBuilder();
            int bytesRead = 0;
            int totalBytesRead = 0;

            while (totalBytesRead < MaxFileSizeInBytes && (bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                contentBuilder.Append(buffer, 0, bytesRead);
                totalBytesRead += bytesRead * sizeof(char);
            }

            result = contentBuilder.ToString();
        }
        catch (IOException ex)
        {
            Debug.Print($"ファイルの読み込み中にエラーが発生しました: {ex.Message}");
        }

        return result;
    }

    // バイナリダンプのロード
    public static async Task<string> LoadBinaryDumpAsync(string path)
    {
        string result = "";
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fs);

            char[] buffer = new char[4096];
            var dumpBuilder = new System.Text.StringBuilder();
            int bytesRead = 0;
            long offset = 0;

            while (offset < MaxFileSizeInBytes && (bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                int processBytes = Math.Min(bytesRead, (int)(MaxFileSizeInBytes - offset));

                for (int i = 0; i < processBytes; i += BytesPerLine)
                {
                    int lineBytes = Math.Min(BytesPerLine, processBytes - i);

                    // オフセット表示
                    dumpBuilder.Append($"{offset + i:X8}  ");

                    // 16進数表示
                    for (int j = 0; j < BytesPerLine; j++)
                    {
                        if (j < lineBytes)
                        {
                            dumpBuilder.Append($"{(byte)buffer[i + j]:X2} ");
                        }
                        else
                        {
                            dumpBuilder.Append("   "); // 空白で埋める
                        }
                    }
                    dumpBuilder.Append(" |");

                    // ASCII表示 (印刷可能な文字のみ)
                    for (int j = 0; j < lineBytes; j++)
                    {
                        char c = (char)buffer[i + j];
                        if (char.IsControl(c) || c > 126)
                        {
                            dumpBuilder.Append('.');
                        }
                        else
                        {
                            dumpBuilder.Append(c);
                        }
                    }
                    dumpBuilder.AppendLine("|");
                }
                offset += processBytes;
            }

            result = dumpBuilder.ToString();
        }
        catch (IOException ex)
        {
            Debug.Print($"ファイルの読み込み中にエラーが発生しました: {ex.Message}");
        }

        return result;
    }
    // イメージマジックによる画像読み込み
    public static async Task<BitmapSource> LoadImageMagicAsync(string path)
    {
        BitmapSource result = new BitmapImage();

        try {
            result = await Task.Run(()=>
            {
                using var mi = new MagickImage(path);
                var bs = mi.ToBitmapSource();
                bs.Freeze();
                return bs;
            });
        }
        catch (Exception ex)
        {
            Debug.Print($"ファイルの読み込み中にエラーが発生しました: {ex.Message}");
        }

        return result;
    }

}// class