using System.Windows.Media.Imaging;

namespace SimpleFileManager.WPFApp;

public class FileInfoEntity
{
    public BitmapSource Icon { get; set; } = new BitmapImage();
    public string FullPath { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime LastModified { get; set; } = new DateTime();
    public long Length { get; set; } = -1;
    public System.IO.FileAttributes Attribute { get; set; } = System.IO.FileAttributes.None;
}