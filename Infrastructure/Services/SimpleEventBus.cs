using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// シンプルなイベントバス実装
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

                // ハンドラーリストのコピーを作成（イテレーション中の変更を防ぐ）
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
                        // 非同期ハンドラーは同期的に開始（fire-and-forget）
                        Task.Run(() => asyncHandler(eventData));
                    }
                }
                catch (Exception ex)
                {
                    // ハンドラーのエラーをログに記録（実際のログ実装は後で追加）
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
        /// 購読解除用のヘルパークラス
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