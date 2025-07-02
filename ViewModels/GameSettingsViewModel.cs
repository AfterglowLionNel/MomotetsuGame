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
    /// �Q�[���ݒ��ʂ�ViewModel
    /// </summary>
    public class GameSettingsViewModel : INotifyPropertyChanged
    {
        private readonly GameConfiguration _gameConfig;

        #region Properties

        // �v���C�N��
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

        // �v���C���[�ݒ�
        private string _playerName = "�v���C���[";
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

        // COM�ݒ�
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

        // COM����
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

        // ���ꃋ�[��
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

            // �v���C���[�F�̏�����
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

            // �R�}���h�̏�����
            StartGameCommand = new RelayCommand(StartGame, CanStartGame);
            CancelCommand = new RelayCommand(Cancel);

            // �f�t�H���g�l��ݒ肩��ǂݍ���
            LoadDefaults();
        }

        /// <summary>
        /// �p�����[�^����M
        /// </summary>
        public void ReceiveParameter(object parameter)
        {
            // �K�v�ɉ����ăp�����[�^������
        }

        /// <summary>
        /// �f�t�H���g�l��ǂݍ���
        /// </summary>
        private void LoadDefaults()
        {
            var settings = _gameConfig.Config.GameSettings;

            // �v���C�N��
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

            // �v���C���[�ݒ�
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

            // COM�ݒ�
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

            // ���ꃋ�[��
            EnableBonby = settings.EnableBonby;
            EnableSpecialEvents = settings.EnableSpecialEvents;
        }

        /// <summary>
        /// �Q�[���J�n�\���`�F�b�N
        /// </summary>
        private bool CanStartGame()
        {
            return !string.IsNullOrWhiteSpace(PlayerName) && SelectedPlayerColor != null;
        }

        /// <summary>
        /// �Q�[���J�n
        /// </summary>
        private void StartGame()
        {
            // �Q�[���ݒ���쐬
            var settings = new GameSettings
            {
                GameMode = GameMode.Normal,
                MaxYears = GetSelectedYears(),
                ComPlayerCount = GetSelectedComCount(),
                ComDifficulty = GetSelectedComDifficulty(),
                EnableBonby = EnableBonby,
                EnableSpecialEvents = EnableSpecialEvents
            };

            // �v���C���[����ݒ�ɒǉ�
            settings.PlayerName = PlayerName;
            settings.PlayerColor = SelectedPlayerColor!.Type;

            // �ݒ��ۑ�
            SaveSettings();

            // �Q�[���J�n�C�x���g�𔭉�
            GameStartRequested?.Invoke(settings);
        }

        /// <summary>
        /// �L�����Z��
        /// </summary>
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }

        /// <summary>
        /// �I�����ꂽ�N�����擾
        /// </summary>
        private int GetSelectedYears()
        {
            if (IsYear3Selected) return 3;
            if (IsYear5Selected) return 5;
            if (IsYear10Selected) return 10;
            return CustomYears;
        }

        /// <summary>
        /// �I�����ꂽCOM�����擾
        /// </summary>
        private int GetSelectedComCount()
        {
            if (IsCom1Selected) return 1;
            if (IsCom2Selected) return 2;
            return 3;
        }

        /// <summary>
        /// �I�����ꂽCOM��Փx���擾
        /// </summary>
        private ComDifficulty GetSelectedComDifficulty()
        {
            if (IsComEasySelected) return ComDifficulty.Easy;
            if (IsComHardSelected) return ComDifficulty.Hard;
            return ComDifficulty.Normal;
        }

        /// <summary>
        /// �ݒ��ۑ�
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
    /// �v���C���[�F�I�v�V����
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