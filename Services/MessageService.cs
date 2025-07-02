using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// �Q�[�������b�Z�[�W�\���T�[�r�X
    /// </summary>
    public class MessageService : INotifyPropertyChanged
    {
        private string _currentMessage = string.Empty;
        private MessageType _currentMessageType = MessageType.Info;
        private readonly Queue<GameMessage> _messageQueue;
        private readonly DispatcherTimer _displayTimer;
        private readonly object _lock = new object();

        /// <summary>
        /// ���ݕ\�����̃��b�Z�[�W
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            private set => SetProperty(ref _currentMessage, value);
        }

        /// <summary>
        /// ���݂̃��b�Z�[�W�^�C�v
        /// </summary>
        public MessageType CurrentMessageType
        {
            get => _currentMessageType;
            private set => SetProperty(ref _currentMessageType, value);
        }

        /// <summary>
        /// ���b�Z�[�W���\������Ă��邩
        /// </summary>
        public bool HasMessage => !string.IsNullOrEmpty(CurrentMessage);

        /// <summary>
        /// ���b�Z�[�W����
        /// </summary>
        public List<GameMessage> MessageHistory { get; }

        public MessageService()
        {
            _messageQueue = new Queue<GameMessage>();
            MessageHistory = new List<GameMessage>();

            _displayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // �f�t�H���g�\������
            };
            _displayTimer.Tick += OnDisplayTimerTick;
        }

        /// <summary>
        /// ��񃁃b�Z�[�W��\��
        /// </summary>
        public void ShowInfo(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Info, displaySeconds);
        }

        /// <summary>
        /// �������b�Z�[�W��\��
        /// </summary>
        public void ShowSuccess(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Success, displaySeconds);
        }

        /// <summary>
        /// �x�����b�Z�[�W��\��
        /// </summary>
        public void ShowWarning(string message, int displaySeconds = 3)
        {
            ShowMessage(message, MessageType.Warning, displaySeconds);
        }

        /// <summary>
        /// �G���[���b�Z�[�W��\��
        /// </summary>
        public void ShowError(string message, int displaySeconds = 5)
        {
            ShowMessage(message, MessageType.Error, displaySeconds);
        }

        /// <summary>
        /// �d�v���b�Z�[�W��\���i�����I�ɏ����Ȃ��j
        /// </summary>
        public void ShowImportant(string message)
        {
            ShowMessage(message, MessageType.Important, 0);
        }

        /// <summary>
        /// ���b�Z�[�W��\��
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
                // �d�v���b�Z�[�W�̏ꍇ�͑����ɕ\��
                if (type == MessageType.Important)
                {
                    _displayTimer.Stop();
                    DisplayMessage(gameMessage);
                }
                else
                {
                    _messageQueue.Enqueue(gameMessage);

                    // ���݃��b�Z�[�W���\������Ă��Ȃ��ꍇ�͑����ɕ\��
                    if (!_displayTimer.IsEnabled && string.IsNullOrEmpty(CurrentMessage))
                    {
                        ProcessNextMessage();
                    }
                }
            }
        }

        /// <summary>
        /// ���݂̃��b�Z�[�W���N���A
        /// </summary>
        public void ClearMessage()
        {
            _displayTimer.Stop();
            CurrentMessage = string.Empty;
            CurrentMessageType = MessageType.Info;
            OnPropertyChanged(nameof(HasMessage));
        }

        /// <summary>
        /// ���̃��b�Z�[�W������
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
        /// ���b�Z�[�W��\��
        /// </summary>
        private void DisplayMessage(GameMessage message)
        {
            CurrentMessage = message.Message;
            CurrentMessageType = message.Type;
            OnPropertyChanged(nameof(HasMessage));

            // �����ɒǉ�
            MessageHistory.Add(message);
            if (MessageHistory.Count > 100) // �ő�100���܂ŕێ�
            {
                MessageHistory.RemoveAt(0);
            }

            // ���������^�C�}�[�ݒ�
            if (message.DisplaySeconds > 0)
            {
                _displayTimer.Interval = TimeSpan.FromSeconds(message.DisplaySeconds);
                _displayTimer.Start();
            }
        }

        /// <summary>
        /// �^�C�}�[�e�B�b�N�C�x���g
        /// </summary>
        private void OnDisplayTimerTick(object? sender, EventArgs e)
        {
            _displayTimer.Stop();
            ProcessNextMessage();
        }

        /// <summary>
        /// ���b�Z�[�W�^�C�v�ɉ������F���擾
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
    /// �Q�[�����b�Z�[�W
    /// </summary>
    public class GameMessage
    {
        public string Message { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public int DisplaySeconds { get; set; }
    }

    /// <summary>
    /// ���b�Z�[�W�^�C�v
    /// </summary>
    public enum MessageType
    {
        Info,      // ���
        Success,   // ����
        Warning,   // �x��
        Error,     // �G���[
        Important  // �d�v�i�蓮�ŃN���A���K�v�j
    }
}