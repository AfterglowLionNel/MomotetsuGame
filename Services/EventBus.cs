using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// イベントバス実装
    /// アプリケーション全体でのイベント通知を管理
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers;
        private readonly object _lock = new object();

        public EventBus()
        {
            _handlers = new Dictionary<Type, List<Delegate>>();
        }

        /// <summary>
        /// イベントを発行
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            List<Delegate>? handlers;

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_handlers.TryGetValue(eventType, out handlers))
                {
                    return; // ハンドラーが登録されていない場合は何もしない
                }

                // ハンドラーのコピーを作成（イテレーション中の変更を防ぐ）
                handlers = handlers.ToList();
            }

            // ハンドラーを実行
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Action<TEvent> action)
                    {
                        action(eventData);
                    }
                }
                catch (Exception ex)
                {
                    // ログに記録（後でロガーを実装したら使用）
                    System.Diagnostics.Debug.WriteLine($"EventBus: Error in handler for {typeof(TEvent).Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// イベントを購読
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }

                _handlers[eventType].Add(handler);
            }
        }

        /// <summary>
        /// イベントの購読を解除
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);

                    // ハンドラーがなくなったら辞書から削除
                    if (handlers.Count == 0)
                    {
                        _handlers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// 特定のイベントタイプのすべてのハンドラーをクリア
        /// </summary>
        public void ClearHandlers<TEvent>() where TEvent : class
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);
                _handlers.Remove(eventType);
            }
        }

        /// <summary>
        /// すべてのハンドラーをクリア
        /// </summary>
        public void ClearAllHandlers()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }
    }

    /// <summary>
    /// イベントバスで使用する共通イベント
    /// </summary>
    public static class CommonEvents
    {
        /// <summary>
        /// ゲーム開始イベント
        /// </summary>
        public class GameStartedEvent
        {
            public DateTime StartTime { get; set; }
            public string GameMode { get; set; } = string.Empty;

            public GameStartedEvent()
            {
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// ゲーム終了イベント
        /// </summary>
        public class GameEndedEvent
        {
            public DateTime EndTime { get; set; }
            public string Winner { get; set; } = string.Empty;

            public GameEndedEvent()
            {
                EndTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 画面遷移イベント
        /// </summary>
        public class NavigationEvent
        {
            public string FromView { get; set; } = string.Empty;
            public string ToView { get; set; } = string.Empty;
            public object? Parameter { get; set; }
        }

        /// <summary>
        /// エラー発生イベント
        /// </summary>
        public class ErrorOccurredEvent
        {
            public string ErrorMessage { get; set; } = string.Empty;
            public Exception? Exception { get; set; }
            public DateTime OccurredAt { get; set; }

            public ErrorOccurredEvent()
            {
                OccurredAt = DateTime.Now;
            }
        }
    }
}