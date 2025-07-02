using System.Windows;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// ModeSelectionWindow.xaml �̑��ݍ�p���W�b�N
    /// </summary>
    public partial class ModeSelectionWindow : Window
    {
        public ModeSelectionWindow()
        {
            InitializeComponent();

            // ViewModel��ݒ�
            var viewModel = new ModeSelectionViewModel();
            DataContext = viewModel;

            // �E�B���h�E�����C�x���g���w��
            viewModel.CloseRequested += () => this.Close();
            viewModel.NavigateRequested += OnNavigateRequested;
        }

        private void OnNavigateRequested(string viewName, object? parameter)
        {
            Window? nextWindow = viewName switch
            {
                "GameSettings" => new GameSettingsWindow(),
                "LoadGame" => null, // TODO: LoadGameWindow
                "Tutorial" => null, // TODO: TutorialWindow
                "Extras" => null,   // TODO: ExtrasWindow
                _ => null
            };

            if (nextWindow != null)
            {
                nextWindow.Owner = this;
                if (parameter != null && nextWindow.DataContext is IParameterReceiver receiver)
                {
                    receiver.ReceiveParameter(parameter);
                }

                Hide();
                var result = nextWindow.ShowDialog();

                // �Q�[�����J�n���ꂽ�ꍇ�͂��̃E�B���h�E������
                if (result == true && viewName == "GameSettings")
                {
                    Close();
                }
                else
                {
                    Show();
                }
            }
        }
    }

    /// <summary>
    /// �p�����[�^��M�C���^�[�t�F�[�X
    /// </summary>
    public interface IParameterReceiver
    {
        void ReceiveParameter(object parameter);
    }
}