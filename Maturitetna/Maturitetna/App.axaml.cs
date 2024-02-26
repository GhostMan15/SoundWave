using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Maturitetna;

public class App : Application
{

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow.CreateInstance();
        }

        base.OnFrameworkInitializationCompleted();
    }
}