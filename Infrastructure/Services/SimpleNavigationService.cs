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
    /// WPF�i�r�Q�[�V�����T�[�r�X�̎���
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

            // �r���[���Ɋ�Â��ăE�B���h�E��ViewModel���쐬
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
                    // TODO: LoadGameWindow����
                    await ShowNotImplementedAsync("�Z�[�u�f�[�^�ǂݍ���");
                    return;

                case "TutorialView":
                    // TODO: TutorialWindow����
                    await ShowNotImplementedAsync("�`���[�g���A��");
                    return;

                default:
                    throw new ArgumentException($"Unknown view: {viewName}");
            }

            if (newWindow != null)
            {
                // ���݂̃E�B���h�E���X�^�b�N�ɕۑ�
                if (_currentWindow != null)
                {
                    _navigationStack.Push(_currentWindow);
                    _currentWindow.Hide();
                }

                // �V�����E�B���h�E��\��
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

            // ���݂̃E�B���h�E�����
            if (_currentWindow != null)
            {
                _currentWindow.Closed -= OnWindowClosed;
                _currentWindow.Close();
            }

            // �O�̃E�B���h�E�𕜌�
            _currentWindow = _navigationStack.Pop();
            _currentWindow?.Show();

            await Task.CompletedTask;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            // �E�B���h�E������ꂽ�ꍇ�A�A�v���P�[�V�������I�����邩�O�̉�ʂɖ߂�
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
                $"{feature}�@�\�͌��ݎ������ł��B",
                "�������@�\");
        }
    }
}