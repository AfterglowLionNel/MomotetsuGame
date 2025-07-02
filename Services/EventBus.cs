using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// �C�x���g�o�X����
    /// �A�v���P�[�V�����S�̂ł̃C�x���g�ʒm���Ǘ�
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
        /// �C�x���g�𔭍s
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
                    return; // �n���h���[���o�^����Ă��Ȃ��ꍇ�͉������Ȃ�
                }

                // �n���h���[�̃R�s�[���쐬�i�C�e���[�V�������̕ύX��h���j
                handlers = handlers.ToList();
            }

            // �n���h���[�����s
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
                    // ���O�ɋL�^�i��Ń��K�[������������g�p�j
                    System.Diagnostics.Debug.WriteLine($"EventBus: Error in handler for {typeof(TEvent).Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// �C�x���g���w��
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
        /// �C�x���g�̍w�ǂ�����
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

                    // �n���h���[���Ȃ��Ȃ����玫������폜
                    if (handlers.Count == 0)
                    {
                        _handlers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// ����̃C�x���g�^�C�v�̂��ׂẴn���h���[���N���A
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
        /// ���ׂẴn���h���[���N���A
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
    /// �C�x���g�o�X�Ŏg�p���鋤�ʃC�x���g
    /// </summary>
    public static class CommonEvents
    {
        /// <summary>
        /// �Q�[���J�n�C�x���g
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
        /// �Q�[���I���C�x���g
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
        /// ��ʑJ�ڃC�x���g
        /// </summary>
        public class NavigationEvent
        {
            public string FromView { get; set; } = string.Empty;
            public string ToView { get; set; } = string.Empty;
            public object? Parameter { get; set; }
        }

        /// <summary>
        /// �G���[�����C�x���g
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