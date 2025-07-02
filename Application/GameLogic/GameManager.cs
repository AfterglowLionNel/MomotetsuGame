using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// ゲーム全体の進行を管理するインターフェース
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        /// 現在のプレイヤー
        /// </summary>
        Player CurrentPlayer { get; }

        /// <summary>
        /// ゲーム進行イベント
        /// </summary>
        event EventHandler<GameEventArgs>? GameEventOccurred;

        /// <summary>
        /// ターン変更イベント
        /// </summary>
        event EventHandler<TurnChangedEventArgs>? TurnChanged;

        /// <summary>
        /// プレイヤー移動イベント
        /// </summary>
        event EventHandler<PlayerMovedEventArgs>? PlayerMoved;

        /// <summary>
        /// 金額変更イベント
        /// </summary>
        event EventHandler<MoneyChangedEventArgs>? MoneyChanged;

        /// <summary>
        /// 物件購入イベント
        /// </summary>
        event EventHandler<PropertyPurchasedEventArgs>? PropertyPurchased;

        /// <summary>
        /// カード使用イベント
        /// </summary>
        event EventHandler<CardUsedEventArgs>? CardUsed;

        /// <summary>
        /// ゲームメッセージイベント
        /// </summary>
        event EventHandler<GameMessageEventArgs>? GameMessage;

        // ゲーム開始・終了
        Task<GameState> StartNewGameAsync(GameSettings settings);
        Task<GameState> LoadGameAsync(string saveId);
        Task SaveGameAsync(string saveId);
        Task EndGameAsync();

        // ターン進行
        Task StartTurnAsync();
        Task EndTurnAsync();
        Task<DiceResult> RollDiceAsync();
        Task<MoveResult> MovePlayerAsync(Player player, int steps);
        Task ProcessArrivalAsync(Player player, Station station);

        // プレイヤーアクション
        Task<PurchaseResult> PurchasePropertiesAsync(Player player, List<Property> properties);
        Task<bool> SellPropertyAsync(Player player, Property property);
        Task<CardEffectResult> UseCardAsync(Card card);
        Task<List<Card>> ShowCardShopAsync(Station station);
        Task<bool> PurchaseCardAsync(Player player, Card card);

        // 目的地関連
        void SetDestination(Station station);
        Station GetDestination();
        Task ProcessDestinationArrivalAsync(Player player);

        // イベント処理
        Task ProcessSpecialStationAsync(Station station);
        Task ProcessMonthEndAsync();
        Task ProcessYearEndAsync();

        // ユーティリティ
        List<Station> GetRoute(Station from, int steps);
        Money CalculatePlusStationBonus();
        Money CalculateMinusStationPenalty();
        bool CheckMonopoly(Player player, Station station);
        void UpdatePlayerRankings();
    }

    /// <summary>
    /// 移動結果
    /// </summary>
    public class MoveResult
    {
        public Player Player { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
        public Station FinalStation { get; set; } = null!;
        public List<Station> PassedStations { get; set; } = new List<Station>();
        public bool ReachedDestination { get; set; }
    }

    /// <summary>
    /// 購入結果
    /// </summary>
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// カード効果結果
    /// </summary>
    public class CardEffectResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool AllowContinuation { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    #region Event Args

    /// <summary>
    /// ゲームイベント引数の基底クラス
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        public DateTime OccurredAt { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// ターン変更イベント引数
    /// </summary>
    public class TurnChangedEventArgs : GameEventArgs
    {
        public Player CurrentPlayer { get; set; } = null!;
        public int Turn { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    /// <summary>
    /// プレイヤー移動イベント引数
    /// </summary>
    public class PlayerMovedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Station FromStation { get; set; } = null!;
        public Station ToStation { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
        public int Steps { get; set; }
    }

    /// <summary>
    /// 金額変更イベント引数
    /// </summary>
    public class MoneyChangedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Money OldAmount { get; set; }
        public Money NewAmount { get; set; }
        public Money Difference { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 物件購入イベント引数
    /// </summary>
    public class PropertyPurchasedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public List<Property> Properties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; }
        public bool AchievedMonopoly { get; set; }
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
    public class GameMessageEventArgs : GameEventArgs
    {
        public MessageType Type { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// メッセージタイプ
    /// </summary>
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Important
    }

    #endregion
}