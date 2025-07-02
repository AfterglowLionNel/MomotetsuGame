using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// コンピュータAIのインターフェース
    /// </summary>
    public interface IComputerAI
    {
        /// <summary>
        /// AI難易度
        /// </summary>
        ComDifficulty Difficulty { get; set; }

        /// <summary>
        /// 行動を決定
        /// </summary>
        /// <param name="context">ゲームコンテキスト</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <returns>決定した行動</returns>
        Task<ActionDecision> DecideAction(AIContext context, Player player);

        /// <summary>
        /// 分岐点での方向を選択
        /// </summary>
        /// <param name="options">選択可能な駅</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>選択した駅</returns>
        Task<Station> SelectDestination(List<Station> options, Player player, AIContext context);

        /// <summary>
        /// 購入する物件を選択
        /// </summary>
        /// <param name="available">購入可能な物件</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>購入する物件のリスト</returns>
        Task<List<Property>> SelectPropertiesToBuy(List<Property> available, Player player, AIContext context);

        /// <summary>
        /// 使用するカードを選択
        /// </summary>
        /// <param name="available">使用可能なカード</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>使用するカード（使用しない場合null）</returns>
        Task<Card?> SelectCardToUse(List<Card> available, Player player, AIContext context);

        /// <summary>
        /// カード売り場で購入するカードを選択
        /// </summary>
        /// <param name="available">購入可能なカード</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>購入するカード（購入しない場合null）</returns>
        Task<Card?> SelectCardToBuy(List<Card> available, Player player, AIContext context);

        /// <summary>
        /// 売却する物件を選択（資金不足時）
        /// </summary>
        /// <param name="properties">所有物件</param>
        /// <param name="targetAmount">必要金額</param>
        /// <param name="player">操作するプレイヤー</param>
        /// <returns>売却する物件のリスト</returns>
        Task<List<Property>> SelectPropertiesToSell(List<Property> properties, Money targetAmount, Player player);
    }

    /// <summary>
    /// AI用ゲームコンテキスト
    /// </summary>
    public class AIContext
    {
        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        public GameState GameState { get; set; } = null!;

        /// <summary>
        /// 全プレイヤー情報
        /// </summary>
        public List<Player> AllPlayers { get; set; } = new List<Player>();

        /// <summary>
        /// 現在の目的地
        /// </summary>
        public Station Destination { get; set; } = null!;

        /// <summary>
        /// 目的地までの最短距離
        /// </summary>
        public int DistanceToDestination { get; set; }

        /// <summary>
        /// 1位との総資産差
        /// </summary>
        public Money AssetGapToFirst { get; set; }

        /// <summary>
        /// ゲーム進行度（0.0〜1.0）
        /// </summary>
        public double GameProgress { get; set; }

        /// <summary>
        /// 利用可能な駅ネットワーク
        /// </summary>
        public StationNetwork StationNetwork { get; set; } = null!;

        /// <summary>
        /// 市場情報
        /// </summary>
        public PropertyMarket PropertyMarket { get; set; } = null!;
    }

    /// <summary>
    /// AI行動決定結果
    /// </summary>
    public class ActionDecision
    {
        /// <summary>
        /// 行動タイプ
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// 選択したカード（カード使用時）
        /// </summary>
        public Card? SelectedCard { get; set; }

        /// <summary>
        /// 優先度（0〜100）
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 理由（デバッグ用）
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// AI行動タイプ
    /// </summary>
    public enum ActionType
    {
        RollDice,      // サイコロを振る
        UseCard,       // カードを使用
        Wait           // 待機（特殊な場合）
    }

    /// <summary>
    /// AI戦略インターフェース
    /// </summary>
    public interface IAIStrategy
    {
        /// <summary>
        /// 物件評価
        /// </summary>
        double EvaluateProperty(Property property, AIAnalysis analysis);

        /// <summary>
        /// カード評価
        /// </summary>
        double EvaluateCard(Card card, AIAnalysis analysis);

        /// <summary>
        /// 経路評価
        /// </summary>
        double EvaluateRoute(Station destination, AIAnalysis analysis);

        /// <summary>
        /// リスク許容度を取得
        /// </summary>
        double GetRiskTolerance(AIAnalysis analysis);
    }

    /// <summary>
    /// AI分析データ
    /// </summary>
    public class AIAnalysis
    {
        /// <summary>
        /// プレイヤーの順位
        /// </summary>
        public int PlayerRank { get; set; }

        /// <summary>
        /// 目的地までの距離
        /// </summary>
        public int DistanceToDestination { get; set; }

        /// <summary>
        /// 所持金の割合（全プレイヤー中）
        /// </summary>
        public double MoneyRatio { get; set; }

        /// <summary>
        /// ゲーム進行度
        /// </summary>
        public double TurnProgress { get; set; }

        /// <summary>
        /// ボンビーが付いているか
        /// </summary>
        public bool HasBonby { get; set; }

        /// <summary>
        /// 1位との総資産差
        /// </summary>
        public Money TopPlayerLead { get; set; }

        /// <summary>
        /// 独占可能な駅のリスト
        /// </summary>
        public List<Station> MonopolizableStations { get; set; } = new List<Station>();

        /// <summary>
        /// 脅威となるプレイヤー
        /// </summary>
        public List<Player> ThreateningPlayers { get; set; } = new List<Player>();
    }

    /// <summary>
    /// AI性格タイプ
    /// </summary>
    public enum AIPersonality
    {
        Aggressive,    // 攻撃的（高額物件狙い、攻撃カード多用）
        Defensive,     // 守備的（安定重視、防御カード優先）
        Balanced,      // バランス型（状況に応じて変化）
        Opportunistic, // 機会主義（独占狙い、カード収集）
        Speedster      // スピード重視（移動カード優先）
    }

    /// <summary>
    /// AIユーティリティ関数
    /// </summary>
    public static class AIUtility
    {
        /// <summary>
        /// 目的地への最短距離を計算
        /// </summary>
        public static int CalculateDistanceToDestination(Station current, Station destination, StationNetwork network)
        {
            // TODO: ダイクストラ法などで実装
            return 10; // 仮実装
        }

        /// <summary>
        /// 独占可能性を評価
        /// </summary>
        public static double EvaluateMonopolyPotential(Player player, Station station)
        {
            var ownedCount = station.Properties.Count(p => p.Owner == player);
            var totalCount = station.Properties.Count;

            if (totalCount == 0) return 0;

            var ratio = (double)ownedCount / totalCount;

            // 既に半分以上所有している場合は高評価
            if (ratio >= 0.5) return ratio * 2.0;

            return ratio;
        }

        /// <summary>
        /// プレイヤーの脅威度を評価
        /// </summary>
        public static double EvaluateThreatLevel(Player target, Player self)
        {
            var assetRatio = (double)target.TotalAssets.Value / self.TotalAssets.Value;
            var rankDiff = self.Rank - target.Rank;

            // 総資産が自分の2倍以上なら高脅威
            if (assetRatio >= 2.0) return 1.0;

            // 順位が上なら脅威度上昇
            if (rankDiff > 0) return 0.5 + (rankDiff * 0.1);

            return Math.Max(0, assetRatio - 1.0);
        }
    }
}