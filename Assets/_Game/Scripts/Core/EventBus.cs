using System;
using System.Collections.Generic;

namespace Capybrawlers.Core
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T evt);
    }

    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
                return;

            // Iterate over a snapshot so handlers can safely unsubscribe during dispatch.
            var snapshot = list.ToArray();
            foreach (var handler in snapshot)
                ((Action<T>)handler)(evt);
        }
    }
}
