using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Application.Commands;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Services;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// ���C���E�B���h�E�i�Q�[����ʁj��ViewModel
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly MessageService _messageService;
        private GameState? _gameState;
        private string _currentMessage = "�Q�[�������������Ă��܂�...";

        #region Properties

        /// <summary>
        /// ���݂̃Q�[�����b�Z�[�W
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        /// <summary>
        /// �w���X�g�i�}�b�v�\���p�j
        /// </summary>
        public ObservableCollection<StationViewModel> Stations { get; }

        /// <summary>
        /// �v���C���[���X�g
        /// </summary>
        public ObservableCollection<PlayerViewModel> Players { get; }

        /// <summary>
        /// �v���C���[��񃊃X�g�i�T�C�h�p�l���p�j
        /// </summary>
        public ObservableCollection<PlayerInfoViewModel> PlayerInfos { get; }

        /// <summary>
        /// ���݂̃v���C���[�̃J�[�h
        /// </summary>
        public ObservableCollection<CardViewModel> CurrentPlayerCards { get; }

        /// <summary>
        /// ���݂̔N
        /// </summary>
        private int _currentYear = 1;
        public int CurrentYear
        {
            get => _currentYear;
            set => SetProperty(ref _currentYear, value);
        }

        /// <summary>
        /// ���݂̌�
        /// </summary>
        private int _currentMonth = 4;
        public int CurrentMonth
        {
            get => _currentMonth;
            set => SetProperty(ref _currentMonth, value);
        }

        /// <summary>
        /// �ړI�n
        /// </summary>
        private string _destination = "���ݒ�";
        public string Destination
        {
            get => _destination;
            set => SetProperty(ref _destination, value);
        }

        /// <summary>
        /// �T�C�R����U��邩
        /// </summary>
        private bool _canRollDice;
        public bool CanRollDice
        {
            get => _canRollDice;
            set => SetProperty(ref _canRollDice, value);
        }

        /// <summary>
        /// �^�[�����I���ł��邩
        /// </summary>
        private bool _canEndTurn;
        public bool CanEndTurn
        {
            get => _canEndTurn;
            set => SetProperty(ref _canEndTurn, value);
        }

        #endregion

        #region Commands

        public ICommand RollDiceCommand { get; }
        public ICommand EndTurnCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand LoadGameCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        #endregion

        public MainWindowViewModel()
        {
            _messageService = new MessageService();

            // �R���N�V�����̏�����
            Stations = new ObservableCollection<StationViewModel>();
            Players = new ObservableCollection<PlayerViewModel>();
            PlayerInfos = new ObservableCollection<PlayerInfoViewModel>();
            CurrentPlayerCards = new ObservableCollection<CardViewModel>();

            // �R�}���h�̏�����
            RollDiceCommand = new RelayCommand(RollDice, () => CanRollDice);
            EndTurnCommand = new RelayCommand(EndTurn, () => CanEndTurn);
            SaveGameCommand = new RelayCommand(SaveGame);
            LoadGameCommand = new RelayCommand(LoadGame);
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            // ���b�Z�[�W�T�[�r�X�̍w��
            _messageService.PropertyChanged += OnMessageServicePropertyChanged;
        }

        /// <summary>
        /// �V�����Q�[�����J�n
        /// </summary>
        public void StartNewGame(GameSettings settings)
        {
            CurrentMessage = "�V�����Q�[�����J�n���Ă��܂�...";

            // �Q�[����Ԃ�������
            _gameState = new GameState
            {
                GameMode = settings.GameMode,
                MaxYears = settings.MaxYears,
                CurrentYear = 1,
                CurrentMonth = 4,
                CurrentPhase = GamePhase.Setup
            };

            // �v���C���[���쐬
            CreatePlayers(settings);

            // �}�b�v���������i�������j
            InitializeMap();

            // UI�X�V
            UpdateUI();

            CurrentMessage = "�Q�[�����J�n����܂����I";
            _messageService.ShowInfo($"{settings.PlayerName}����A�����Y�d�S�̐��E�ւ悤�����I");

            // �ŏ��̃^�[�����J�n
            StartFirstTurn();
        }

        /// <summary>
        /// �v���C���[���쐬
        /// </summary>
        private void CreatePlayers(GameSettings settings)
        {
            // �l�ԃv���C���[
            var humanPlayer = new Player
            {
                Name = settings.PlayerName,
                Color = settings.PlayerColor,
                IsHuman = true,
                CurrentMoney = new Money(100000000), // 1���~
                Rank = 1
            };
            _gameState!.Players.Add(humanPlayer);

            // COM�v���C���[
            var comColors = new[] { PlayerColor.Red, PlayerColor.Green, PlayerColor.Purple, PlayerColor.Orange, PlayerColor.Yellow };
            var comNames = new[] { "COM1", "COM2", "COM3" };

            for (int i = 0; i < settings.ComPlayerCount; i++)
            {
                var comPlayer = new Player
                {
                    Name = comNames[i],
                    Color = comColors[i],
                    IsHuman = false,
                    CurrentMoney = new Money(100000000),
                    Rank = i + 2
                };
                _gameState.Players.Add(comPlayer);
            }

            // ViewModel�R���N�V�������X�V
            Players.Clear();
            PlayerInfos.Clear();

            foreach (var player in _gameState.Players)
            {
                Players.Add(new PlayerViewModel(player));
                PlayerInfos.Add(new PlayerInfoViewModel(player));
            }
        }

        /// <summary>
        /// �}�b�v���������i�������j
        /// </summary>
        private void InitializeMap()
        {
            // ���̉w�f�[�^���쐬
            var stations = new[]
            {
                Station.Create(1, "����", 800, 400, StationType.Property, Region.Kanto),
                Station.Create(2, "���l", 850, 450, StationType.Property, Region.Kanto),
                Station.Create(3, "���É�", 600, 450, StationType.Property, Region.Chubu),
                Station.Create(4, "���", 500, 500, StationType.Property, Region.Kinki),
                Station.Create(5, "�_��", 450, 500, StationType.Property, Region.Kinki),
            };

            // �w��ڑ�
            stations[0].ConnectTo(stations[1]); // ����-���l
            stations[0].ConnectTo(stations[2]); // ����-���É�
            stations[2].ConnectTo(stations[3]); // ���É�-���
            stations[3].ConnectTo(stations[4]); // ���-�_��

            // �l�b�g���[�N�ɒǉ�
            foreach (var station in stations)
            {
                _gameState!.StationNetwork.AddStation(station);
            }

            // ViewModel�R���N�V�������X�V
            Stations.Clear();
            foreach (var station in stations)
            {
                Stations.Add(new StationViewModel(station));
            }

            // �v���C���[�̏����ʒu��ݒ�
            foreach (var player in _gameState.Players)
            {
                player.CurrentStation = stations[0]; // ��������X�^�[�g
            }

            // �ړI�n��ݒ�
            _gameState.Destination = stations[4]; // �_��
            stations[4].IsDestination = true;
            Destination = stations[4].Name;
        }

        /// <summary>
        /// �ŏ��̃^�[�����J�n
        /// </summary>
        private void StartFirstTurn()
        {
            _gameState!.CurrentPhase = GamePhase.TurnStart;
            _gameState.CurrentPlayerIndex = 0;

            var currentPlayer = _gameState.GetCurrentPlayer();
            if (currentPlayer.IsHuman)
            {
                CanRollDice = true;
                CanEndTurn = false;
                _messageService.ShowInfo($"{currentPlayer.Name}����̃^�[���ł��B�T�C�R����U���Ă��������B");
            }
            else
            {
                // COM�����i��Ŏ����j
                _messageService.ShowInfo($"{currentPlayer.Name}�̃^�[���ł��B");
            }
        }

        /// <summary>
        /// UI�X�V
        /// </summary>
        private void UpdateUI()
        {
            if (_gameState == null) return;

            CurrentYear = _gameState.CurrentYear;
            CurrentMonth = _gameState.CurrentMonth;

            // �v���C���[�ʒu�X�V
            foreach (var player in Players)
            {
                player.UpdatePosition();
            }

            // �v���C���[���X�V
            foreach (var info in PlayerInfos)
            {
                info.UpdateInfo();
            }
        }

        /// <summary>
        /// �T�C�R����U��
        /// </summary>
        private void RollDice()
        {
            _messageService.ShowInfo("�T�C�R����U���Ă��܂�...");
            // TODO: �T�C�R�������̎���
            CanRollDice = false;
            CanEndTurn = true;
        }

        /// <summary>
        /// �^�[���I��
        /// </summary>
        private void EndTurn()
        {
            _messageService.ShowInfo("�^�[�����I�����Ă��܂�...");
            // TODO: �^�[���I�������̎���
            CanEndTurn = false;
        }

        /// <summary>
        /// �Q�[�����Z�[�u
        /// </summary>
        private void SaveGame()
        {
            _messageService.ShowInfo("�Z�[�u�@�\�͏������ł��B");
        }

        /// <summary>
        /// �Q�[�������[�h
        /// </summary>
        private void LoadGame()
        {
            _messageService.ShowInfo("���[�h�@�\�͏������ł��B");
        }

        /// <summary>
        /// �ݒ��\��
        /// </summary>
        private void ShowSettings()
        {
            _messageService.ShowInfo("�ݒ�@�\�͏������ł��B");
        }

        /// <summary>
        /// ���b�Z�[�W�T�[�r�X�̃v���p�e�B�ύX��
        /// </summary>
        private void OnMessageServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MessageService.CurrentMessage))
            {
                CurrentMessage = _messageService.CurrentMessage;
            }
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

    #region ViewModels for Collections

    /// <summary>
    /// �wViewModel
    /// </summary>
    public class StationViewModel
    {
        private readonly Station _station;

        public StationViewModel(Station station)
        {
            _station = station;
        }

        public double X => _station.X;
        public double Y => _station.Y;
        public string Name => _station.Name;
        public string ShortName => _station.ShortName;
        public string TypeColor => _station.TypeColor;
    }

    /// <summary>
    /// �v���C���[ViewModel
    /// </summary>
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private readonly Player _player;
        private double _positionX;
        private double _positionY;

        public PlayerViewModel(Player player)
        {
            _player = player;
            UpdatePosition();
        }

        public Guid Id => _player.Id;
        public string Name => _player.Name;
        public string IconPath => $"/Resources/Images/Players/{_player.Color}.png";

        public double PositionX
        {
            get => _positionX;
            set => SetProperty(ref _positionX, value);
        }

        public double PositionY
        {
            get => _positionY;
            set => SetProperty(ref _positionY, value);
        }

        public void UpdatePosition()
        {
            if (_player.CurrentStation != null)
            {
                PositionX = _player.CurrentStation.X;
                PositionY = _player.CurrentStation.Y;
            }
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
    /// �v���C���[���ViewModel
    /// </summary>
    public class PlayerInfoViewModel : INotifyPropertyChanged
    {
        private readonly Player _player;

        public PlayerInfoViewModel(Player player)
        {
            _player = player;
        }

        public string Name => _player.Name;
        public string Money => _player.CurrentMoney.ToString();
        public string TotalAssets => _player.TotalAssets.ToString();
        public int Rank => _player.Rank;

        public string BorderColor => _player.Color switch
        {
            PlayerColor.Blue => "#FF4169E1",
            PlayerColor.Red => "#FFDC143C",
            PlayerColor.Yellow => "#FFFFD700",
            PlayerColor.Green => "#FF32CD32",
            PlayerColor.Purple => "#FF9370DB",
            PlayerColor.Orange => "#FFFF8C00",
            _ => "#FF808080"
        };

        public void UpdateInfo()
        {
            OnPropertyChanged(nameof(Money));
            OnPropertyChanged(nameof(TotalAssets));
            OnPropertyChanged(nameof(Rank));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// �J�[�hViewModel
    /// </summary>
    public class CardViewModel
    {
        private readonly Card _card;

        public CardViewModel(Card card)
        {
            _card = card;
        }

        public int Id => _card.Id;
        public string Name => _card.Name;
        public string UsageText => _card.GetUsageText();
        public string RarityColor => _card.GetRarityColor();
        public string IconPath => $"/Resources/Images/Cards/{_card.Type}/{_card.Id}.png";
    }

    #endregion
}