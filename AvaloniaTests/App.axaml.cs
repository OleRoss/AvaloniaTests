using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Strings = AvaloniaTests.Assets.Strings;

namespace AvaloniaTests;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ServiceProvider provider = new ServiceCollection()
                .AddLogging()
                .AddLocalization()
                .AddSingleton<StringsTest>(_ => StringsTest.Default)
                .AddTransient<MainWindowViewModel>()
                .BuildServiceProvider();
            var vm = provider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
                ViewModel = vm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}