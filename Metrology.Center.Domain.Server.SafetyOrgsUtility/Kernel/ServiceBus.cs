using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel
{
    internal sealed class ServiceBus : SingletonBase<ServiceBus>
    {
        private ServiceBus() { }  //TODO: debug test




        //private Dictionary<Type, List<Object>> _Subscribers = new Dictionary<Type, List<Object>>();
        
        private HashSet<HandlerDescriptor> _Descriptors = new HashSet<HandlerDescriptor>();

        //public void Register<TEvent>(Action<TEvent> handler) where TEvent: IEvent
        //{
        //    List<Object> handlers;
        //    if (_Subscribers.ContainsKey(typeof(TEvent)))
        //    {
        //        handlers = _Subscribers[typeof(TEvent)];
        //        handlers.Add(handler);
        //    }
        //    else
        //    {
        //        handlers = new List<Object>();
        //        handlers.Add(handler);
        //        _Subscribers[typeof(TEvent)] = handlers;
        //    }

        //    //return new RegisterToken(
        //    //    () =>
        //    //    {
        //    //        handlers.Remove(handler);
        //    //    }    
        //    //);
        //}

        public IDisposable RegisterInstance(object handler)
        {
            var handlers = handler.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(i => new HandlerDescriptor(i.GetGenericArguments()[0], handler))
                .ToArray();
            if (handlers == null || handlers.Count() == 0)
                throw new Exception("Trying to register non-handler object");

            Array.ForEach(handlers, (h) => _Descriptors.Add(h));

            return new RegisterToken(() => UnregisterInstance(handler));
        }

        public IDisposable RegisterInstance(object[] handlers)
        {
            var disposeables = handlers.Select(o => RegisterInstance(o)).ToArray();
            return new RegisterToken(() => Array.ForEach<IDisposable>(disposeables, disposeable => disposeable.Dispose()));
        }

        public void UnregisterInstance(object handler)
        {
            _Descriptors.RemoveWhere(x => x.Reference.Target == handler);
        }

        //public void Unregister<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        //{
        //    if (_Subscribers.ContainsKey(typeof(TEvent)))
        //    {
        //        var handlers = _Subscribers[typeof(TEvent)];
        //        handlers.Remove(handler);

        //        if (handlers.Count == 0)
        //        {
        //            _Subscribers.Remove(typeof(TEvent));
        //        }
        //    }
        //}

        public void UnregisterAll()
        {
            //_Subscribers.Clear();
            _Descriptors.Clear();
        }

        public void Raise<TEvent>(TEvent e) where TEvent : IEvent
        {
            var handlers = _Descriptors
                .Where(x => x.EventType == typeof(TEvent) && x.Reference.IsAlive && x.Reference.Target != null)
                .Select(x => x.Reference.Target)
                .OfType<IEventHandler<TEvent>>();

            foreach (var item in handlers)
            {
                item.Handle(e);
            }

            //if (_Subscribers.ContainsKey(typeof(TEvent)))
            //{
            //    var handlers = _Subscribers[typeof(TEvent)];
            //    foreach (Action<TEvent> handler in handlers)
            //    {
            //        handler.Invoke(e);
            //    }
            //}
        }

        private class HandlerDescriptor
        {
            public HandlerDescriptor(Type eventType, object handler)
            {
                EventType = eventType;
                Reference = new WeakReference(handler);
            }

            public readonly Type EventType;
            public readonly WeakReference Reference;
        }

        private class RegisterToken : IDisposable
        {
            private readonly Action _DisposeDelegate;

            public RegisterToken(Action disposeDelegate)
            {
                _DisposeDelegate = disposeDelegate;
            }

            public void Dispose()
            {
                if (_DisposeDelegate != null)
                {
                    _DisposeDelegate();
                }
            }
        }
    }
}
