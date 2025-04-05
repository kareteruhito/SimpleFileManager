using System.ComponentModel;
using Reactive.Bindings;
using System.Reactive.Disposables;

using System.IO;
using Reactive.Bindings.Extensions;
using System.Windows;
using System.Diagnostics;
using System.Reactive.Linq;

using System.Windows.Media.Imaging;

namespace SimpleFileManager.WPFApp;
/// <summary>
/// メインウィンドウビューモデル
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
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
    public ReactiveCommand<EventArgs> WindowLoadedCommand { get; } = new();
    public ReactiveCommand<EventArgs> FilesListViewDoubleClickCommand { get; } = new();

    public ReactiveCollection<FileInfoEntity> FileInfos { get; set; } = [];
    public ReactiveProperty<FileInfoEntity> FileListViewSelected { get; set; } = new();

    //@"\\celeron-n4000.local\archives",
    readonly Lib.FileSystemModel fileSystemModel = new()
    {
        CurrentDirectory = @"C:\",
    };
    public ReactiveProperty<string> CurrentDirectory { get; }

    // アドレスバーの入力値
    public ReactiveProperty<string> AddressBar { get; set; } = new();
    // ドライブリスト(アドレスバー)
    public ReactiveCollection<string> DriveList { get; private set; } = [];
    // Webアドレス
    public ReactiveProperty<string> WebAddr { get; set; } = new(@"");
    // ピクチャビュー表示・非表示
    public ReactiveProperty<Visibility>
    PictureViewVisibility { get; private set; } = new(Visibility.Collapsed);
    // ピクチャビュー
    public ReactiveProperty<BitmapImage>
    PictureView { get; private set; } = new(new BitmapImage());

    // データベース
    private MyDatabase _db = MyDatabase.GetInstance();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindowViewModel()
    {
        WindowLoadedCommand.Subscribe(e => WindowLoadedCommand_Subscribe(e))
            .AddTo(this.Disposable);
        
        FilesListViewDoubleClickCommand.Subscribe(e => FilesListViewDoubleClickCommand_Subscribe(e))
            .AddTo(this.Disposable);
        
        this.CurrentDirectory = fileSystemModel.ObserveProperty(x => x.CurrentDirectory)
            .ToReactiveProperty("")
            .AddTo(this.Disposable);
        
        // ドライブリストの登録
        foreach(var f in fileSystemModel.GetDrives())
        {
            DriveList.AddOnScheduler(f.FullName);
        }

        this.CurrentDirectory.Subscribe(newDir=>
        {
            FileListViewUpdate();
            if (AddressBar.Value != newDir)
            {
                AddressBar.Value = newDir;
            }
        });
        this.AddressBar.Subscribe(newDir =>
        {
            if (!Directory.Exists(newDir)) return;
            if (fileSystemModel.CurrentDirectory != newDir)
            {
                fileSystemModel.CurrentDirectory = newDir;
            }
        });

        // ファイルリストビュー選択アイテムの購読
        FileListViewSelected.Skip(1).Subscribe(e=>
        {
            if (e is null) return;

            string fullPath = e.FullPath;

            // 画像のプレビュー表示
            var bi = FileSystemManager.LoadImage(fullPath);
            if (bi is not null)
            {
                PictureViewVisibility.Value = Visibility.Visible;
                PictureView.Value = bi;
            }
            else
            {
                PictureViewVisibility.Value = Visibility.Collapsed;
                PictureView.Value = new BitmapImage();
            }
        });


    }//MainWindowViewModel

    /// <summary>
    /// WindowLoadedCommand_Subscribe
    /// </summary>
    /// <param name="e"></param>
    void WindowLoadedCommand_Subscribe(EventArgs e)
    {
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
        }

    }
    /// <summary>
    /// ファイルリストビューの更新
    /// </summary>
    async void FileListViewUpdate()
    {
        if (0 < FileInfos.Count)
        {
            var tcs = new TaskCompletionSource<bool>();
            var v = FileInfos.ObserveResetChanged<int>().Subscribe(x=>
            {
                tcs.SetResult(true);
            });
            FileInfos.ClearOnScheduler();
            await tcs.Task;
            v.Dispose();
        }

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

        try {
            foreach(var f in fileSystemModel.GetFiles())
            {
                string name = Path.GetFileName(f.FullName);
                string fullName = f.FullName;
                if (name == "")
                {
                    name = f.FullName;
                    fullName = "DRIVE";
                }

                var fi = new FileInfoEntity()
                {
                    Icon = IconManager.GetIcon(fullName),
                    Name = name,
                    FullPath = f.FullName,
                    LastModified = f.LastModified,
                    Length = f.Length,
                    Attribute = f.Attributes,
                };


                string commentStr = _db.GetComment(f.FullName);

                var comment = new ReactiveProperty<string>(commentStr);
                comment.Subscribe(e =>
                {
                    if (FileListViewSelected.Value is null) return;
                    if (FileInfos.Any() == false) return;
                    
                    string fullName = FileListViewSelected.Value.FullPath;
                    string comment = FileListViewSelected.Value.Comment.Value;
                    if (e != comment) return;
                    Debug.Print($"{fullName}のコメント{comment}");
                    if ("" != comment)
                    {
                        _db.InsertOrUpdate(fullName, comment);
                    }
                });
                fi.Comment = comment;


                FileInfos.AddOnScheduler(fi);
            }
        } catch (System.UnauthorizedAccessException ex ) {
            MessageBox.Show(ex.Message);
        }
    }
}