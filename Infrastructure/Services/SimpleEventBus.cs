using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// �V���v���ȃC�x���g�o�X����
    /// </summary>
    public class SimpleEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            List<Delegate> handlers;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out handlers))
                    return;

                // �n���h���[���X�g�̃R�s�[���쐬�i�C�e���[�V�������̕ύX��h���j
                handlers = handlers.ToList();
            }

            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Action<TEvent> action)
                    {
                        action(eventData);
                    }
                    else if (handler is Func<TEvent, Task> asyncHandler)
                    {
                        // �񓯊��n���h���[�͓����I�ɊJ�n�ifire-and-forget�j
                        Task.Run(() => asyncHandler(eventData));
                    }
                }
                catch (Exception ex)
                {
                    // �n���h���[�̃G���[�����O�ɋL�^�i���ۂ̃��O�����͌�Œǉ��j
                    System.Diagnostics.Debug.WriteLine($"EventBus handler error: {ex}");
                }
            }
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }

                _handlers[eventType].Add(handler);
            }

            return new Unsubscriber(() =>
            {
                lock (_lock)
                {
                    if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
                    {
                        handlers.Remove(handler);
                        if (handlers.Count == 0)
                        {
                            _handlers.Remove(typeof(TEvent));
                        }
                    }
                }
            });
        }

        public IDisposable SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }

                _handlers[eventType].Add(handler);
            }

            return new Unsubscriber(() =>
            {
                lock (_lock)
                {
                    if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
                    {
                        handlers.Remove(handler);
                        if (handlers.Count == 0)
                        {
                            _handlers.Remove(typeof(TEvent));
                        }
                    }
                }
            });
        }

        /// <summary>
        /// �w�ǉ����p�̃w���p�[�N���X
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Unsubscriber(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _unsubscribe?.Invoke();
                    _disposed = true;
                }
            }
        }
    }
}