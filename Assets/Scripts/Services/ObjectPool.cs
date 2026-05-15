using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownShooter.Services
{
    /// <summary>
    /// Generic component pool with factory + lifecycle hooks.
    /// Constructor must not invoke the factory before the caller has a chance
    /// to assign the pool reference (so factories may close over the pool
    /// to wire self-release callbacks). Use <see cref="Prewarm"/> after
    /// construction to populate the pool.
    /// </summary>
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Stack<T> _stack = new();

        public int CountInactive => _stack.Count;

        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onRelease = onRelease;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var inst = _factory();
                if (!inst) continue;
                _onRelease?.Invoke(inst);
                _stack.Push(inst);
            }
        }

        public T Get()
        {
            T inst = _stack.Count > 0 ? _stack.Pop() : _factory();
            _onGet?.Invoke(inst);
            return inst;
        }

        public void Release(T inst)
        {
            if (!inst) return;
            _onRelease?.Invoke(inst);
            _stack.Push(inst);
        }
    }
}
