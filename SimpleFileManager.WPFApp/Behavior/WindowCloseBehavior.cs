using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace SimpleFileManager.WPFApp.Behavior;

public class WindowCloseBehavior  : Behavior<Window>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        this.AssociatedObject.Closed += this.WindowClose;
    }

    private void WindowClose(object? sender, EventArgs e)
    {
            (this.AssociatedObject.DataContext as IDisposable)?.Dispose();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        this.AssociatedObject.Closed -= this.WindowClose;
    }
}
