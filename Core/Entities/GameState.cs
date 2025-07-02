using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// ゲーム全体の状態を管理するエンティティ
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// ゲーム状態ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ゲーム作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// ゲームモード
        /// </summary>
        public GameMode GameMode { get; set; }

        /// <summary>
        /// 最大プレイ年数
        /// </summary>
        public int MaxYears { get; set; }

        /// <summary>
        /// 現在のターン数（通算）
        /// </summary>
        public int CurrentTurn { get; set; }

        /// <summary>
        /// 現在の年
        /// </summary>
        public int CurrentYear { get; set; }

        /// <summary>
        /// 現在の月（1〜12）
        /// </summary>
        public int CurrentMonth { get; set; }

        /// <summary>
        /// 現在のフェーズ
        /// </summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>
        /// プレイヤーリスト
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// 現在のプレイヤーインデックス
        /// </summary>
        public int CurrentPlayerIndex { get; set; }

        /// <summary>
        /// 現在の目的地駅
        /// </summary>
        public Station? Destination { get; set; }

        /// <summary>
        /// 駅ネットワーク
        /// </summary>
        public StationNetwork StationNetwork { get; set; }

        /// <summary>
        /// 物件マーケット情報
        /// </summary>
        public PropertyMarket PropertyMarket { get; set; }

        /// <summary>
        /// ゲーム設定
        /// </summary>
        public GameSettings Settings { get; set; }

        /// <summary>
        /// イベント履歴
        /// </summary>
        public List<GameEvent> EventHistory { get; set; }

        /// <summary>
        /// 現在アクティブなボンビー
        /// </summary>
        public Bonby? ActiveBonby { get; set; }

        /// <summary>
        /// ダイヤ改正レベル（カード価格倍率）
        /// </summary>
        public int DiamondRevisionLevel => CalculateDiamondRevisionLevel();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameState()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Players = new List<Player>();
            StationNetwork = new StationNetwork();
            PropertyMarket = new PropertyMarket();
            Settings = new GameSettings();
            EventHistory = new List<GameEvent>();
            CurrentYear = 1;
            CurrentMonth = 4; // 4月スタート
            CurrentPhase = GamePhase.Setup;
        }

        /// <summary>
        /// 現在のプレイヤーを取得
        /// </summary>
        public Player GetCurrentPlayer()
        {
            if (Players.Count == 0 || CurrentPlayerIndex >= Players.Count)
                throw new InvalidOperationException("有効なプレイヤーが存在しません。");

            return Players[CurrentPlayerIndex];
        }

        /// <summary>
        /// 次のプレイヤーに移動
        /// </summary>
        public void MoveToNextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            CurrentTurn++;

            // 全員が1回ずつプレイしたら月を進める
            if (CurrentPlayerIndex == 0)
            {
                AdvanceMonth();
            }
        }

        /// <summary>
        /// 月を進める
        /// </summary>
        private void AdvanceMonth()
        {
            CurrentMonth++;

            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;

                // 年度末処理
                ProcessYearEnd();
            }

            // 月末処理
            ProcessMonthEnd();

            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 月末処理
        /// </summary>
        private void ProcessMonthEnd()
        {
            // 借金の利子計算
            foreach (var player in Players)
            {
                if (player.Debt > Money.Zero)
                {
                    var interest = player.Debt * 0.1m / 12; // 年利10%の月割り
                    player.Debt += interest;

                    AddEvent(new GameEvent
                    {
                        Type = EventType.Interest,
                        Description = $"{player.Name}の借金利子: {interest}",
                        Amount = interest,
                        PlayerId = player.Id
                    });
                }
            }

            // 物件市場の更新
            PropertyMarket.UpdateMarket();
        }

        /// <summary>
        /// 年度末処理
        /// </summary>
        private void ProcessYearEnd()
        {
            // 決算処理
            foreach (var player in Players)
            {
                var yearlyIncome = CalculateYearlyIncome(player);
                player.CurrentMoney += yearlyIncome;
                player.YearlyIncome = yearlyIncome;

                AddEvent(new GameEvent
                {
                    Type = EventType.YearlyIncome,
                    Description = $"{player.Name}の年間収益: {yearlyIncome}",
                    Amount = yearlyIncome,
                    PlayerId = player.Id
                });
            }

            // 順位更新
            UpdatePlayerRankings();
        }

        /// <summary>
        /// 年間収益を計算
        /// </summary>
        private Money CalculateYearlyIncome(Player player)
        {
            Money totalIncome = Money.Zero;

            foreach (var property in player.OwnedProperties)
            {
                var income = property.CalculateIncome();

                // 独占チェック
                if (StationNetwork.GetStation(property.Location.Id)?.IsMonopolizedBy(player) ?? false)
                {
                    property.HasMonopolyBonus = true;
                    income = income * 2; // 独占ボーナスで2倍
                }

                // ヒーロー効果（後で実装）
                if (player.AssignedHero != null)
                {
                    // player.AssignedHero.ApplyIncomeBonus(ref income, property);
                }

                totalIncome += income;
            }

            return totalIncome;
        }

        /// <summary>
        /// プレイヤーの順位を更新
        /// </summary>
        public void UpdatePlayerRankings()
        {
            var rankedPlayers = Players
                .OrderByDescending(p => p.TotalAssets)
                .ToList();

            for (int i = 0; i < rankedPlayers.Count; i++)
            {
                rankedPlayers[i].Rank = i + 1;
            }

            // ボンビー付着処理（最下位プレイヤー）
            if (ActiveBonby != null && rankedPlayers.Count > 0)
            {
                var lastPlayer = rankedPlayers.Last();
                if (lastPlayer.AttachedBonby == null)
                {
                    lastPlayer.AttachedBonby = ActiveBonby;
                }
            }
        }

        /// <summary>
        /// ダイヤ改正レベルを計算
        /// </summary>
        private int CalculateDiamondRevisionLevel()
        {
            return CurrentYear switch
            {
                < 17 => 1,   // 1〜16年目: 1倍
                < 37 => 2,   // 17〜36年目: 2倍
                < 57 => 4,   // 37〜56年目: 4倍
                < 77 => 8,   // 57〜76年目: 8倍
                _ => 16      // 77年目以降: 16倍
            };
        }

        /// <summary>
        /// ゲーム終了判定
        /// </summary>
        public bool IsGameOver()
        {
            return CurrentYear > MaxYears;
        }

        /// <summary>
        /// 目的地到着処理
        /// </summary>
        public void ProcessDestinationArrival(Player player)
        {
            if (Destination == null || player.CurrentStation != Destination)
                return;

            // 到着ボーナス
            var bonus = CalculateDestinationBonus();
            player.CurrentMoney += bonus;

            AddEvent(new GameEvent
            {
                Type = EventType.DestinationArrival,
                Description = $"{player.Name}が{Destination.Name}に到着！ ボーナス: {bonus}",
                Amount = bonus,
                PlayerId = player.Id
            });

            // ボンビー移動処理
            if (player.AttachedBonby != null)
            {
                TransferBonby(player);
            }

            // 新しい目的地を設定
            SetNewDestination();
        }

        /// <summary>
        /// 目的地到着ボーナスを計算
        /// </summary>
        private Money CalculateDestinationBonus()
        {
            // 基本ボーナス（年数に応じて増加）
            var baseBonus = 100000000L * CurrentYear; // 1億円 × 年数

            // 距離ボーナス（後で実装）
            var distanceBonus = 0L;

            return new Money(baseBonus + distanceBonus);
        }

        /// <summary>
        /// ボンビーを他のプレイヤーに移動
        /// </summary>
        private void TransferBonby(Player fromPlayer)
        {
            fromPlayer.AttachedBonby = null;

            // 最も近いプレイヤーに付着
            var targetPlayer = Players
                .Where(p => p.Id != fromPlayer.Id)
                .OrderBy(p => p.CurrentStation?.DistanceTo(fromPlayer.CurrentStation!) ?? double.MaxValue)
                .FirstOrDefault();

            if (targetPlayer != null && ActiveBonby != null)
            {
                targetPlayer.AttachedBonby = ActiveBonby;

                AddEvent(new GameEvent
                {
                    Type = EventType.BonbyTransfer,
                    Description = $"ボンビーが{targetPlayer.Name}に移動！",
                    PlayerId = targetPlayer.Id
                });
            }
        }

        /// <summary>
        /// 新しい目的地を設定
        /// </summary>
        private void SetNewDestination()
        {
            var propertyStations = StationNetwork.GetStationsByType(StationType.Property);

            if (propertyStations.Count == 0)
                return;

            var random = new Random();
            Station newDestination;

            // 現在の目的地と異なる駅を選択
            do
            {
                newDestination = propertyStations[random.Next(propertyStations.Count)];
            } while (newDestination == Destination && propertyStations.Count > 1);

            Destination = newDestination;
            Destination.IsDestination = true;

            AddEvent(new GameEvent
            {
                Type = EventType.DestinationChange,
                Description = $"新しい目的地: {Destination.Name}！"
            });
        }

        /// <summary>
        /// イベントを追加
        /// </summary>
        public void AddEvent(GameEvent gameEvent)
        {
            gameEvent.OccurredAt = DateTime.Now;
            gameEvent.Turn = CurrentTurn;
            gameEvent.Year = CurrentYear;
            gameEvent.Month = CurrentMonth;

            EventHistory.Add(gameEvent);

            // 履歴は最新1000件まで保持
            if (EventHistory.Count > 1000)
            {
                EventHistory.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// ゲーム設定
    /// </summary>
    public class GameSettings
    {
        public GameMode GameMode { get; set; } = GameMode.Normal;
        public int MaxYears { get; set; } = 10;
        public ComDifficulty ComDifficulty { get; set; } = ComDifficulty.Normal;
        public int ComPlayerCount { get; set; } = 3;
        public bool EnableBonby { get; set; } = true;
        public bool EnableSpecialEvents { get; set; } = true;
        public float BgmVolume { get; set; } = 0.7f;
        public float SeVolume { get; set; } = 0.8f;
        public AnimationSpeed AnimationSpeed { get; set; } = AnimationSpeed.Normal;

        // プレイヤー情報（一時的）
        public string PlayerName { get; set; } = "プレイヤー";
        public PlayerColor PlayerColor { get; set; } = PlayerColor.Blue;
    }

    /// <summary>
    /// ゲームイベント
    /// </summary>
    public class GameEvent
    {
        public EventType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public int Turn { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public Guid? PlayerId { get; set; }
        public Money? Amount { get; set; }
    }

    /// <summary>
    /// イベントタイプ
    /// </summary>
    public enum EventType
    {
        GameStart,
        TurnStart,
        DiceRoll,
        Movement,
        PropertyPurchase,
        PropertySale,
        CardUsage,
        YearlyIncome,
        Interest,
        DestinationArrival,
        DestinationChange,
        BonbyAppear,
        BonbyTransfer,
        BonbyMischief,
        HeroAcquired,
        SpecialEvent,
        MarketChange,
        MonopolyAchieved,
        GameEnd
    }