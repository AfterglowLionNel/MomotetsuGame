using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Application.GameLogic;
using MomotetsuGame.Application.DependencyInjection;
using MomotetsuGame.Infrastructure.Data;

namespace MomotetsuGame.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // サービス
        private readonly IGameManager _gameManager;
        private readonly IEventBus _eventBus;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly ISaveDataService _saveDataService;
        private readonly IMasterDataLoader _masterDataLoader;

        // プライベートフィールド
        private GameState? _gameState;
        private string _currentMessage = "ゲームを開始してください";
        private bool _canRollDice = false;
        private bool _canEndTurn = false;
        private bool _isLoading = false;

        // プロパティ
        public ObservableCollection<StationViewModel> Stations { get; set; }
        public ObservableCollection<PlayerViewModel> Players { get; set; }
        public ObservableCollection<PlayerInfoViewModel> PlayerInfos { get; set; }

        public int CurrentYear => _gameState?.CurrentYear ?? 1;
        public int CurrentMonth => _gameState?.CurrentMonth ?? 4;
        public string Destination => _gameState?.Destination?.Name ?? "未設定";

        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        public bool CanRollDice
        {
            get => _canRollDice;
            set => SetProperty(ref _canRollDice, value);
        }

        public bool CanEndTurn
        {
            get => _canEndTurn;
            set => SetProperty(ref _canEndTurn, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // コマンド
        public ICommand RollDiceCommand { get; private set; }
        public ICommand EndTurnCommand { get; private set; }
        public ICommand SaveGameCommand { get; private set; }
        public ICommand LoadGameCommand { get; private set; }
        public ICommand ShowSettingsCommand { get; private set; }

        // コンストラクタ
        public MainWindowViewModel()
        {
            // サービスをDIコンテナから取得
            _gameManager = ServiceContainer.GetService<IGameManager>();
            _eventBus = ServiceContainer.GetService<IEventBus>();
            _dialogService = ServiceContainer.GetService<IDialogService>();
            _messageService = ServiceContainer.GetService<IMessageService>();
            _saveDataService = ServiceContainer.GetService<ISaveDataService>();
            _masterDataLoader = new MasterDataLoader();

            InitializeCollections();
            InitializeCommands();
            SubscribeToEvents();
        }

        private void InitializeCollections()
        {
            Stations = new ObservableCollection<StationViewModel>();
            Players = new ObservableCollection<PlayerViewModel>();
            PlayerInfos = new ObservableCollection<PlayerInfoViewModel>();
        }

        private void InitializeCommands()
        {
            RollDiceCommand = new RelayCommand(async () => await RollDiceAsync(), () => CanRollDice && !IsLoading);
            EndTurnCommand = new RelayCommand(async () => await EndTurnAsync(), () => CanEndTurn && !IsLoading);
            SaveGameCommand = new RelayCommand(async () => await SaveGameAsync(), () => _gameState != null && !IsLoading);
            LoadGameCommand = new RelayCommand(async () => await LoadGameAsync(), () => !IsLoading);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
        }

        private void SubscribeToEvents()
        {
            // ゲームイベントの購読
            _eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
            _eventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            _eventBus.Subscribe<PropertyPurchasedEvent>(OnPropertyPurchased);
            _eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            _eventBus.Subscribe<MessageEvent>(OnMessage);
        }

        // イベントハンドラ
        private void OnTurnStarted(TurnStartedEvent e)
        {
            CurrentMessage = $"{e.Player.Name}のターンです";
            CanRollDice = !e.Player.IsComputer;
            CanEndTurn = false;
            UpdateUI();
        }

        private void OnDiceRolled(DiceRolledEvent e)
        {
            CurrentMessage = $"{e.Player.Name}がサイコロを振りました: {e.Result}";
            CanRollDice = false;
        }

        private void OnPlayerMoved(PlayerMovedEvent e)
        {
            CurrentMessage = $"{e.Player.Name}が{e.To.Name}に到着しました";
            UpdatePlayerPosition(e.Player);
        }

        private void OnPropertyPurchased(PropertyPurchasedEvent e)
        {
            CurrentMessage = $"{e.Player.Name}が{e.Property.Name}を購入しました！";
            _messageService.ShowSuccess(CurrentMessage);
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            CanEndTurn = false;
            UpdateUI();
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            _gameState = e.GameState;
            UpdateUI();
        }

        private void OnMessage(MessageEvent e)
        {
            CurrentMessage = e.Message;
            if (e.Type == MessageType.Error)
            {
                _messageService.ShowError(e.Message);
            }
        }

        // コマンドハンドラ
        private async Task RollDiceAsync()
        {
            if (_gameState == null) return;

            IsLoading = true;
            try
            {
                var currentPlayer = _gameState.GetCurrentPlayer();
                await _gameManager.RollDiceAsync(currentPlayer);

                // プレイヤーが移動した後の処理
                CanRollDice = false;
                CanEndTurn = true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"エラーが発生しました: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EndTurnAsync()
        {
            if (_gameState == null) return;

            IsLoading = true;
            try
            {
                await _gameManager.EndTurnAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"エラーが発生しました: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveGameAsync()
        {
            if (_gameState == null) return;

            IsLoading = true;
            try
            {
                var saveId = await _dialogService.ShowSelectionAsync(
                    "セーブスロットを選択してください",
                    new[] { "セーブ1", "セーブ2", "セーブ3" },
                    "セーブ"
                );

                if (saveId != null)
                {
                    await _saveDataService.SaveAsync(saveId, _gameState);
                    _messageService.ShowSuccess("セーブしました");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"セーブに失敗しました: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadGameAsync()
        {
            IsLoading = true;
            try
            {
                var saveDataList = await _saveDataService.GetSaveDataListAsync();
                if (!saveDataList.Any())
                {
                    await _dialogService.ShowInformationAsync("セーブデータがありません");
                    return;
                }

                // セーブデータ選択（簡易版）
                var selectedSave = saveDataList.FirstOrDefault();
                if (selectedSave != null)
                {
                    var gameState = await _saveDataService.LoadAsync<GameState>(selectedSave.Id);
                    if (gameState != null)
                    {
                        _gameState = gameState;
                        _gameManager.LoadGameState(gameState);
                        UpdateUI();
                        _messageService.ShowSuccess("ロードしました");
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"ロードに失敗しました: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowSettings()
        {
            CurrentMessage = "設定画面を開きます。";
            // TODO: 設定画面の実装
        }

        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        public async Task StartNewGameAsync(GameSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            IsLoading = true;
            try
            {
                // マスターデータを読み込む
                var stationNetwork = await _masterDataLoader.LoadStationNetworkAsync();
                var properties = await _masterDataLoader.LoadPropertiesAsync();
                var cardMaster = await _masterDataLoader.LoadCardMasterAsync();

                // 駅と物件を関連付け
                _masterDataLoader.AssociatePropertiesWithStations(stationNetwork, properties);

                // ゲームを初期化
                await _gameManager.InitializeGameAsync(settings);
                _gameState = _gameManager.GetCurrentGameState();

                // 駅ネットワークを設定
                if (_gameState != null)
                {
                    _gameState.StationNetwork = stationNetwork;
                }

                UpdateUI();
                CurrentMessage = "ゲームを開始しました。";
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"ゲームの開始に失敗しました: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // UI更新メソッド
        private void UpdateUI()
        {
            if (_gameState == null) return;

            UpdateStations();
            UpdatePlayers();
            UpdatePlayerInfos();

            OnPropertyChanged(nameof(CurrentYear));
            OnPropertyChanged(nameof(CurrentMonth));
            OnPropertyChanged(nameof(Destination));
        }

        private void UpdateStations()
        {
            Stations.Clear();

            if (_gameState?.StationNetwork == null) return;

            foreach (var station in _gameState.StationNetwork.GetAllStations())
            {
                var vm = new StationViewModel
                {
                    X = station.Coordinate.X,
                    Y = station.Coordinate.Y,
                    ShortName = station.Name.Length > 3 ? station.Name.Substring(0, 3) : station.Name,
                    TypeColor = GetStationColor(station.Type)
                };
                Stations.Add(vm);
            }
        }

        private void UpdatePlayers()
        {
            Players.Clear();

            if (_gameState?.Players == null) return;

            foreach (var player in _gameState.Players)
            {
                if (player.CurrentStation != null)
                {
                    var vm = new PlayerViewModel
                    {
                        PositionX = player.CurrentStation.Coordinate.X,
                        PositionY = player.CurrentStation.Coordinate.Y,
                        Name = player.Name
                    };
                    Players.Add(vm);
                }
            }
        }

        private void UpdatePlayerInfos()
        {
            PlayerInfos.Clear();

            if (_gameState?.Players == null) return;

            foreach (var player in _gameState.Players)
            {
                var vm = new PlayerInfoViewModel
                {
                    Name = player.Name,
                    Money = player.CurrentMoney.ToString(),
                    TotalAssets = player.TotalAssets.ToString(),
                    Rank = player.Rank,
                    BorderColor = GetPlayerColor(player.Color)
                };
                PlayerInfos.Add(vm);
            }
        }

        private void UpdatePlayerPosition(Player player)
        {
            var playerVm = Players.FirstOrDefault(p => p.Name == player.Name);
            if (playerVm != null && player.CurrentStation != null)
            {
                playerVm.PositionX = player.CurrentStation.Coordinate.X;
                playerVm.PositionY = player.CurrentStation.Coordinate.Y;
            }
        }

        private string GetStationColor(StationType type)
        {
            return type switch
            {
                StationType.Property => "#FF0000",
                StationType.Plus => "#00FF00",
                StationType.Minus => "#0000FF",
                StationType.CardShop => "#FFFF00",
                StationType.NiceCard => "#FF00FF",
                StationType.SuperCard => "#00FFFF",
                _ => "#808080"
            };
        }

        private string GetPlayerColor(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Blue => "#0000FF",
                PlayerColor.Red => "#FF0000",
                PlayerColor.Green => "#00FF00",
                PlayerColor.Yellow => "#FFFF00",
                PlayerColor.Purple => "#800080",
                PlayerColor.Orange => "#FFA500",
                PlayerColor.Pink => "#FFC0CB",
                PlayerColor.Brown => "#A52A2A",
                _ => "#808080"
            };
        }

        // INotifyPropertyChanged実装
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            CommandManager.InvalidateRequerySuggested();
            return true;
        }
    }

    // ViewModelクラスは変更なし
    public class StationViewModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public string ShortName { get; set; }
        public string TypeColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class PlayerViewModel : INotifyPropertyChanged
    {
        private double _positionX;
        private double _positionY;

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

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class PlayerInfoViewModel
    {
        public string Name { get; set; }
        public string Money { get; set; }
        public string TotalAssets { get; set; }
        public int Rank { get; set; }
        public string BorderColor { get; set; }
    }

    // イベントクラス
    public class MessageEvent
    {
        public string Message { get; set; }
        public MessageType Type { get; set; }
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success
    }

    // RelayCommand実装は変更なし
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}