using System;
using System.Windows;
using System.Windows.Threading;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// ���b�Z�[�W�ʒm�T�[�r�X�̎���
    /// </summary>
    public class SimpleMessageService : IMessageService
    {
        private readonly IEventBus _eventBus;
        private readonly DispatcherTimer _toastTimer;
        private Window? _currentToast;

        public SimpleMessageService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3);
            _toastTimer.Tick += (s, e) => HideToast();
        }

        public void ShowInfo(string message)
        {
            // �C�x���g�o�X�ɒʒm
            _eventBus.Publish(new MessageEvent
            {
                Message = message,
                Type = MessageType.Info,
                Timestamp = DateTime.Now
            });

            // �g�[�X�g�ʒm��\���i�I�v�V�����j
            ShowToast(message, MessageType.Info);
        }

        public void ShowWarning(string message)
        {
            _eventBus.Publish(new MessageEvent
            {
                Message = message,
                Type = MessageType.Warning,
                Timestamp = DateTime.Now
            });

            ShowToast(message, MessageType.Warning);
        }

        public void ShowError(string message)
        {
            _eventBus.Publish(new MessageEvent
            {
                Message = message,
                Type = MessageType.Error,
                Timestamp = DateTime.Now
            });

            // �G���[�̏ꍇ��MessageBox���\��
            MessageBox.Show(
                message,
                "�G���[",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void ShowSuccess(string message)
        {
            _eventBus.Publish(new MessageEvent
            {
                Message = message,
                Type = MessageType.Success,
                Timestamp = DateTime.Now
            });

            ShowToast(message, MessageType.Success);
        }

        private void ShowToast(string message, MessageType type)
        {
            // �����̃g�[�X�g������Ε���
            HideToast();

            // �g�[�X�g�ʒm�E�B���h�E���쐬�i�ȈՎ����j
            _currentToast = new ToastWindow
            {
                Message = message,
                MessageType = type
            };

            // ��ʉE���ɕ\��
            var workingArea = SystemParameters.WorkArea;
            _currentToast.Left = workingArea.Right - _currentToast.Width - 20;
            _currentToast.Top = workingArea.Bottom - _currentToast.Height - 20;

            _currentToast.Show();
            _toastTimer.Start();
        }

        private void HideToast()
        {
            _toastTimer.Stop();
            _currentToast?.Close();
            _currentToast = null;
        }
    }

    /// <summary>
    /// ���b�Z�[�W�C�x���g
    /// </summary>
    public class MessageEvent
    {
        public string Message { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// ���b�Z�[�W�^�C�v
    /// </summary>
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success
    }

    /// <summary>
    /// �g�[�X�g�ʒm�E�B���h�E�i�ȈՎ����j
    /// </summary>
    public class ToastWindow : Window
    {
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }

        public ToastWindow()
        {
            Width = 300;
            Height = 80;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;

            // ���ۂ̎����ł́A�K�؂�UI���\�z
            var border = new System.Windows.Controls.Border
            {
                Background = GetBackgroundBrush(),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10)
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = Message,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            Content = border;

            // �t�F�[�h�C���A�j���[�V����
            Opacity = 0;
            Loaded += (s, e) =>
            {
                var animation = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                BeginAnimation(OpacityProperty, animation);
            };
        }

        private System.Windows.Media.Brush GetBackgroundBrush()
        {
            return MessageType switch
            {
                MessageType.Success => System.Windows.Media.Brushes.Green,
                MessageType.Warning => System.Windows.Media.Brushes.Orange,
                MessageType.Error => System.Windows.Media.Brushes.Red,
                _ => System.Windows.Media.Brushes.Blue
            };
        }
    }
}