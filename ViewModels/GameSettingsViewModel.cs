using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// ゲーム設定画面のViewModel
    /// </summary>
    public class GameSettingsViewModel : INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private readonly IAudioService _audioService;
        private readonly IGameManager _gameManager;
        private GameMode _gameMode;
        private int _selectedYears = 10;
        private int _customYears = 30;
        private ComDifficulty _comDifficulty = ComDifficulty.Normal;
        private int _comCount = 3;
        private string _playerName = "プレイヤー";
        private PlayerColor _playerColor = PlayerColor.Blue;

        public GameSettingsViewModel(
            INavigationService navigationService,
            IAudioService audioService,
            IGameManager gameManager)
        {
            _navigationService = navigationService;
            _audioService = audioService;
            _gameManager = gameManager;

            InitializeCommands();
            InitializeCollections();
        }

        #region Properties

        public GameMode GameMode
        {
            get => _gameMode;
            set => SetProperty(ref _gameMode, value);
        }

        public int SelectedYears
        {
            get => _selectedYears;
            set => SetProperty(ref _selectedYears, value);
        }

        public int CustomYears
        {
            get => _customYears;
            set
            {
                if (value >= 1 && value <= 100)
                {
                    SetProperty(ref _customYears, value);
                }
            }
        }

        public ComDifficulty ComDifficulty
        {
            get => _comDifficulty;
            set => SetProperty(ref _comDifficulty, value);
        }

        public int ComCount
        {
            get => _comCount;
            set
            {
                if (value >= 1 && value <= 3)
                {
                    SetProperty(ref _comCount, value);
                }
            }
        }

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public PlayerColor PlayerColor
        {
            get => _playerColor;
            set => SetProperty(ref _playerColor, value);
        }

        public ObservableCollection<ComDifficultyOption> DifficultyOptions { get; }
        public ObservableCollection<PlayerColorOption> ColorOptions { get; }

        #endregion

        #region Commands

        public ICommand StartGameCommand { get; private set; } = null!;
        public ICommand BackCommand { get; private set; } = null!;
        public ICommand IncreaseComCountCommand { get; private set; } = null!;
        public ICommand DecreaseComCountCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(
                async () => await StartGame(),
                () => !string.IsNullOrWhiteSpace(PlayerName));

            BackCommand = new RelayCommand(
                async () => await GoBack());

            IncreaseComCountCommand = new RelayCommand(
                () => ComCount++,
                () => ComCount < 3);

            DecreaseComCountCommand = new RelayCommand(
                () => ComCount--,
                () => ComCount > 1);
        }

        #endregion

        #region Methods

        private void InitializeCollections()
        {
            DifficultyOptions = new ObservableCollection<ComDifficultyOption>
            {
                new() { Value = ComDifficulty.Weak, DisplayName = "よわい" },
                new() { Value = ComDifficulty.Normal, DisplayName = "ふつう" },
                new() { Value = ComDifficulty.Strong, DisplayName = "つよい" }
            };

            ColorOptions = new ObservableCollection<PlayerColorOption>
            {
                new() { Value = PlayerColor.Blue, DisplayName = "青", ColorCode = "#0000FF" },
                new() { Value = PlayerColor.Red, DisplayName = "赤", ColorCode = "#FF0000" },
                new() { Value = PlayerColor.Green, DisplayName = "緑", ColorCode = "#00FF00" },
                new() { Value = PlayerColor.Yellow, DisplayName = "黄", ColorCode = "#FFFF00" }
            };
        }

        private async Task StartGame()
        {
            _audioService.PlaySe("game_start");

            var settings = new GameSettings
            {
                GameMode = GameMode,
                MaxYears = SelectedYears == 0 ? CustomYears : SelectedYears,
                ComDifficulty = ComDifficulty,
                ComPlayerCount = ComCount,
                PlayerName = PlayerName,
                PlayerColor = PlayerColor
            };

            // ゲームを開始
            await _gameManager.StartNewGameAsync(settings);

            // メインゲーム画面へ遷移
            await _navigationService.NavigateToAsync("MainGameView");
        }

        private async Task GoBack()
        {
            _audioService.PlaySe("cancel");
            await _navigationService.GoBackAsync();
        }

        #endregion

        #region Helper Classes

        public class ComDifficultyOption
        {
            public ComDifficulty Value { get; set; }
            public string DisplayName { get; set; } = string.Empty;
        }

        public class PlayerColorOption
        {
            public PlayerColor Value { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public string ColorCode { get; set; } = string.Empty;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            CommandManager.InvalidateRequerySuggested();
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