using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// ゲーム設定管理サービス
    /// </summary>
    public class GameConfiguration : INotifyPropertyChanged
    {
        private static readonly string ConfigFileName = "game_config.json";
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MomotetsuGame",
            ConfigFileName);

        private static GameConfiguration? _instance;
        private GameConfig _config;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static GameConfiguration Instance => _instance ??= new GameConfiguration();

        /// <summary>
        /// 現在の設定
        /// </summary>
        public GameConfig Config
        {
            get => _config;
            private set => SetProperty(ref _config, value);
        }

        private GameConfiguration()
        {
            _config = LoadConfiguration();
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        private GameConfig LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<GameConfig>(json) ?? new GameConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定の読み込みに失敗しました: {ex.Message}");
            }

            return new GameConfig();
        }

        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefault()
        {
            Config = new GameConfig();
            Save();
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
    /// ゲーム設定
    /// </summary>
    public class GameConfig : INotifyPropertyChanged
    {
        private GameSettings _gameSettings;
        private AudioSettings _audioSettings;
        private DisplaySettings _displaySettings;
        private PlayerSettings _playerSettings;

        /// <summary>
        /// ゲーム設定
        /// </summary>
        public GameSettings GameSettings
        {
            get => _gameSettings;
            set => SetProperty(ref _gameSettings, value);
        }

        /// <summary>
        /// 音声設定
        /// </summary>
        public AudioSettings AudioSettings
        {
            get => _audioSettings;
            set => SetProperty(ref _audioSettings, value);
        }

        /// <summary>
        /// 表示設定
        /// </summary>
        public DisplaySettings DisplaySettings
        {
            get => _displaySettings;
            set => SetProperty(ref _displaySettings, value);
        }

        /// <summary>
        /// プレイヤー設定
        /// </summary>
        public PlayerSettings PlayerSettings
        {
            get => _playerSettings;
            set => SetProperty(ref _playerSettings, value);
        }

        public GameConfig()
        {
            _gameSettings = new GameSettings();
            _audioSettings = new AudioSettings();
            _displaySettings = new DisplaySettings();
            _playerSettings = new PlayerSettings();
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
    /// ゲーム設定
    /// </summary>
    public class GameSettings : INotifyPropertyChanged
    {
        private int _defaultYears = 3;
        private ComDifficulty _defaultComDifficulty = ComDifficulty.Normal;
        private int _defaultComCount = 3;
        private bool _enableBonby = true;
        private bool _enableSpecialEvents = true;
        private bool _autoSave = true;
        private int _autoSaveInterval = 10; // 分

        public int DefaultYears
        {
            get => _defaultYears;
            set => SetProperty(ref _defaultYears, Math.Clamp(value, 1, 100));
        }

        public ComDifficulty DefaultComDifficulty
        {
            get => _defaultComDifficulty;
            set => SetProperty(ref _defaultComDifficulty, value);
        }

        public int DefaultComCount
        {
            get => _defaultComCount;
            set => SetProperty(ref _defaultComCount, Math.Clamp(value, 1, 3));
        }

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

        public bool AutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => SetProperty(ref _autoSaveInterval, Math.Clamp(value, 1, 60));
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
    /// 音声設定
    /// </summary>
    public class AudioSettings : INotifyPropertyChanged
    {
        private float _masterVolume = 1.0f;
        private float _bgmVolume = 0.7f;
        private float _seVolume = 0.8f;
        private bool _enableBgm = true;
        private bool _enableSe = true;

        public float MasterVolume
        {
            get => _masterVolume;
            set => SetProperty(ref _masterVolume, Math.Clamp(value, 0f, 1f));
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set => SetProperty(ref _bgmVolume, Math.Clamp(value, 0f, 1f));
        }

        public float SeVolume
        {
            get => _seVolume;
            set => SetProperty(ref _seVolume, Math.Clamp(value, 0f, 1f));
        }

        public bool EnableBgm
        {
            get => _enableBgm;
            set => SetProperty(ref _enableBgm, value);
        }

        public bool EnableSe
        {
            get => _enableSe;
            set => SetProperty(ref _enableSe, value);
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
    /// 表示設定
    /// </summary>
    public class DisplaySettings : INotifyPropertyChanged
    {
        private bool _fullScreen = false;
        private int _windowWidth = 1600;
        private int _windowHeight = 900;
        private AnimationSpeed _animationSpeed = AnimationSpeed.Normal;
        private bool _showTips = true;
        private bool _showGrid = false;

        public bool FullScreen
        {
            get => _fullScreen;
            set => SetProperty(ref _fullScreen, value);
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, Math.Clamp(value, 800, 3840));
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, Math.Clamp(value, 600, 2160));
        }

        public AnimationSpeed AnimationSpeed
        {
            get => _animationSpeed;
            set => SetProperty(ref _animationSpeed, value);
        }

        public bool ShowTips
        {
            get => _showTips;
            set => SetProperty(ref _showTips, value);
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set => SetProperty(ref _showGrid, value);
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
    /// プレイヤー設定
    /// </summary>
    public class PlayerSettings : INotifyPropertyChanged
    {
        private string _defaultPlayerName = "プレイヤー";
        private PlayerColor _defaultPlayerColor = PlayerColor.Blue;
        private string _lastUsedPlayerName = "";

        public string DefaultPlayerName
        {
            get => _defaultPlayerName;
            set => SetProperty(ref _defaultPlayerName, value);
        }

        public PlayerColor DefaultPlayerColor
        {
            get => _defaultPlayerColor;
            set => SetProperty(ref _defaultPlayerColor, value);
        }

        public string LastUsedPlayerName
        {
            get => _lastUsedPlayerName;
            set => SetProperty(ref _lastUsedPlayerName, value);
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
}