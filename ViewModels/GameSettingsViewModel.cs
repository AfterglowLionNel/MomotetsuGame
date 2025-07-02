using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MomotetsuGame.Application.Commands;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Services;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// ゲーム設定画面のViewModel
    /// </summary>
    public class GameSettingsViewModel : INotifyPropertyChanged
    {
        private readonly GameConfiguration _gameConfig;

        #region Properties

        // プレイ年数
        private bool _isYear3Selected;
        private bool _isYear5Selected;
        private bool _isYear10Selected = true;
        private bool _isCustomYearSelected;
        private int _customYears = 20;

        public bool IsYear3Selected
        {
            get => _isYear3Selected;
            set => SetProperty(ref _isYear3Selected, value);
        }

        public bool IsYear5Selected
        {
            get => _isYear5Selected;
            set => SetProperty(ref _isYear5Selected, value);
        }

        public bool IsYear10Selected
        {
            get => _isYear10Selected;
            set => SetProperty(ref _isYear10Selected, value);
        }

        public bool IsCustomYearSelected
        {
            get => _isCustomYearSelected;
            set
            {
                SetProperty(ref _isCustomYearSelected, value);
                OnPropertyChanged(nameof(CustomYearVisibility));
            }
        }

        public int CustomYears
        {
            get => _customYears;
            set => SetProperty(ref _customYears, Math.Clamp(value, 1, 100));
        }

        public Visibility CustomYearVisibility => IsCustomYearSelected ? Visibility.Visible : Visibility.Collapsed;

        // プレイヤー設定
        private string _playerName = "プレイヤー";
        private PlayerColorOption? _selectedPlayerColor;

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public ObservableCollection<PlayerColorOption> PlayerColors { get; }

        public PlayerColorOption? SelectedPlayerColor
        {
            get => _selectedPlayerColor;
            set => SetProperty(ref _selectedPlayerColor, value);
        }

        // COM設定
        private bool _isCom1Selected;
        private bool _isCom2Selected;
        private bool _isCom3Selected = true;

        public bool IsCom1Selected
        {
            get => _isCom1Selected;
            set => SetProperty(ref _isCom1Selected, value);
        }

        public bool IsCom2Selected
        {
            get => _isCom2Selected;
            set => SetProperty(ref _isCom2Selected, value);
        }

        public bool IsCom3Selected
        {
            get => _isCom3Selected;
            set => SetProperty(ref _isCom3Selected, value);
        }

        // COM強さ
        private bool _isComEasySelected;
        private bool _isComNormalSelected = true;
        private bool _isComHardSelected;

        public bool IsComEasySelected
        {
            get => _isComEasySelected;
            set => SetProperty(ref _isComEasySelected, value);
        }

        public bool IsComNormalSelected
        {
            get => _isComNormalSelected;
            set => SetProperty(ref _isComNormalSelected, value);
        }

        public bool IsComHardSelected
        {
            get => _isComHardSelected;
            set => SetProperty(ref _isComHardSelected, value);
        }

        // 特殊ルール
        private bool _enableBonby = true;
        private bool _enableSpecialEvents = true;

        public bool EnableBonby
        {
            get => _enableBonby;
            set => SetProperty(ref _enableBonby, value);
        }

        public bool EnableSpecialEvents
        {
            get => _enableSpecialEvents;
            set => SetProperty(ref _enableSpecialEvents, value);
        }

        #endregion

        #region Commands

        public ICommand StartGameCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Events

        public event Action<bool>? CloseRequested;
        public event Action<GameSettings>? GameStartRequested;

        #endregion

        public GameSettingsViewModel()
        {
            _gameConfig = GameConfiguration.Instance;

            // プレイヤー色の初期化
            PlayerColors = new ObservableCollection<PlayerColorOption>
            {
                new PlayerColorOption(PlayerColor.Blue, Colors.RoyalBlue),
                new PlayerColorOption(PlayerColor.Red, Colors.Crimson),
                new PlayerColorOption(PlayerColor.Yellow, Colors.Gold),
                new PlayerColorOption(PlayerColor.Green, Colors.LimeGreen),
                new PlayerColorOption(PlayerColor.Purple, Colors.MediumPurple),
                new PlayerColorOption(PlayerColor.Orange, Colors.DarkOrange)
            };
            SelectedPlayerColor = PlayerColors.First();

            // コマンドの初期化
            StartGameCommand = new RelayCommand(StartGame, CanStartGame);
            CancelCommand = new RelayCommand(Cancel);

            // デフォルト値を設定から読み込み
            LoadDefaults();
        }

        /// <summary>
        /// パラメータを受信
        /// </summary>
        public void ReceiveParameter(object parameter)
        {
            // 必要に応じてパラメータを処理
        }

        /// <summary>
        /// デフォルト値を読み込み
        /// </summary>
        private void LoadDefaults()
        {
            var settings = _gameConfig.Config.GameSettings;

            // プレイ年数
            switch (settings.DefaultYears)
            {
                case 3:
                    IsYear3Selected = true;
                    break;
                case 5:
                    IsYear5Selected = true;
                    break;
                case 10:
                    IsYear10Selected = true;
                    break;
                default:
                    IsCustomYearSelected = true;
                    CustomYears = settings.DefaultYears;
                    break;
            }

            // プレイヤー設定
            var playerSettings = _gameConfig.Config.PlayerSettings;
            if (!string.IsNullOrEmpty(playerSettings.LastUsedPlayerName))
            {
                PlayerName = playerSettings.LastUsedPlayerName;
            }
            else
            {
                PlayerName = playerSettings.DefaultPlayerName;
            }

            var defaultColor = PlayerColors.FirstOrDefault(c => c.Type == playerSettings.DefaultPlayerColor);
            if (defaultColor != null)
            {
                SelectedPlayerColor = defaultColor;
            }

            // COM設定
            switch (settings.DefaultComCount)
            {
                case 1:
                    IsCom1Selected = true;
                    break;
                case 2:
                    IsCom2Selected = true;
                    break;
                case 3:
                    IsCom3Selected = true;
                    break;
            }

            switch (settings.DefaultComDifficulty)
            {
                case ComDifficulty.Easy:
                    IsComEasySelected = true;
                    break;
                case ComDifficulty.Normal:
                    IsComNormalSelected = true;
                    break;
                case ComDifficulty.Hard:
                    IsComHardSelected = true;
                    break;
            }

            // 特殊ルール
            EnableBonby = settings.EnableBonby;
            EnableSpecialEvents = settings.EnableSpecialEvents;
        }

        /// <summary>
        /// ゲーム開始可能かチェック
        /// </summary>
        private bool CanStartGame()
        {
            return !string.IsNullOrWhiteSpace(PlayerName) && SelectedPlayerColor != null;
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        private void StartGame()
        {
            // ゲーム設定を作成
            var settings = new GameSettings
            {
                GameMode = GameMode.Normal,
                MaxYears = GetSelectedYears(),
                ComPlayerCount = GetSelectedComCount(),
                ComDifficulty = GetSelectedComDifficulty(),
                EnableBonby = EnableBonby,
                EnableSpecialEvents = EnableSpecialEvents
            };

            // プレイヤー情報を設定に追加
            settings.PlayerName = PlayerName;
            settings.PlayerColor = SelectedPlayerColor!.Type;

            // 設定を保存
            SaveSettings();

            // ゲーム開始イベントを発火
            GameStartRequested?.Invoke(settings);
        }

        /// <summary>
        /// キャンセル
        /// </summary>
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }

        /// <summary>
        /// 選択された年数を取得
        /// </summary>
        private int GetSelectedYears()
        {
            if (IsYear3Selected) return 3;
            if (IsYear5Selected) return 5;
            if (IsYear10Selected) return 10;
            return CustomYears;
        }

        /// <summary>
        /// 選択されたCOM数を取得
        /// </summary>
        private int GetSelectedComCount()
        {
            if (IsCom1Selected) return 1;
            if (IsCom2Selected) return 2;
            return 3;
        }

        /// <summary>
        /// 選択されたCOM難易度を取得
        /// </summary>
        private ComDifficulty GetSelectedComDifficulty()
        {
            if (IsComEasySelected) return ComDifficulty.Easy;
            if (IsComHardSelected) return ComDifficulty.Hard;
            return ComDifficulty.Normal;
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        private void SaveSettings()
        {
            var settings = _gameConfig.Config.GameSettings;
            settings.DefaultYears = GetSelectedYears();
            settings.DefaultComCount = GetSelectedComCount();
            settings.DefaultComDifficulty = GetSelectedComDifficulty();
            settings.EnableBonby = EnableBonby;
            settings.EnableSpecialEvents = EnableSpecialEvents;

            var playerSettings = _gameConfig.Config.PlayerSettings;
            playerSettings.LastUsedPlayerName = PlayerName;
            playerSettings.DefaultPlayerColor = SelectedPlayerColor!.Type;

            _gameConfig.Save();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// プレイヤー色オプション
    /// </summary>
    public class PlayerColorOption
    {
        public PlayerColor Type { get; }
        public Color Color { get; }

        public PlayerColorOption(PlayerColor type, Color color)
        {
            Type = type;
            Color = color;
        }
    }
}