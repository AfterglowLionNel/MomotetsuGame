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
    /// メインウィンドウ（ゲーム画面）のViewModel
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly MessageService _messageService;
        private GameState? _gameState;
        private string _currentMessage = "ゲームを初期化しています...";

        #region Properties

        /// <summary>
        /// 現在のゲームメッセージ
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        /// <summary>
        /// 駅リスト（マップ表示用）
        /// </summary>
        public ObservableCollection<StationViewModel> Stations { get; }

        /// <summary>
        /// プレイヤーリスト
        /// </summary>
        public ObservableCollection<PlayerViewModel> Players { get; }

        /// <summary>
        /// プレイヤー情報リスト（サイドパネル用）
        /// </summary>
        public ObservableCollection<PlayerInfoViewModel> PlayerInfos { get; }

        /// <summary>
        /// 現在のプレイヤーのカード
        /// </summary>
        public ObservableCollection<CardViewModel> CurrentPlayerCards { get; }

        /// <summary>
        /// 現在の年
        /// </summary>
        private int _currentYear = 1;
        public int CurrentYear
        {
            get => _currentYear;
            set => SetProperty(ref _currentYear, value);
        }

        /// <summary>
        /// 現在の月
        /// </summary>
        private int _currentMonth = 4;
        public int CurrentMonth
        {
            get => _currentMonth;
            set => SetProperty(ref _currentMonth, value);
        }

        /// <summary>
        /// 目的地
        /// </summary>
        private string _destination = "未設定";
        public string Destination
        {
            get => _destination;
            set => SetProperty(ref _destination, value);
        }

        /// <summary>
        /// サイコロを振れるか
        /// </summary>
        private bool _canRollDice;
        public bool CanRollDice
        {
            get => _canRollDice;
            set => SetProperty(ref _canRollDice, value);
        }

        /// <summary>
        /// ターンを終了できるか
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

            // コレクションの初期化
            Stations = new ObservableCollection<StationViewModel>();
            Players = new ObservableCollection<PlayerViewModel>();
            PlayerInfos = new ObservableCollection<PlayerInfoViewModel>();
            CurrentPlayerCards = new ObservableCollection<CardViewModel>();

            // コマンドの初期化
            RollDiceCommand = new RelayCommand(RollDice, () => CanRollDice);
            EndTurnCommand = new RelayCommand(EndTurn, () => CanEndTurn);
            SaveGameCommand = new RelayCommand(SaveGame);
            LoadGameCommand = new RelayCommand(LoadGame);
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            // メッセージサービスの購読
            _messageService.PropertyChanged += OnMessageServicePropertyChanged;
        }

        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        public void StartNewGame(GameSettings settings)
        {
            CurrentMessage = "新しいゲームを開始しています...";

            // ゲーム状態を初期化
            _gameState = new GameState
            {
                GameMode = settings.GameMode,
                MaxYears = settings.MaxYears,
                CurrentYear = 1,
                CurrentMonth = 4,
                CurrentPhase = GamePhase.Setup
            };

            // プレイヤーを作成
            CreatePlayers(settings);

            // マップを初期化（仮実装）
            InitializeMap();

            // UI更新
            UpdateUI();

            CurrentMessage = "ゲームが開始されました！";
            _messageService.ShowInfo($"{settings.PlayerName}さん、桃太郎電鉄の世界へようこそ！");

            // 最初のターンを開始
            StartFirstTurn();
        }

        /// <summary>
        /// プレイヤーを作成
        /// </summary>
        private void CreatePlayers(GameSettings settings)
        {
            // 人間プレイヤー
            var humanPlayer = new Player
            {
                Name = settings.PlayerName,
                Color = settings.PlayerColor,
                IsHuman = true,
                CurrentMoney = new Money(100000000), // 1億円
                Rank = 1
            };
            _gameState!.Players.Add(humanPlayer);

            // COMプレイヤー
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

            // ViewModelコレクションを更新
            Players.Clear();
            PlayerInfos.Clear();

            foreach (var player in _gameState.Players)
            {
                Players.Add(new PlayerViewModel(player));
                PlayerInfos.Add(new PlayerInfoViewModel(player));
            }
        }

        /// <summary>
        /// マップを初期化（仮実装）
        /// </summary>
        private void InitializeMap()
        {
            // 仮の駅データを作成
            var stations = new[]
            {
                Station.Create(1, "東京", 800, 400, StationType.Property, Region.Kanto),
                Station.Create(2, "横浜", 850, 450, StationType.Property, Region.Kanto),
                Station.Create(3, "名古屋", 600, 450, StationType.Property, Region.Chubu),
                Station.Create(4, "大阪", 500, 500, StationType.Property, Region.Kinki),
                Station.Create(5, "神戸", 450, 500, StationType.Property, Region.Kinki),
            };

            // 駅を接続
            stations[0].ConnectTo(stations[1]); // 東京-横浜
            stations[0].ConnectTo(stations[2]); // 東京-名古屋
            stations[2].ConnectTo(stations[3]); // 名古屋-大阪
            stations[3].ConnectTo(stations[4]); // 大阪-神戸

            // ネットワークに追加
            foreach (var station in stations)
            {
                _gameState!.StationNetwork.AddStation(station);
            }

            // ViewModelコレクションを更新
            Stations.Clear();
            foreach (var station in stations)
            {
                Stations.Add(new StationViewModel(station));
            }

            // プレイヤーの初期位置を設定
            foreach (var player in _gameState.Players)
            {
                player.CurrentStation = stations[0]; // 東京からスタート
            }

            // 目的地を設定
            _gameState.Destination = stations[4]; // 神戸
            stations[4].IsDestination = true;
            Destination = stations[4].Name;
        }

        /// <summary>
        /// 最初のターンを開始
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
                _messageService.ShowInfo($"{currentPlayer.Name}さんのターンです。サイコロを振ってください。");
            }
            else
            {
                // COM処理（後で実装）
                _messageService.ShowInfo($"{currentPlayer.Name}のターンです。");
            }
        }

        /// <summary>
        /// UI更新
        /// </summary>
        private void UpdateUI()
        {
            if (_gameState == null) return;

            CurrentYear = _gameState.CurrentYear;
            CurrentMonth = _gameState.CurrentMonth;

            // プレイヤー位置更新
            foreach (var player in Players)
            {
                player.UpdatePosition();
            }

            // プレイヤー情報更新
            foreach (var info in PlayerInfos)
            {
                info.UpdateInfo();
            }
        }

        /// <summary>
        /// サイコロを振る
        /// </summary>
        private void RollDice()
        {
            _messageService.ShowInfo("サイコロを振っています...");
            // TODO: サイコロ処理の実装
            CanRollDice = false;
            CanEndTurn = true;
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        private void EndTurn()
        {
            _messageService.ShowInfo("ターンを終了しています...");
            // TODO: ターン終了処理の実装
            CanEndTurn = false;
        }

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        private void SaveGame()
        {
            _messageService.ShowInfo("セーブ機能は準備中です。");
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        private void LoadGame()
        {
            _messageService.ShowInfo("ロード機能は準備中です。");
        }

        /// <summary>
        /// 設定を表示
        /// </summary>
        private void ShowSettings()
        {
            _messageService.ShowInfo("設定機能は準備中です。");
        }

        /// <summary>
        /// メッセージサービスのプロパティ変更時
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
    /// 駅ViewModel
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
    /// プレイヤーViewModel
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
    /// プレイヤー情報ViewModel
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
    /// カードViewModel
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