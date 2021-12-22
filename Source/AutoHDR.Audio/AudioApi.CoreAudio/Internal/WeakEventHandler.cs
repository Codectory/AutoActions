using System;
using System.Diagnostics;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    public delegate void UnregisterCallback<TEventArgs>(EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs;

    public interface IWeakEventHandler<TEventArgs>
        where TEventArgs : EventArgs
    {
        EventHandler<TEventArgs> Handler { get; }
    }

    [DebuggerNonUserCode]
    public class WeakEventHandler<T, TEventArgs> : IWeakEventHandler<TEventArgs>
        where T : class
        where TEventArgs : EventArgs
    {
        private delegate void OpenEventHandler(T @this, object sender, TEventArgs e);

        private readonly WeakReference _targetRef;
        private readonly OpenEventHandler _openHandler;
        private readonly EventHandler<TEventArgs> _handler;
        private UnregisterCallback<TEventArgs> _unregister;

        [DebuggerNonUserCode]
        public WeakEventHandler(EventHandler<TEventArgs> eventHandler, UnregisterCallback<TEventArgs> unregister)
        {
            _targetRef = new WeakReference(eventHandler.Target);
            _openHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler),
              null, eventHandler.Method);
            _handler = Invoke;
            _unregister = unregister;
        }

        [DebuggerNonUserCode]
        public void Invoke(object sender, TEventArgs e)
        {
            T target = (T)_targetRef.Target;

            if (target != null)
                _openHandler.Invoke(target, sender, e);
            else if (_unregister != null)
            {
                _unregister(_handler);
                _unregister = null;
            }
        }

        [DebuggerNonUserCode]
        public EventHandler<TEventArgs> Handler
        {
            get { return _handler; }
        }

        [DebuggerNonUserCode]
        public static implicit operator EventHandler<TEventArgs>(WeakEventHandler<T, TEventArgs> weh)
        {
            return weh._handler;
        }
    }

    public static class EventHandlerUtils
    {
        public static EventHandler<TEventArgs> MakeWeak<TEventArgs>(this EventHandler<TEventArgs> eventHandler, UnregisterCallback<TEventArgs> unregister)
            where TEventArgs : EventArgs
        {
            if (eventHandler == null)
                throw new ArgumentNullException("eventHandler");
            if (eventHandler.Method.IsStatic || eventHandler.Target == null)
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");

            var wehType = typeof(WeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(TEventArgs));
            var wehConstructor = wehType.GetConstructor(new[] { typeof(EventHandler<TEventArgs>), typeof(UnregisterCallback<TEventArgs>) });

            var weh = (IWeakEventHandler<TEventArgs>)wehConstructor.Invoke(
              new object[] { eventHandler, unregister });

            return weh.Handler;
        }
    }

}
