using System.Windows;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// ModeSelectionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ModeSelectionWindow : Window
    {
        public ModeSelectionWindow()
        {
            InitializeComponent();

            // ViewModelを設定
            var viewModel = new ModeSelectionViewModel();
            DataContext = viewModel;

            // ウィンドウを閉じるイベントを購読
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

                // ゲームが開始された場合はこのウィンドウも閉じる
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
    /// パラメータ受信インターフェース
    /// </summary>
    public interface IParameterReceiver
    {
        void ReceiveParameter(object parameter);
    }
}