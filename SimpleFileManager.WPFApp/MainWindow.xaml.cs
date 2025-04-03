using System.Windows;

namespace SimpleFileManager.WPFApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var str = MayworkNet.Lib.Class1.Hello();
        System.Diagnostics.Debug.Print($"{str}");
    }
}