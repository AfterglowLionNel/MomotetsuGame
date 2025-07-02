using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// ゲーム内メッセージ表示サービス
    /// </summary>
    public class MessageService : INotifyPropertyChanged
    {
        private string _currentMessage = string.Empty;
        private MessageType _currentMessageType = MessageType.Info;
        private readonly Queue<GameMessage> _messageQueue;
        private readonly DispatcherTimer _displayTimer;
        private readonly object _lock = new object();

        /// <summary>
        /// 現在表示中のメッセージ
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            private set => SetProperty(ref _currentMessage, value);
        }

        /// <summary>
        /// 現在のメッセージタイプ
        /// </summary>
        public MessageType CurrentMessageType
        {
            get => _currentMessageType;
            private set => SetProperty(ref _currentMessageType, value);
        }

        /// <summary>
        /// メッセージが表示されているか
        /// </summary>
        public bool HasMessage => !string.IsNullOrEmpty(CurrentMessage);

        /// <summary>
        /// メッセージ履歴
        /// </summary>
        public List<GameMessage> MessageHistory { get; }

        public MessageService()
        {
            _messageQueue = new Queue<GameMessage>();
            MessageHistory = new List<GameMessage>();

            _displayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // デフォルト表示時間
            };
            _displayTimer.Tick += OnDisplayTimerTick;
        }

        /// <summary>
        /// 情報メッセージを表示
        /// </summary>
        public void ShowInfo(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Info, displaySeconds);
        }

        /// <summary>
        /// 成功メッセージを表示
        /// </summary>
        public void ShowSuccess(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Success, displaySeconds);
        }

        /// <summary>
        /// 警告メッセージを表示
        /// </summary>
        public void ShowWarning(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Warning, displaySeconds);
        }

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        public void ShowError(string message, int displaySeconds = 5)
        {
            ShowMessage(message, MessageType.Error, displaySeconds);
        }

        /// <summary>
        /// 重要メッセージを表示（自動的に消えない）
        /// </summary>
        public void ShowImportant(string message)
        {
            ShowMessage(message, MessageType.Important, 0);
        }

        /// <summary>
        /// メッセージを表示
        /// </summary>
        private void ShowMessage(string message, MessageType type, int displaySeconds)
        {
            var gameMessage = new GameMessage
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                DisplaySeconds = displaySeconds
            };

            lock (_lock)
            {
                // 重要メッセージの場合は即座に表示
                if (type == MessageType.Important)
                {
                    _displayTimer.Stop();
                    DisplayMessage(gameMessage);
                }
                else
                {
                    _messageQueue.Enqueue(gameMessage);

                    // 現在メッセージが表示されていない場合は即座に表示
                    if (!_displayTimer.IsEnabled && string.IsNullOrEmpty(CurrentMessage))
                    {
                        ProcessNextMessage();
                    }
                }
            }
        }

        /// <summary>
        /// 現在のメッセージをクリア
        /// </summary>
        public void ClearMessage()
        {
            _displayTimer.Stop();
            CurrentMessage = string.Empty;
            CurrentMessageType = MessageType.Info;
            OnPropertyChanged(nameof(HasMessage));
        }

        /// <summary>
        /// 次のメッセージを処理
        /// </summary>
        private void ProcessNextMessage()
        {
            lock (_lock)
            {
                if (_messageQueue.Count > 0)
                {
                    var message = _messageQueue.Dequeue();
                    DisplayMessage(message);
                }
                else
                {
                    ClearMessage();
                }
            }
        }

        /// <summary>
        /// メッセージを表示
        /// </summary>
        private void DisplayMessage(GameMessage message)
        {
            CurrentMessage = message.Message;
            CurrentMessageType = message.Type;
            OnPropertyChanged(nameof(HasMessage));

            // 履歴に追加
            MessageHistory.Add(message);
            if (MessageHistory.Count > 100) // 最大100件まで保持
            {
                MessageHistory.RemoveAt(0);
            }

            // 自動消去タイマー設定
            if (message.DisplaySeconds > 0)
            {
                _displayTimer.Interval = TimeSpan.FromSeconds(message.DisplaySeconds);
                _displayTimer.Start();
            }
        }

        /// <summary>
        /// タイマーティックイベント
        /// </summary>
        private void OnDisplayTimerTick(object? sender, EventArgs e)
        {
            _displayTimer.Stop();
            ProcessNextMessage();
        }

        /// <summary>
        /// メッセージタイプに応じた色を取得
        /// </summary>
        public static string GetMessageColor(MessageType type)
        {
            return type switch
            {
                MessageType.Success => "#FF32CD32",   // LimeGreen
                MessageType.Warning => "#FFFFA500",   // Orange
                MessageType.Error => "#FFDC143C",     // Crimson
                MessageType.Important => "#FFFF1493", // DeepPink
                _ => "#FF4169E1"                      // RoyalBlue (Info)
            };
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// ゲームメッセージ
    /// </summary>
    public class GameMessage
    {
        public string Message { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public int DisplaySeconds { get; set; }
    }

    /// <summary>
    /// メッセージタイプ
    /// </summary>
    public enum MessageType
    {
        Info,      // 情報
        Success,   // 成功
        Warning,   // 警告
        Error,     // エラー
        Important  // 重要（手動でクリアが必要）
    }
}