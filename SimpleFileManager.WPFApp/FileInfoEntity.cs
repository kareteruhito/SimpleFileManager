using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Reactive.Disposables;
using Reactive.Bindings;

namespace SimpleFileManager.WPFApp;

public class FileInfoEntity : INotifyPropertyChanged, IDisposable
{
#region 
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    private CompositeDisposable Disposable { get; } = new ();
    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() => this.Disposable.Dispose();
#endregion
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
    public ReactiveProperty<string> Comment {get; set;} = new("");
    
}