using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using AvaloniaTests.Assets;

namespace AvaloniaTests.ViewModels;

public interface IResourceObservable : IObservable<string>, IDisposable
{
    void TriggerCultureUpdate();
}

public sealed class ResourceObservable<TState>(TState state, Func<TState, string> resourceGetter) : IResourceObservable
{
    private readonly TState _state = state;
    private readonly Func<TState, string> _resourceGetter = resourceGetter;
    private readonly ConcurrentDictionary<IObserver<string>, IObserver<string>> _observers = new();
    private bool _isDisposing;

    public void TriggerCultureUpdate()
    {
        var currentResource = _resourceGetter(_state);
        foreach (var value in _observers.Values)
        {
            value.OnNext(currentResource);
        }
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        ObjectDisposedException.ThrowIf(_isDisposing, this);
        if (!_observers.TryAdd(observer, observer))
            return ResourceDisposable<object>.Empty;
        observer.OnNext(_resourceGetter(_state));
        return new ResourceDisposable<(ResourceObservable<TState> Self, IObserver<string> Observer)>((this, observer), state =>
        {
            state.Self._observers.Remove(state.Observer, out _);
        });
    }

    public void Dispose()
    {
        if (_isDisposing) return;
        _isDisposing = true;
        foreach (var value in _observers.Values)
        {
            value.OnCompleted();
        }
        _observers.Clear();
    }
}

public sealed class ResourceDisposable<TState>(TState state, Action<TState> onDispose) : IDisposable
{
    public static ResourceDisposable<TState> Empty { get; } = new(default!, _ => {});
    private readonly TState _state = state;
    private readonly Action<TState> _onDispose = onDispose;
    public void Dispose() => _onDispose(_state);
}

// ReSharper disable InconsistentNaming
public sealed partial class StringsTest : IDisposable
{
    public static StringsTest Default { get; } = new();

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public ResourceManager ResourceManager { get; } = new(typeof(Strings));
    private CultureInfo? _culture;
    private Dictionary<string, IResourceObservable>? _observables;

    private StringsTest(CultureInfo? culture = null)
    {
        Culture = culture;
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CultureInfo? Culture
    {
        get => _culture;
        set
        {
            CultureInfo? oldCulture = _culture;
            _culture = value;
            if (_observables is null || EqualityComparer<CultureInfo>.Default.Equals(oldCulture, value))
                return;
            foreach (IResourceObservable observable in _observables.Values)
            {
                observable.TriggerCultureUpdate();
            }
        }
    }

    /// <summary> Asd </summary>
    /// <value>
    /// Default Culture: "Asd" <br/>
    /// de: "DE: Asd"
    /// <see href="Strings.resx"/>
    /// </value>
    public string Asd => GetResourceString("Asd");
    /// <summary> asd {0} </summary>
    public string Asd_F => GetResourceString("Asd_F");
    /// <summary> asd {F} </summary>
    public string Asd_Ff => GetResourceString("Asd_Ff");

    /// <summary> string.Format <see cref="Asd"/> </summary>
    /// <returns> The formatted <see cref="Asd"/> string </returns>
    public string FormatAsd() => Asd;
    /// <summary> string.Format <see cref="Asd_F"/> </summary>
    /// <param name="p0"> The parameter to be used at position {0} </param>
    /// <returns> The formatted <see cref="Asd_F"/> string </returns>
    public string FormatAsd_F(object? p0) => string.Format(Culture, Asd_F, p0);
    /// <summary> string.Format <see cref="Asd_Ff"/> </summary>
    /// <param name="f"> The parameter to be used at position {0} </param>
    /// <param name="ff"> The parameter to be used at position {1} </param>
    /// <returns> The formatted <see cref="Asd_Ff"/> string </returns>
    public string FormatAsd_Ff(object? f, object ff) => string.Format(Culture, GetResourceString(Asd_Ff, new[] {"f", "ff"}), f, ff);

    public IObservable<string> ObserveAsd => GetOrAddObservable("___Format___Asd", this, state => state.Asd);
    public IObservable<string> ObserveAsd_F => GetOrAddObservable("___Format___Asd_F", this, state => state.Asd_F);
    public IObservable<string> ObserveAsd_Ff => GetOrAddObservable("___Format___Asd_Ff", this, state => state.Asd_Ff);

    public IObservable<string> ObserveAndFormatAsd() => ObserveAsd;
    public IObservable<string> ObserveAndFormatAsd_F(object? p0)
    {
        return GetOrAddObservable("___ObserveAndFormat___Asd_F", (this, p0), state => state.Item1.FormatAsd_F(state.p0));
    }
    public IObservable<string> ObserveAndFormatAsd_Ff(object? f, object ff)
    {
        return GetOrAddObservable("___ObserveAndFormat___Asd_Ff", (this, f, ff), state => state.Item1.FormatAsd_Ff(state.f, state.ff));
    }

    private IResourceObservable GetOrAddObservable<TState>(string resourceKey,
        TState state,
        Func<TState, string> resourceGetter)
    {
        _observables ??= new Dictionary<string, IResourceObservable>();
        if (_observables.TryGetValue(resourceKey, out IResourceObservable? observable))
            return observable;
        observable = new ResourceObservable<TState>(state, resourceGetter);
        if (!_observables.TryAdd(resourceKey, observable))
            throw new Exception("Could not add new observable. This should not happen!");
        return observable;
    }

    private string GetResourceString(string resourceKey) => ResourceManager.GetString(resourceKey, Culture) ?? resourceKey;
    private string GetResourceString(string resourceKey, string[]? formatterNames)
    {
       var value = GetResourceString(resourceKey);
       if (formatterNames == null) return value;
       for (var i = 0; i < formatterNames.Length; i++)
       {
           value = value.Replace($"{{{formatterNames[i]}}}", $"{{{i}}}");
       }
       return value;
    }

    public void Dispose()
    {
        if (_observables is null)
        {
            return;
        }
        foreach (var observable in _observables.Values)
        {
            observable.Dispose();
        }
        _observables.Clear();
    }
}

public class MainWindowViewModel(StringsTest localizer) : ViewModelBase
{
    public StringsTest Localizer { get; } = localizer;

    public IObservable<string> Greeting => Localizer.ObserveAndFormatAsd_Ff("'f param'", "'ff param'");

    public void SetLanguage(string? langCode)
    {
        if (langCode is null) return;
        Localizer.Culture = CultureInfo.GetCultureInfo(langCode);
    }
}