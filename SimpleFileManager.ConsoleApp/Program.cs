namespace SimpleFileManager.ConsoleApp;
/// <summary>
/// プログラム
/// </summary>
class Program
{
    /// <summary>
    /// エントリーポイント
    /// </summary>
    static void Main()
    {
        var fileSystemModel = new SimpleFileManager.Lib.FileSystemModel();

        fileSystemModel.MoveParentDirectory();
        fileSystemModel.MoveParentDirectory();
        fileSystemModel.MoveParentDirectory();
        fileSystemModel.MoveParentDirectory();
        fileSystemModel.MoveParentDirectory();
        fileSystemModel.MoveParentDirectory();

        foreach(var info in fileSystemModel.GetFiles())
        {
            // 不可視属性を排除
            if ((info.Attributes & FileAttributes.Hidden) != 0) continue;

            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                Console.WriteLine($"[{info.FullName}]");
            } else {
                Console.WriteLine($"{info.FullName}");
            }
        }
    }
}
