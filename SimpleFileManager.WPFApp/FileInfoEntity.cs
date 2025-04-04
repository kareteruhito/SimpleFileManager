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
    public string SortName
    {
        get
        {
            string prefix = "5_";
            if ((Attribute & System.IO.FileAttributes.Directory) != 0)
            {
                if (".." == Name)
                {
                    prefix = "1_";
                }
                else
                {
                    prefix = "3_";
                }

            }
            return prefix + Name;
        }
    }
}