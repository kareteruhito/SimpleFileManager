using System.ComponentModel;
using Reactive.Bindings;
using System.Reactive.Disposables;

using System.IO;
using Reactive.Bindings.Extensions;
using System.Windows;
using System.Diagnostics;
using System.Reactive.Linq;

using System.Windows.Media.Imaging;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Drawing;

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
        CurrentDirectory = @"F:\",
    };
    public ReactiveProperty<string> CurrentDirectory { get; }

    // アドレスバーの入力値
    public ReactiveProperty<string> AddressBar { get; set; } = new();
    // ドライブリスト(アドレスバー)
    public ReactiveCollection<string> DriveList { get; private set; } = [];
    // Webアドレス
    public ReactiveProperty<string> WebAddr { get; set; } = new(@"");
    // ピクチャビュー表示・非表示
    public ReactiveProperty<Visibility> PictureViewVisibility { get; private set; } = new(Visibility.Collapsed);
    // ピクチャビュー
    public ReactiveProperty<BitmapSource> PictureView { get; private set; } = new(new BitmapImage());

    // テキストプレビュー表示・非表示
    public ReactiveProperty<Visibility> TextPreviewVisibility { get; private set; } = new(Visibility.Collapsed);
    // テキストプレビュー
    public ReactiveProperty<string> TextPreview { get; private set; } = new("");

    // 音声再生表示・非表示
    public ReactiveProperty<Visibility> AudioPlayButtonVisibility { get; private set; } = new(Visibility.Collapsed);
    // 音声再生ボタン標示文字
    public ReactiveProperty<string> AudioPlayButtonText { get; private set; } = new("▶");
    // 音声再生ボタン
    public ReactiveCommand AudioPlayButtonCommand { get; } = new();

    // 動画再生表示・非表示
    public ReactiveProperty<Visibility> MediaPlayVisibility { get; private set; } = new(Visibility.Collapsed);
    // 動画再生
    public ReactiveProperty<string> MediaPlay { get; private set; } = new("");



    // ドラックアンドドロップ用
    public ReactiveCommand<System.Windows.Input.MouseEventArgs> MouseMoveCommand { get; }

    // データベース
    private MyDatabase _db;





    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindowViewModel()
    {
        _db = MyDatabase.GetInstance(((App)Application.Current).ConnectionString);
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
        FileListViewSelected.Skip(1).Subscribe(async e=>
        {
            if (e is null) return;

            string fullPath = e.FullPath;
            /*
            if (File.Exists(fullPath))
            {
                var dataObject = new DataObject(DataFormats.FileDrop, new[] { fullPath });
                DragDrop.DoDragDrop( new BitmapImage(), dataObject, DragDropEffects.Copy);
            }
            */

            PictureViewVisibility.Value = Visibility.Collapsed;
            PictureView.Value = new BitmapImage();
            TextPreviewVisibility.Value = Visibility.Collapsed;
            TextPreview.Value = "";
            AudioPlayButtonVisibility.Value = Visibility.Collapsed;
            AudioPlayButtonText.Value = "▶";
            MediaPlayVisibility.Value = Visibility.Collapsed;
            MediaPlay.Value="";

            var extensionsType = FileSystemManager.GetExtensionsType(fullPath);

            switch (extensionsType)
            {
                case FileSystemManager.ExtensionsType.IMAGE:    // 画像
                    PictureViewVisibility.Value = Visibility.Visible;
                    PictureView.Value = await FileSystemManager.LoadImageAsync(fullPath);
                    break;
                case FileSystemManager.ExtensionsType.IMAGIC:    // 画像(ImageMagic)
                    PictureViewVisibility.Value = Visibility.Visible;
                    PictureView.Value = await FileSystemManager.LoadImageMagicAsync(fullPath);
                    break;
                case FileSystemManager.ExtensionsType.TEXT:     // テキスト
                    TextPreviewVisibility.Value = Visibility.Visible;
                    TextPreview.Value = await FileSystemManager.LoadTextAsync(fullPath);
                    break;
                case FileSystemManager.ExtensionsType.BINARY:     // バイナリ
                    TextPreviewVisibility.Value = Visibility.Visible;
                    TextPreview.Value = await FileSystemManager.LoadBinaryDumpAsync(fullPath);
                    break;
                case FileSystemManager.ExtensionsType.AUDIO:     // 音声
                    AudioPlayButtonVisibility.Value = Visibility.Visible;
                    break;
                case FileSystemManager.ExtensionsType.VIDEO:     // 動画
                    PictureViewVisibility.Value = Visibility.Visible;
                    PictureView.Value = await FileSystemManager.LoadThumbnailFromVideoAsync(fullPath);
                    break;
            }
        });
        // 音声再生
        AudioPlayButtonCommand.Subscribe(e=>
        {
            var filePath = FileListViewSelected.Value.FullPath;
            if (!File.Exists(filePath)) return;
            if (AudioPlayButtonText.Value == "■") return;

            try
            {
                var waveOut = new WaveOutEvent();   // WaveOutの生成
                IWaveProvider? audioFile;
                string ext = Path.GetExtension(filePath).ToUpper();
                if (ext == ".WAV" || ext == ".MP3")
                {
                    audioFile = new AudioFileReader(filePath);  // 再生ファイルオブジェクトの生成
                } else {
                    audioFile = new NAudio.Vorbis.VorbisWaveReader(filePath);
                }

                if (waveOut is null) return;
                waveOut.Init(audioFile);    // 再生ファイルオブジェクトの初期化
                waveOut.PlaybackStopped += (s, e) =>
                {
                    waveOut?.Dispose();
                    AudioPlayButtonText.Value = "▶";
                }; // 再生終了イベント登録
                AudioPlayButtonText.Value = "■";
                waveOut.Play(); // 再生
            }
            catch (Exception ex)
            {
                Debug.Print($"再生中にエラーが発生しました: {ex.Message}", "エラー");
            }            
        });

        // ドラックアンドドロップ用
        MouseMoveCommand = new ReactiveCommand<System.Windows.Input.MouseEventArgs>()
            .WithSubscribe(e =>
            {
                if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;
                if (PictureViewVisibility.Value != Visibility.Visible) return;;
                if (!Path.Exists(FileListViewSelected.Value.FullPath)) return;

                var obj = (DependencyObject)e.Source;

                var dataObject = new DataObject(DataFormats.FileDrop, new[] { FileListViewSelected.Value.FullPath });
                DragDrop.DoDragDrop(obj, dataObject, DragDropEffects.Copy);
            }
        );

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