using System;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Application.AI
{
    /// <summary>
    /// バランス型AI戦略
    /// </summary>
    public class BalancedAIStrategy : IAIStrategy
    {
        public double EvaluateProperty(Property property, AIAnalysis analysis)
        {
            var score = 50.0;

            // 収益率による評価
            score += property.IncomeRate * 100;

            // 価格による評価（高すぎる物件は控える）
            if (property.CurrentPrice.Value > 10000000000) // 100億以上
            {
                score -= 20;
            }

            // カテゴリによる評価
            score += property.Category switch
            {
                PropertyCategory.Tourism => 15,    // 観光は高収益
                PropertyCategory.Industry => 10,   // 工業も良い
                PropertyCategory.Commerce => 5,    // 商業は普通
                PropertyCategory.Agriculture => 0, // 農林は安定
                PropertyCategory.Fishery => -5,    // 水産は変動大
                _ => 0
            };

            // 独占可能性による評価
            var station = property.Location;
            var ownedCount = station.Properties.Count(p => p.Owner?.Id == analysis.PlayerId);
            if (ownedCount > 0)
            {
                score += ownedCount * 15; // 既に持っている駅は高評価
            }

            // ゲーム進行度による調整
            if (analysis.TurnProgress > 0.7)
            {
                // 終盤は高額物件を重視
                if (property.CurrentPrice.Value > 5000000000)
                {
                    score += 20;
                }
            }

            return Math.Max(0, Math.Min(100, score));
        }

        public double EvaluateCard(Card card, AIAnalysis analysis)
        {
            var score = 50.0;

            // カードタイプによる基本評価
            score += card.Type switch
            {
                CardType.Movement => EvaluateMovementCard(card, analysis),
                CardType.Convenience => EvaluateConvenienceCard(card, analysis),
                CardType.Attack => EvaluateAttackCard(card, analysis),
                CardType.Defense => EvaluateDefenseCard(card, analysis),
                CardType.Special => 30,
                _ => 0
            };

            // レア度による補正
            score += card.Rarity switch
            {
                CardRarity.SS => 20,
                CardRarity.S => 15,
                CardRarity.A => 10,
                CardRarity.B => 5,
                CardRarity.C => 0,
                _ => 0
            };

            return Math.Max(0, Math.Min(100, score));
        }

        public double EvaluateRoute(Station destination, AIAnalysis analysis)
        {
            var score = 50.0;

            // 目的地への距離
            if (analysis.DistanceToDestination > 0)
            {
                score -= analysis.DistanceToDestination * 5;
            }

            // 駅タイプによる評価
            score += destination.Type switch
            {
                StationType.Property => 20,
                StationType.Plus => 30,
                StationType.Minus => -30,
                StationType.CardShop => 15,
                StationType.NiceCard => 25,
                StationType.SuperCard => 35,
                _ => 0
            };

            return Math.Max(0, Math.Min(100, score));
        }

        public double GetRiskTolerance(AIAnalysis analysis)
        {
            // 状況に応じてリスク許容度を調整
            if (analysis.PlayerRank == 1)
            {
                // 1位の場合は守備的
                return 0.3;
            }
            else if (analysis.PlayerRank >= 3 && analysis.TurnProgress > 0.6)
            {
                // 劣勢かつ終盤は積極的
                return 0.8;
            }

            return 0.5; // 通常は中間
        }

        protected virtual double EvaluateMovementCard(Card card, AIAnalysis analysis)
        {
            // 目的地が近い場合は移動カードの価値が上がる
            if (analysis.DistanceToDestination <= 10)
            {
                return 30;
            }
            return 15;
        }

        protected virtual double EvaluateConvenienceCard(Card card, AIAnalysis analysis)
        {
            // 便利系カードは状況次第
            return 20;
        }

        protected virtual double EvaluateAttackCard(Card card, AIAnalysis analysis)
        {
            // 劣勢の場合は攻撃カードの価値が上がる
            if (analysis.PlayerRank >= 3)
            {
                return 25;
            }
            return 10;
        }

        protected virtual double EvaluateDefenseCard(Card card, AIAnalysis analysis)
        {
            // ボンビーが付いている場合は防御カードの価値が上がる
            if (analysis.HasBonby)
            {
                return 30;
            }
            return 15;
        }
    }

    /// <summary>
    /// 攻撃的AI戦略
    /// </summary>
    public class AggressiveAIStrategy : BalancedAIStrategy
    {
        public override double EvaluateProperty(Property property, AIAnalysis analysis)
        {
            var score = base.EvaluateProperty(property, analysis);

            // 高額物件を優遇
            if (property.CurrentPrice.Value > 5000000000) // 50億以上
            {
                score += 20;
            }

            // 観光物件を特に優遇
            if (property.Category == PropertyCategory.Tourism)
            {
                score += 10;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        protected override double EvaluateMovementCard(Card card, AIAnalysis analysis)
        {
            // 常に移動を重視
            return 35;
        }

        protected override double EvaluateAttackCard(Card card, AIAnalysis analysis)
        {
            // 攻撃カードを高評価
            return 40;
        }

        public override double GetRiskTolerance(AIAnalysis analysis)
        {
            // 高リスク許容
            return 0.8;
        }
    }

    /// <summary>
    /// 守備的AI戦略
    /// </summary>
    public class ConservativeAIStrategy : BalancedAIStrategy
    {
        public override double EvaluateProperty(Property property, AIAnalysis analysis)
        {
            var score = base.EvaluateProperty(property, analysis);

            // 低価格物件を優遇
            if (property.CurrentPrice.Value < 1000000000) // 10億未満
            {
                score += 15;
            }

            // 農林物件を優遇（安定収益）
            if (property.Category == PropertyCategory.Agriculture)
            {
                score += 10;
            }

            // 高額物件にペナルティ
            if (property.CurrentPrice.Value > 10000000000) // 100億以上
            {
                score -= 30;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        protected override double EvaluateDefenseCard(Card card, AIAnalysis analysis)
        {
            // 防御カードを高評価
            return 35;
        }

        protected override double EvaluateAttackCard(Card card, AIAnalysis analysis)
        {
            // 攻撃カードは控えめ
            return 5;
        }

        public override double GetRiskTolerance(AIAnalysis analysis)
        {
            // 低リスク許容
            return 0.2;
        }
    }

    /// <summary>
    /// 機会主義的AI戦略（独占狙い）
    /// </summary>
    public class OpportunisticAIStrategy : BalancedAIStrategy
    {
        public override double EvaluateProperty(Property property, AIAnalysis analysis)
        {
            var score = base.EvaluateProperty(property, analysis);

            // 独占可能性を最重視
            var station = property.Location;
            var ownedCount = station.Properties.Count(p => p.Owner?.Id == analysis.PlayerId);
            var totalCount = station.Properties.Count;

            if (ownedCount > 0)
            {
                // 既に物件を持っている駅を超優遇
                score += ownedCount * 25;

                // あと1件で独占の場合は最優先
                if (ownedCount == totalCount - 1)
                {
                    score = 100;
                }
            }

            return Math.Max(0, Math.Min(100, score));
        }

        protected override double EvaluateConvenienceCard(Card card, AIAnalysis analysis)
        {
            // カード収集を重視
            return 30;
        }

        public override double EvaluateRoute(Station destination, AIAnalysis analysis)
        {
            var score = base.EvaluateRoute(destination, analysis);

            // 既に物件を持っている駅への移動を優先
            if (destination.Type == StationType.Property)
            {
                var ownedCount = destination.Properties.Count(p => p.Owner?.Id == analysis.PlayerId);
                if (ownedCount > 0)
                {
                    score += ownedCount * 20;
                }
            }

            return Math.Max(0, Math.Min(100, score));
        }
    }

    /// <summary>
    /// スピード重視AI戦略
    /// </summary>
    public class SpeedsterAIStrategy : BalancedAIStrategy
    {
        protected override double EvaluateMovementCard(Card card, AIAnalysis analysis)
        {
            // 移動カードを最優先
            return 50;
        }

        public override double EvaluateRoute(Station destination, AIAnalysis analysis)
        {
            var score = base.EvaluateRoute(destination, analysis);

            // 目的地への最短距離を最重視
            if (analysis.DistanceToDestination > 0)
            {
                score = 100 - (analysis.DistanceToDestination * 10);
            }

            return Math.Max(0, Math.Min(100, score));
        }

        public override double EvaluateProperty(Property property, AIAnalysis analysis)
        {
            var score = base.EvaluateProperty(property, analysis);

            // 物件購入は控えめ（資金を移動カードに回す）
            score -= 20;

            return Math.Max(0, Math.Min(100, score));
        }
    }

    /// <summary>
    /// AI分析データ（拡張）
    /// </summary>
    public partial class AIAnalysis
    {
        /// <summary>
        /// 分析対象プレイヤーのID
        /// </summary>
        public Guid PlayerId { get; set; }
    }
}