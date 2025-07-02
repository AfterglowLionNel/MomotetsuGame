using System.Windows;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// GameSettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class GameSettingsWindow : Window, IParameterReceiver
    {
        private GameSettingsViewModel? _viewModel;

        public GameSettingsWindow()
        {
            InitializeComponent();

            // ViewModelを設定
            _viewModel = new GameSettingsViewModel();
            DataContext = _viewModel;

            // イベントを購読
            _viewModel.CloseRequested += OnCloseRequested;
            _viewModel.GameStartRequested += OnGameStartRequested;
        }

        /// <summary>
        /// パラメータを受信
        /// </summary>
        public void ReceiveParameter(object parameter)
        {
            _viewModel?.ReceiveParameter(parameter);
        }

        /// <summary>
        /// 閉じる要求時の処理
        /// </summary>
        private void OnCloseRequested(bool result)
        {
            DialogResult = result;
            Close();
        }

        /// <summary>
        /// ゲーム開始要求時の処理
        /// </summary>
        private void OnGameStartRequested(Core.Entities.GameSettings settings)
        {
            // メインゲーム画面を表示
            var mainWindow = new MainWindow();

            // GameManagerにゲーム設定を渡す（後で実装）
            if (mainWindow.DataContext is MainWindowViewModel mainViewModel)
            {
                mainViewModel.StartNewGame(settings);
            }

            // アプリケーションのメインウィンドウを切り替え
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            // この画面とモード選択画面を閉じる
            DialogResult = true;
            Close();
        }
    }
}