using System.Windows;

namespace SimpleFileManager.WPFApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public string ConnectionString {get; private set;} = "";
    protected override void OnStartup(StartupEventArgs e)
    {
        string dbFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"mydatabase.db");
        this.ConnectionString = $"Data Source={dbFile};Version=3;";
    }
}

