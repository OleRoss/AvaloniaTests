using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Resources;

namespace AvaloniaTests.Assets;

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
public sealed partial class Strings
{
    private Strings()
    {
        Observables = new StringsTestObservables(this);
    }

    public StringsTestObservables Observables { get; }

    public sealed class StringsTestObservables : IDisposable
    {
        private readonly Strings _parent;
        private Dictionary<string, IResourceObservable>? _observables;

        public StringsTestObservables(Strings parent)
        {
            _parent = parent;
            _parent.CultureChanged += (_, _) =>
            {
                if (_observables is null)
                {
                    return;
                }

                foreach (IResourceObservable observable in _observables.Values)
                {
                    observable.TriggerCultureUpdate();
                }
            };
        }

        public IObservable<string> @Asd => GetOrAddObservable("___Format___Asd", _parent, state => state.Asd);
        public IObservable<string> @Asd_F => GetOrAddObservable("___Format___Asd_F", _parent, state => state.Asd_F);
        public IObservable<string> @Asd_Ff => GetOrAddObservable("___Format___Asd_Ff", _parent, state => state.Asd_Ff);

        public IObservable<string> @FormatAsd() => @Asd;
        public IObservable<string> @FormatAsd_F(object? p0)
        {
            return GetOrAddObservable("___ObserveAndFormat___Asd_F", (_parent, p0), state => state.Item1.FormatAsd_F(state.p0));
        }
        public IObservable<string> @FormatAsd_Ff(object? f, object ff)
        {
            return GetOrAddObservable("___ObserveAndFormat___Asd_Ff", (_parent, f, ff), state => state.Item1.FormatAsd_Ff(state.f, state.ff));
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

        void IDisposable.Dispose()
        {
            if (_observables is null)
            {
                return;
            }
            foreach (IResourceObservable observable in _observables.Values)
            {
                observable.Dispose();
            }
            _observables.Clear();
        }
    }
}