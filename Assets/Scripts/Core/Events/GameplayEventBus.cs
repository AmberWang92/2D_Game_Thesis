using System;
using System.Collections.Generic;

namespace TopDownShooter.Core.Events
{
    public sealed class GameplayEventBus
    {
        public static GameplayEventBus Global { get; } = new GameplayEventBus();

        private readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);

            if (subscribers.TryGetValue(eventType, out Delegate existing))
            {
                subscribers[eventType] = Delegate.Combine(existing, handler);
                return;
            }

            subscribers[eventType] = handler;
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);

            if (!subscribers.TryGetValue(eventType, out Delegate existing))
            {
                return;
            }

            Delegate updated = Delegate.Remove(existing, handler);

            if (updated == null)
            {
                subscribers.Remove(eventType);
                return;
            }

            subscribers[eventType] = updated;
        }

        public void Publish<TEvent>(TEvent gameplayEvent)
        {
            if (subscribers.TryGetValue(typeof(TEvent), out Delegate existing))
            {
                ((Action<TEvent>)existing)?.Invoke(gameplayEvent);
            }
        }
    }
}
