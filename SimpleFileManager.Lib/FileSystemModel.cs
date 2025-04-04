namespace SimpleFileManager.Lib;

/// <summary>
/// ファイルシステムモデルクラス
/// </summary>
public class FileSystemModel
{
    const string VIRTUAL_ROOT = "MyComputer";
    /// <summary>
    /// ファイル情報クラス
    /// </summary>
    public class Info
    {
        public string FullName { get; set; } = "";
        public FileAttributes Attributes { get; set; } = FileAttributes.None;
        public DateTime LastModified { get; set; } = new DateTime();
        public long Length { get; set; } = -1;
    }

    /// <summary>
    /// ドライブリスト
    /// </summary>
    List<FileSystemModel.Info> DriveList = [];
    /// <summary>
    ///  カレントディレクトリ
    /// </summary>
    public string CurrentDirectory { get; set; }
        = Directory.GetCurrentDirectory();
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public FileSystemModel()
    {
        // ドライブリストの更新
        UpdateDriveList();

    }
    /// <summary>
    /// カレントディレクトリのファイルの一覧を取得
    /// </summary>
    /// <returns>IEnumerable<string></returns>
    public IEnumerable<FileSystemModel.Info> GetFiles()
    {
        List<FileSystemModel.Info> files = [];

        // 仮想ルートの場合ドライブリストを返す
        if (this.CurrentDirectory == VIRTUAL_ROOT)
        {
            foreach(var file in DriveList)
            {
                yield return file;
            }
        } else {
            DirectoryInfo directoryInfo = new (this.CurrentDirectory);

            // サブディレクトリの一覧を取得
            foreach(var dir in directoryInfo.GetDirectories())
            {
                // 不可視属性を除く
                if ((dir.Attributes & FileAttributes.Hidden) != 0) continue;
                if ((dir.Attributes & FileAttributes.System) != 0) continue;
                yield return new FileSystemModel.Info()
                {
                    FullName = dir.FullName,
                    Attributes = dir.Attributes,
                    LastModified = dir.LastWriteTime,
                };
            }
            foreach(var file in directoryInfo.GetFiles())
            {
                // 不可視属性を除く
                if ((file.Attributes & FileAttributes.Hidden) != 0) continue;
                if ((file.Attributes & FileAttributes.System) != 0) continue;
                yield return new FileSystemModel.Info()
                {
                    FullName = file.FullName,
                    Attributes = file.Attributes,
                    Length = file.Length,
                    LastModified = file.LastWriteTime,
                };
            }

        }
    }
    /// <summary>
    /// 親ディレクトリへ移動
    /// </summary>
    /// <returns></returns>
    public bool MoveParentDirectory()
    {
        // 仮想ルートの場合戻る
        if (this.CurrentDirectory == VIRTUAL_ROOT) return false;


        string parentDirectory = Path.GetDirectoryName(this.CurrentDirectory) ?? "";

        if (parentDirectory == "")
        {
            // 親ディレクトリが無い場合仮想ルート
            CurrentDirectory = VIRTUAL_ROOT;
            return true;

        }
        CurrentDirectory = parentDirectory;
        return true;
    }
    /// <summary>
    /// ドライブリストの更新
    /// </summary>;
    void UpdateDriveList()
    {
        if (DriveList.Count > 0) DriveList.Clear();

        foreach(var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady == false) continue;
            DriveList.Add(new()
            {
                FullName = drive.Name,
                Attributes = FileAttributes.Directory,
            });
        }
    }
    /// <summary>
    /// 仮想ルートか？
    /// </summary>
    /// <returns>bool</returns>
    public bool IsRoot() => (this.CurrentDirectory == VIRTUAL_ROOT);
}//class