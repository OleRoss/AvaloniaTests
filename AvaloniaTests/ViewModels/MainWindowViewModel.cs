using System;
using System.Globalization;
using AvaloniaTests.Assets;

namespace AvaloniaTests.ViewModels;

public class MainWindowViewModel(Strings i18N) : ViewModelBase
{
    public Strings I18N { get; } = i18N;

    public IObservable<string> Greeting => I18N.Observables.FormatAsd_Ff(1, "'ff param'");

    public void SetLanguage(string? langCode)
    {
        if (langCode is null) return;
        I18N.Culture = CultureInfo.GetCultureInfo(langCode);
    }
}