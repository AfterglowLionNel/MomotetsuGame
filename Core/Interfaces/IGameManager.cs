using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// ゲーム管理のインターフェース
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
        /// ゲームイベント
        /// </summary>
        event EventHandler<GameEventArgs>? GameEvent;

        /// <summary>
        /// ターン変更イベント
        /// </summary>
        event EventHandler<TurnChangedEventArgs>? TurnChanged;

        /// <summary>
        /// プレイヤー移動イベント
        /// </summary>
        event EventHandler<PlayerMovedEventArgs>? PlayerMoved;

        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        Task<GameState> StartNewGameAsync(GameSettings settings);

        /// <summary>
        /// ゲームをロード
        /// </summary>
        Task<GameState> LoadGameAsync(string saveId);

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        Task SaveGameAsync(string saveId);

        /// <summary>
        /// サイコロを振る
        /// </summary>
        Task<DiceResult> RollDiceAsync();

        /// <summary>
        /// プレイヤーを移動
        /// </summary>
        Task<MoveResult> MovePlayerAsync(Player player, int steps);

        /// <summary>
        /// カードを使用
        /// </summary>
        Task<CardEffectResult> UseCardAsync(Card card);

        /// <summary>
        /// 物件を購入
        /// </summary>
        Task<bool> PurchasePropertyAsync(Player player, Property property);

        /// <summary>
        /// 物件を売却
        /// </summary>
        Task<bool> SellPropertyAsync(Player player, Property property);

        /// <summary>
        /// ターンを終了
        /// </summary>
        Task EndTurnAsync();

        /// <summary>
        /// 目的地到着処理
        /// </summary>
        Task ProcessDestinationArrivalAsync(Player player);

        /// <summary>
        /// 移動可能な駅を取得
        /// </summary>
        List<Station> GetReachableStations(Station from, int steps);

        /// <summary>
        /// ゲーム終了チェック
        /// </summary>
        bool IsGameOver();

        /// <summary>
        /// 最終結果を取得
        /// </summary>
        GameResult GetGameResult();
    }

    /// <summary>
    /// ゲームイベント引数の基底クラス
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.Now;
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
    }

    /// <summary>
    /// 移動結果
    /// </summary>
    public class MoveResult
    {
        public Player Player { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
        public Station FinalStation { get; set; } = null!;
        public bool ReachedDestination { get; set; }
        public List<string> Events { get; set; } = new List<string>();
    }

    /// <summary>
    /// ゲーム結果
    /// </summary>
    public class GameResult
    {
        public List<PlayerResult> PlayerResults { get; set; } = new List<PlayerResult>();
        public Player Winner { get; set; } = null!;
        public int TotalTurns { get; set; }
        public int Years { get; set; }
        public DateTime GameDuration { get; set; }
    }

    /// <summary>
    /// プレイヤー結果
    /// </summary>
    public class PlayerResult
    {
        public Player Player { get; set; } = null!;
        public int FinalRank { get; set; }
        public Money FinalAssets { get; set; }
        public int PropertyCount { get; set; }
        public int DestinationCount { get; set; }
        public Money MaxAssets { get; set; }
    }
}