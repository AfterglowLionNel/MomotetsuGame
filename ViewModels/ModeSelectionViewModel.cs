using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// モード選択画面のViewModel
    /// </summary>
    public class ModeSelectionViewModel : INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private readonly IAudioService _audioService;
        private readonly ISaveDataService _saveDataService;
        private bool _hasSaveData;

        public ModeSelectionViewModel(
            INavigationService navigationService,
            IAudioService audioService,
            ISaveDataService saveDataService)
        {
            _navigationService = navigationService;
            _audioService = audioService;
            _saveDataService = saveDataService;

            InitializeCommands();
            CheckSaveData();

            // タイトルBGMを再生
            _audioService.PlayBgm("title_theme", true);
        }

        #region Properties

        public bool HasSaveData
        {
            get => _hasSaveData;
            set => SetProperty(ref _hasSaveData, value);
        }

        #endregion

        #region Commands

        public ICommand ContinueGameCommand { get; private set; } = null!;
        public ICommand SinglePlayCommand { get; private set; } = null!;
        public ICommand MultiPlayCommand { get; private set; } = null!;
        public ICommand LocalPlayCommand { get; private set; } = null!;
        public ICommand OnlinePlayCommand { get; private set; } = null!;
        public ICommand ShowExtrasCommand { get; private set; } = null!;
        public ICommand ShowTutorialCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            ContinueGameCommand = new RelayCommand(
                async () => await ContinueGame(),
                () => HasSaveData);

            SinglePlayCommand = new RelayCommand(
                async () => await StartSinglePlay());

            MultiPlayCommand = new RelayCommand(
                () => ShowNotImplemented("みんなで桃鉄"),
                () => false); // 未実装

            LocalPlayCommand = new RelayCommand(
                () => ShowNotImplemented("ちかくで桃鉄"),
                () => false); // 未実装

            OnlinePlayCommand = new RelayCommand(
                () => ShowNotImplemented("ネットで桃鉄"),
                () => false); // 未実装

            ShowExtrasCommand = new RelayCommand(
                () => ShowNotImplemented("おまけ"));

            ShowTutorialCommand = new RelayCommand(
                async () => await ShowTutorial());
        }

        #endregion

        #region Methods

        private async void CheckSaveData()
        {
            var saves = await _saveDataService.GetSaveDataListAsync();
            HasSaveData = saves.Length > 0;
        }

        private async Task ContinueGame()
        {
            _audioService.PlaySe("decide");
            await _navigationService.NavigateToAsync("LoadGameView");
        }

        private async Task StartSinglePlay()
        {
            _audioService.PlaySe("decide");
            await _navigationService.NavigateToAsync("GameSettingsView", GameMode.SinglePlay);
        }

        private async Task ShowTutorial()
        {
            _audioService.PlaySe("decide");
            await _navigationService.NavigateToAsync("TutorialView");
        }

        private void ShowNotImplemented(string feature)
        {
            _audioService.PlaySe("cancel");
            // TODO: メッセージ表示
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}