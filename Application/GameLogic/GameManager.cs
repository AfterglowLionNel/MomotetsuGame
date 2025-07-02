using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Application.Services;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// ゲーム管理クラス
    /// </summary>
    public class GameManager : IGameManager
    {
        private readonly IDiceService _diceService;
        private readonly IRouteCalculator _routeCalculator;
        private readonly IPropertyService _propertyService;
        private readonly IComputerAI? _computerAI;
        private GameState? _currentGameState;

        // イベント
        public event EventHandler<GameEventArgs>? GameEvent;
        public event EventHandler<TurnChangedEventArgs>? TurnChanged;
        public event EventHandler<PlayerMovedEventArgs>? PlayerMoved;
        public event EventHandler<PropertyTransactionEventArgs>? PropertyPurchased;
        public event EventHandler<PropertyTransactionEventArgs>? PropertySold;
        public event EventHandler<CardUsedEventArgs>? CardUsed;
        public event EventHandler<GameMessageEventArgs>? GameMessage;

        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        public GameState CurrentGameState => _currentGameState ?? throw new InvalidOperationException("ゲームが開始されていません。");

        /// <summary>
        /// 現在のプレイヤー
        /// </summary>
        public Player CurrentPlayer => CurrentGameState.GetCurrentPlayer();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameManager(
            IDiceService diceService,
            IRouteCalculator routeCalculator,
            IPropertyService propertyService,
            IComputerAI? computerAI = null)
        {
            _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
            _routeCalculator = routeCalculator ?? throw new ArgumentNullException(nameof(routeCalculator));
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _computerAI = computerAI;

            // PropertyServiceのイベントを購読
            _propertyService.PropertyPurchased += OnPropertyServicePurchased;
            _propertyService.PropertySold += OnPropertyServiceSold;
        }

        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        public async Task<GameState> StartNewGameAsync(GameSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // ゲーム状態を初期化
            _currentGameState = new GameState
            {
                GameMode = settings.GameMode,
                MaxYears = settings.MaxYears,
                Settings = settings
            };

            // プレイヤーを作成
            CreatePlayers(settings);

            // ステーションネットワークを初期化（仮実装）
            InitializeStationNetwork();

            // 初期目的地を設定
            SetInitialDestination();

            // ゲーム開始イベント
            RaiseGameEvent("ゲームを開始しました！");

            // 最初のターンを開始
            await StartTurnAsync();

            return _currentGameState;
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        public async Task<GameState> LoadGameAsync(string saveId)
        {
            // TODO: 実装（Repositoryを使用）
            throw new NotImplementedException("セーブ/ロード機能は未実装です。");
        }

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        public async Task SaveGameAsync(string saveId)
        {
            // TODO: 実装（Repositoryを使用）
            throw new NotImplementedException("セーブ/ロード機能は未実装です。");
        }

        /// <summary>
        /// サイコロを振る
        /// </summary>
        public async Task<DiceResult> RollDiceAsync()
        {
            var player = CurrentPlayer;

            // 行動済みチェック
            if (CurrentGameState.CurrentPhase != GamePhase.Action)
            {
                throw new InvalidOperationException("現在はサイコロを振ることができません。");
            }

            // サイコロを振る
            var result = await _diceService.RollForPlayerAsync(player);

            RaiseGameEvent($"{player.Name}はサイコロを振って{result.Total}が出ました！");

            // フェーズを移動に変更
            CurrentGameState.CurrentPhase = GamePhase.Movement;

            return result;
        }

        /// <summary>
        /// プレイヤーを移動
        /// </summary>
        public async Task<MoveResult> MovePlayerAsync(Player player, int steps)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (player.CurrentStation == null)
                throw new InvalidOperationException("プレイヤーの現在位置が設定されていません。");

            var startStation = player.CurrentStation;
            var route = _routeCalculator.CalculateRoute(startStation, steps);

            // 分岐点チェック
            if (route.Contains(null!))
            {
                // 分岐点がある場合はUIに選択を委ねる
                var branchIndex = route.IndexOf(null!);
                var partialRoute = route.Take(branchIndex).ToList();

                return new MoveResult
                {
                    Player = player,
                    Route = partialRoute,
                    FinalStation = partialRoute.Last(),
                    RequiresBranchSelection = true,
                    RemainingSteps = steps - partialRoute.Count + 1
                };
            }

            // 移動実行
            var finalStation = route.Last();
            player.CurrentStation = finalStation;

            var moveResult = new MoveResult
            {
                Player = player,
                Route = route,
                FinalStation = finalStation,
                ReachedDestination = finalStation.IsDestination
            };

            // 移動イベント発火
            PlayerMoved?.Invoke(this, new PlayerMovedEventArgs
            {
                Player = player,
                FromStation = startStation,
                ToStation = finalStation,
                Route = route,
                Message = $"{player.Name}は{finalStation.Name}に到着しました。"
            });

            // 到着処理
            await ProcessArrivalAsync(player, finalStation);

            return moveResult;
        }

        /// <summary>
        /// カードを使用
        /// </summary>
        public async Task<CardEffectResult> UseCardAsync(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            var player = CurrentPlayer;

            if (!player.Cards.Contains(card))
            {
                return CardEffectResult.CreateFailure("このカードを所持していません。");
            }

            if (!card.CanUse(player, CurrentGameState))
            {
                return CardEffectResult.CreateFailure("このカードは現在使用できません。");
            }

            // カード効果の実行（仮実装）
            var result = new CardEffectResult
            {
                Success = true,
                Message = $"{card.Name}を使用しました！"
            };

            // カード使用処理
            player.UseCard(card);

            // イベント発火
            CardUsed?.Invoke(this, new CardUsedEventArgs
            {
                Player = player,
                Card = card,
                Result = result
            });

            return result;
        }

        /// <summary>
        /// 物件を購入
        /// </summary>
        public async Task<bool> PurchasePropertyAsync(Player player, Property property)
        {
            var result = await _propertyService.PurchasePropertyAsync(player, property);

            if (result.Success)
            {
                RaiseGameEvent(result.Message);
            }

            return result.Success;
        }

        /// <summary>
        /// 物件を売却
        /// </summary>
        public async Task<bool> SellPropertyAsync(Player player, Property property)
        {
            var result = await _propertyService.SellPropertyAsync(player, property);

            if (result.Success)
            {
                RaiseGameEvent(result.Message);
            }

            return result.Success;
        }

        /// <summary>
        /// ターンを終了
        /// </summary>
        public async Task EndTurnAsync()
        {
            if (CurrentGameState.CurrentPhase != GamePhase.TurnEnd &&
                CurrentGameState.CurrentPhase != GamePhase.Arrival)
            {
                throw new InvalidOperationException("まだターンを終了できません。");
            }

            // プレイヤーのターン終了処理
            CurrentPlayer.ProcessTurnEnd();

            // 次のプレイヤーへ
            CurrentGameState.MoveToNextPlayer();

            // 次のターンを開始
            await StartTurnAsync();
        }

        /// <summary>
        /// 目的地到着処理
        /// </summary>
        public async Task ProcessDestinationArrivalAsync(Player player)
        {
            CurrentGameState.ProcessDestinationArrival(player);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 移動可能な駅を取得
        /// </summary>
        public List<Station> GetReachableStations(Station from, int steps)
        {
            return _routeCalculator.GetReachableStations(from, steps);
        }

        /// <summary>
        /// ゲーム終了チェック
        /// </summary>
        public bool IsGameOver()
        {
            return CurrentGameState.IsGameOver();
        }

        /// <summary>
        /// 最終結果を取得
        /// </summary>
        public GameResult GetGameResult()
        {
            var result = new GameResult
            {
                TotalTurns = CurrentGameState.CurrentTurn,
                Years = CurrentGameState.CurrentYear
            };

            // プレイヤー結果を集計
            foreach (var player in CurrentGameState.Players.OrderBy(p => p.Rank))
            {
                result.PlayerResults.Add(new PlayerResult
                {
                    Player = player,
                    FinalRank = player.Rank,
                    FinalAssets = player.TotalAssets,
                    PropertyCount = player.OwnedProperties.Count
                });
            }

            result.Winner = result.PlayerResults.First().Player;

            return result;
        }

        #region Private Methods

        /// <summary>
        /// プレイヤーを作成
        /// </summary>
        private void CreatePlayers(GameSettings settings)
        {
            // 人間プレイヤー
            var humanPlayer = Player.Create(settings.PlayerName, true, settings.PlayerColor);
            CurrentGameState.Players.Add(humanPlayer);

            // COMプレイヤー
            var comColors = new[] { PlayerColor.Red, PlayerColor.Green, PlayerColor.Yellow, PlayerColor.Purple };
            for (int i = 0; i < settings.ComPlayerCount; i++)
            {
                var comPlayer = Player.Create($"COM{i + 1}", false, comColors[i]);
                CurrentGameState.Players.Add(comPlayer);
            }
        }

        /// <summary>
        /// ステーションネットワークを初期化（仮実装）
        /// </summary>
        private void InitializeStationNetwork()
        {
            // 仮の駅を作成
            var tokyo = Station.CreatePropertyStation(1, "東京", 400, 300, Region.Kanto);
            var shinjuku = Station.CreatePropertyStation(2, "新宿", 350, 280, Region.Kanto);
            var shinagawa = Station.CreatePropertyStation(3, "品川", 450, 320, Region.Kanto);
            var ikebukuro = Station.CreatePropertyStation(4, "池袋", 300, 250, Region.Kanto);
            var yokohama = Station.CreatePropertyStation(5, "横浜", 500, 350, Region.Kanto);

            // 駅を追加
            CurrentGameState.StationNetwork.AddStation(tokyo);
            CurrentGameState.StationNetwork.AddStation(shinjuku);
            CurrentGameState.StationNetwork.AddStation(shinagawa);
            CurrentGameState.StationNetwork.AddStation(ikebukuro);
            CurrentGameState.StationNetwork.AddStation(yokohama);

            // 接続を追加
            CurrentGameState.StationNetwork.AddConnection(1, 2); // 東京-新宿
            CurrentGameState.StationNetwork.AddConnection(1, 3); // 東京-品川
            CurrentGameState.StationNetwork.AddConnection(2, 4); // 新宿-池袋
            CurrentGameState.StationNetwork.AddConnection(3, 5); // 品川-横浜

            // 仮の物件を追加
            tokyo.Properties.Add(Property.Create("東京タワー", PropertyCategory.Tourism, 10000000000, 0.15m));
            tokyo.Properties.Add(Property.Create("皇居", PropertyCategory.Tourism, 20000000000, 0.12m));

            shinjuku.Properties.Add(Property.Create("新宿駅ビル", PropertyCategory.Commerce, 8000000000, 0.10m));

            // 各プレイヤーの初期位置を設定
            foreach (var player in CurrentGameState.Players)
            {
                player.CurrentStation = tokyo;
            }
        }

        /// <summary>
        /// 初期目的地を設定
        /// </summary>
        private void SetInitialDestination()
        {
            var yokohama = CurrentGameState.StationNetwork.GetStation(5);
            if (yokohama != null)
            {
                CurrentGameState.Destination = yokohama;
                yokohama.IsDestination = true;
                RaiseGameEvent($"最初の目的地は{yokohama.Name}です！");
            }
        }

        /// <summary>
        /// ターンを開始
        /// </summary>
        private async Task StartTurnAsync()
        {
            CurrentGameState.CurrentPhase = GamePhase.TurnStart;

            // ターン変更イベント
            TurnChanged?.Invoke(this, new TurnChangedEventArgs
            {
                CurrentPlayer = CurrentPlayer,
                Turn = CurrentGameState.CurrentTurn,
                Year = CurrentGameState.CurrentYear,
                Month = CurrentGameState.CurrentMonth,
                Message = $"{CurrentPlayer.Name}のターンです。"
            });

            CurrentGameState.CurrentPhase = GamePhase.Action;

            // COMの場合は自動プレイ
            if (!CurrentPlayer.IsHuman && _computerAI != null)
            {
                await ProcessComputerTurnAsync();
            }
        }

        /// <summary>
        /// COMのターンを処理
        /// </summary>
        private async Task ProcessComputerTurnAsync()
        {
            await Task.Delay(1000); // 思考時間のシミュレート

            // サイコロを振る
            var diceResult = await RollDiceAsync();

            await Task.Delay(500);

            // 移動
            var moveResult = await MovePlayerAsync(CurrentPlayer, diceResult.Total);

            // 分岐選択が必要な場合
            if (moveResult.RequiresBranchSelection)
            {
                // TODO: AI実装後に分岐選択ロジックを追加
            }

            await Task.Delay(1000);

            // ターン終了
            await EndTurnAsync();
        }

        /// <summary>
        /// 到着処理
        /// </summary>
        private async Task ProcessArrivalAsync(Player player, Station station)
        {
            CurrentGameState.CurrentPhase = GamePhase.Arrival;

            switch (station.Type)
            {
                case StationType.Property:
                    await ProcessPropertyStationAsync(player, station);
                    break;

                case StationType.Plus:
                    ProcessPlusStation(player);
                    break;

                case StationType.Minus:
                    ProcessMinusStation(player);
                    break;

                    // TODO: 他の駅タイプの処理
            }

            // 目的地チェック
            if (station.IsDestination)
            {
                await ProcessDestinationArrivalAsync(player);
            }

            CurrentGameState.CurrentPhase = GamePhase.TurnEnd;
        }

        /// <summary>
        /// 物件駅の処理
        /// </summary>
        private async Task ProcessPropertyStationAsync(Player player, Station station)
        {
            if (player.IsHuman)
            {
                // UIで物件購入ダイアログを表示（イベント経由）
                RaiseGameEvent($"{station.Name}に到着しました。物件を購入できます。");
            }
            else if (_computerAI != null)
            {
                // COMの物件購入判断
                var availableProperties = station.Properties.Where(p => p.Owner == null).ToList();
                if (availableProperties.Any())
                {
                    // TODO: AI実装後に購入判断ロジックを追加
                    var targetProperty = availableProperties.First();
                    if (player.CurrentMoney >= targetProperty.CurrentPrice)
                    {
                        await PurchasePropertyAsync(player, targetProperty);
                    }
                }
            }
        }

        /// <summary>
        /// プラス駅の処理
        /// </summary>
        private void ProcessPlusStation(Player player)
        {
            var amount = new Money(10000000); // 1000万円
            player.Receive(amount);
            RaiseGameEvent($"{player.Name}は{amount}を獲得しました！");
        }

        /// <summary>
        /// マイナス駅の処理
        /// </summary>
        private void ProcessMinusStation(Player player)
        {
            var amount = new Money(10000000); // 1000万円
            player.Pay(amount);
            RaiseGameEvent($"{player.Name}は{amount}を失いました！");
        }

        /// <summary>
        /// ゲームイベントを発生
        /// </summary>
        private void RaiseGameEvent(string message)
        {
            GameEvent?.Invoke(this, new GameEventArgs { Message = message });
            GameMessage?.Invoke(this, new GameMessageEventArgs { Message = message });
        }

        /// <summary>
        /// PropertyServiceのイベントハンドラ
        /// </summary>
        private void OnPropertyServicePurchased(object? sender, PropertyTransactionEventArgs e)
        {
            PropertyPurchased?.Invoke(this, e);
        }

        private void OnPropertyServiceSold(object? sender, PropertyTransactionEventArgs e)
        {
            PropertySold?.Invoke(this, e);
        }

        #endregion
    }

    /// <summary>
    /// カード使用イベント引数
    /// </summary>
    public class CardUsedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Card Card { get; set; } = null!;
        public CardEffectResult Result { get; set; } = null!;
    }

    /// <summary>
    /// ゲームメッセージイベント引数
    /// </summary>
    public class GameMessageEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 移動結果（拡張）
    /// </summary>
    public partial class MoveResult
    {
        public bool RequiresBranchSelection { get; set; }
        public int RemainingSteps { get; set; }
    }
}