using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Views;
using MomotetsuGame.ViewModels;
using MomotetsuGame.Application.DependencyInjection;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// WPFナビゲーションサービスの実装
    /// </summary>
    public class SimpleNavigationService : INavigationService
    {
        private readonly Stack<Window> _navigationStack = new();
        private Window? _currentWindow;

        public bool CanGoBack => _navigationStack.Count > 0;

        public async Task NavigateToAsync(string viewName, object? parameter = null)
        {
            Window? newWindow = null;
            object? viewModel = null;

            // ビュー名に基づいてウィンドウとViewModelを作成
            switch (viewName)
            {
                case "ModeSelectionView":
                    viewModel = ServiceContainer.GetService<ModeSelectionViewModel>();
                    newWindow = new ModeSelectionWindow { DataContext = viewModel };
                    break;

                case "GameSettingsView":
                    viewModel = ServiceContainer.GetService<GameSettingsViewModel>();
                    if (viewModel is GameSettingsViewModel gsvm && parameter is Core.Enums.GameMode gameMode)
                    {
                        gsvm.GameMode = gameMode;
                    }
                    newWindow = new GameSettingsWindow { DataContext = viewModel };
                    break;

                case "MainGameView":
                    viewModel = ServiceContainer.GetService<MainWindowViewModel>();
                    newWindow = new MainWindow { DataContext = viewModel };
                    break;

                case "LoadGameView":
                    // TODO: LoadGameWindow実装
                    await ShowNotImplementedAsync("セーブデータ読み込み");
                    return;

                case "TutorialView":
                    // TODO: TutorialWindow実装
                    await ShowNotImplementedAsync("チュートリアル");
                    return;

                default:
                    throw new ArgumentException($"Unknown view: {viewName}");
            }

            if (newWindow != null)
            {
                // 現在のウィンドウをスタックに保存
                if (_currentWindow != null)
                {
                    _navigationStack.Push(_currentWindow);
                    _currentWindow.Hide();
                }

                // 新しいウィンドウを表示
                _currentWindow = newWindow;
                _currentWindow.Closed += OnWindowClosed;
                _currentWindow.Show();
            }

            await Task.CompletedTask;
        }

        public async Task GoBackAsync()
        {
            if (!CanGoBack)
                return;

            // 現在のウィンドウを閉じる
            if (_currentWindow != null)
            {
                _currentWindow.Closed -= OnWindowClosed;
                _currentWindow.Close();
            }

            // 前のウィンドウを復元
            _currentWindow = _navigationStack.Pop();
            _currentWindow?.Show();

            await Task.CompletedTask;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            // ウィンドウが閉じられた場合、アプリケーションを終了するか前の画面に戻る
            if (_navigationStack.Count > 0)
            {
                _currentWindow = _navigationStack.Pop();
                _currentWindow?.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private async Task ShowNotImplementedAsync(string feature)
        {
            var dialogService = ServiceContainer.GetService<IDialogService>();
            await dialogService.ShowInformationAsync(
                $"{feature}機能は現在実装中です。",
                "未実装機能");
        }
    }
}