using System.Data.SQLite;

namespace SimpleFileManager.WPFApp;

public class MyDatabase
{
    // データベースファイルパス
    const string dbFilePath = @".\mydatabase.db";
    // データベース接続文字列
    string connectionString = $"Data Source={dbFilePath};Version=3;";
    private static MyDatabase? _instance = null;
    private MyDatabase()
    {
        using var connection = new SQLiteConnection(connectionString);
        // データベース接続を開く
        connection.Open();        
        // テーブル作成SQLクエリ
        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Files (
                FullPath TEXT NOT NULL PRIMARY KEY,
                Comment TEXT
            );
        ";
        // SQLコマンドオブジェクト
        using SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
        // SQLクエリ実行
        command.ExecuteNonQuery();
    }
    // インスタンスの取得
    public static MyDatabase GetInstance()
    {
        if (_instance is null)
        {
            _instance = new MyDatabase();
        }
        return _instance;
    }
    // 全てのレコードを取得
    public List<Dictionary<string, object>> GetAllRecords()
    {
        List<Dictionary<string, object>> records = new();

        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        string selectQuery = $"SELECT * FROM Files;";
        using var command = new SQLiteCommand(selectQuery, connection);
        using var reader = command.ExecuteReader();
        while(reader.Read())
        {
            var record = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                record.Add(reader.GetName(i), reader.GetValue(i));
            }
            records.Add(record);
        }

        return records;
    }
    // レコードの有無
    public bool Exits(string fullPath)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        string selectQuery = $"SELECT COUNT(*) FROM Files WHERE FullPath = @fullPath;";
        using var command = new SQLiteCommand(selectQuery, connection);
        command.Parameters.AddWithValue("@fullPath", fullPath);

        int count = Convert.ToInt32(command.ExecuteScalar());
        return (count > 0);
    }
    // レコードの追加・更新
    public int InsertOrUpdate(string fullPath, string comment)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        string upsertQuery  = $@"
            INSERT OR REPLACE INTO Files (FullPath, Comment)
            VALUES (@fullPath, @comment);
        ";
        using var command = new SQLiteCommand(upsertQuery , connection);
        // パラメータを設定
        command.Parameters.AddWithValue("@fullPath", fullPath);
        command.Parameters.AddWithValue("@comment", comment);

        // SQL実行
        return command.ExecuteNonQuery();
    }
    // レコードの削除
    public int Remove(string fullPath)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        string upsertQuery  = $@"
            DELETE FROM Files
            WHERE FullPath = @fullPath;
        ";
        using var command = new SQLiteCommand(upsertQuery , connection);
        // パラメータを設定
        command.Parameters.AddWithValue("@fullPath", fullPath);

        // SQL実行
        return command.ExecuteNonQuery();
    }

    // コメントの取得
    public string GetComment(string fullPath)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        string selectQuery = $"SELECT Comment FROM Files WHERE FullPath = @fullPath;";
        using var command = new SQLiteCommand(selectQuery, connection);
        command.Parameters.AddWithValue("@fullPath", fullPath);

        var obj = command.ExecuteScalar();
        return obj as string ?? "";
    }

}