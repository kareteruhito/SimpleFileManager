using System.ComponentModel;
using Reactive.Bindings;
using System.Reactive.Disposables;

using System.IO;
using Reactive.Bindings.Extensions;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;

namespace SimpleFileManager.WPFApp;
/// <summary>
/// メインウィンドウビューモデル
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    private CompositeDisposable Disposable { get; } = new ();
    public ReactiveCommand<EventArgs> WindowLoadedCommand { get; } = new();
    public ReactiveCommand<EventArgs> FilesListViewDoubleClickCommand { get; } = new();

    public ReactiveCollection<FileInfoEntity> FileInfos { get; set; } = [];
    public ReactiveProperty<FileInfoEntity> FileListViewSelected { get; set; } = new();

    readonly Lib.FileSystemModel fileSystemModel = new();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindowViewModel()
    {
        WindowLoadedCommand.Subscribe(e => WindowLoadedCommand_Subscribe(e))
            .AddTo(this.Disposable);
        
        FilesListViewDoubleClickCommand.Subscribe(e => FilesListViewDoubleClickCommand_Subscribe(e))
            .AddTo(this.Disposable);
    }//MainWindowViewModel

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        System.Diagnostics.Debug.Print("Dispose()が呼ばれました。");
        this.Disposable.Dispose();
    }
    /// <summary>
    /// WindowLoadedCommand_Subscribe
    /// </summary>
    /// <param name="e"></param>
    async void WindowLoadedCommand_Subscribe(EventArgs e)
    {
        if (0 < FileInfos.Count)
        {
            var tcs = new TaskCompletionSource<bool>();
            var v = FileInfos.ObserveResetChanged<int>().Subscribe(x=>
            {
                Debug.Print("クリア");
                tcs.SetResult(true);
            });
            FileInfos.ClearOnScheduler();
            await tcs.Task;
            v.Dispose();
        }

        Debug.Print("追加");
        if (fileSystemModel.IsRoot() == false)
        {
            string parentDir = Path.GetDirectoryName(fileSystemModel.CurrentDirectory) ?? "/";
            FileInfos.AddOnScheduler
            (
                new()
                {
                    Icon = IconManager.GetIcon(parentDir),
                    Name = "..",
                    FullPath = parentDir,
                    Attribute = FileAttributes.Directory,
                }
            );
        }
        foreach(var f in fileSystemModel.GetFiles())
        {
            string name = Path.GetFileName(f.FullName);
            string fullName = f.FullName;
            if (name == "")
            {
                name = f.FullName;
                fullName = "DRIVE";
            }
            FileInfos.AddOnScheduler
            (
                new()
                {
                    Icon = IconManager.GetIcon(fullName),
                    Name = name,
                    FullPath = f.FullName,
                    LastModified = f.LastModified,
                    Length = f.Length,
                    Attribute = f.Attributes,
                }
            );
        }               
    }
    /// <summary>
    /// FilesListViewDoubleClickCommand_Subscribe
    /// </summary>
    /// <param name="e"></param>
    void FilesListViewDoubleClickCommand_Subscribe(EventArgs e)
    {
        // ダブルクリックイベント
        var item = FileListViewSelected.Value;
        if (item is null) return;
        
//System.Diagnostics.Debug.Print($"ダブルクリック{item.FullPath}");
        if ((item.Attribute & FileAttributes.Directory) != 0)
        {
            if (item.Name == "..")
            {
                fileSystemModel.MoveParentDirectory();    
            }
            else
            {
                fileSystemModel.CurrentDirectory = item.FullPath;
            }

            WindowLoadedCommand_Subscribe(e);
        }

    }
}