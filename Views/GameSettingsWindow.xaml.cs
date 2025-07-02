using System.Windows;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// GameSettingsWindow.xaml �̑��ݍ�p���W�b�N
    /// </summary>
    public partial class GameSettingsWindow : Window, IParameterReceiver
    {
        private GameSettingsViewModel? _viewModel;

        public GameSettingsWindow()
        {
            InitializeComponent();

            // ViewModel��ݒ�
            _viewModel = new GameSettingsViewModel();
            DataContext = _viewModel;

            // �C�x���g���w��
            _viewModel.CloseRequested += OnCloseRequested;
            _viewModel.GameStartRequested += OnGameStartRequested;
        }

        /// <summary>
        /// �p�����[�^����M
        /// </summary>
        public void ReceiveParameter(object parameter)
        {
            _viewModel?.ReceiveParameter(parameter);
        }

        /// <summary>
        /// ����v�����̏���
        /// </summary>
        private void OnCloseRequested(bool result)
        {
            DialogResult = result;
            Close();
        }

        /// <summary>
        /// �Q�[���J�n�v�����̏���
        /// </summary>
        private void OnGameStartRequested(Core.Entities.GameSettings settings)
        {
            // ���C���Q�[����ʂ�\��
            var mainWindow = new MainWindow();

            // GameManager�ɃQ�[���ݒ��n���i��Ŏ����j
            if (mainWindow.DataContext is MainWindowViewModel mainViewModel)
            {
                mainViewModel.StartNewGame(settings);
            }

            // �A�v���P�[�V�����̃��C���E�B���h�E��؂�ւ�
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            // ���̉�ʂƃ��[�h�I����ʂ����
            DialogResult = true;
            Close();
        }
    }
}