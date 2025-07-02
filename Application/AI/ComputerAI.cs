using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Application.Services;
using SkiaSharp;

namespace MomotetsuGame.Application.AI
{
    /// <summary>
    /// コンピュータAIの実装
    /// </summary>
    public class ComputerAI : IComputerAI
    {
        private readonly IRouteCalculator _routeCalculator;
        private readonly IAIStrategy _strategy;
        private readonly Random _random;

        /// <summary>
        /// AI難易度
        /// </summary>
        public ComDifficulty Difficulty { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ComputerAI(
            IRouteCalculator routeCalculator,
            ComDifficulty difficulty = ComDifficulty.Normal,
            IAIStrategy? strategy = null)
        {
            _routeCalculator = routeCalculator ?? throw new ArgumentNullException(nameof(routeCalculator));
            Difficulty = difficulty;
            _strategy = strategy ?? CreateDefaultStrategy(difficulty);
            _random = new Random();
        }

        /// <summary>
        /// 行動を決定
        /// </summary>
        public async Task<ActionDecision> DecideAction(AIContext context, Player player)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (player == null) throw new ArgumentNullException(nameof(player));

            // 状況分析
            var analysis = AnalyzeGameState(context, player);

            // カード使用判定
            if (player.Cards.Any() && ShouldUseCard(analysis, player))
            {
                var cardToUse = await SelectCardToUse(player.Cards, player, context);
                if (cardToUse != null)
                {
                    return new ActionDecision
                    {
                        Type = ActionType.UseCard,
                        SelectedCard = cardToUse,
                        Priority = 80,
                        Reason = "戦略的にカードを使用"
                    };
                }
            }

            // 通常はサイコロを振る
            return new ActionDecision
            {
                Type = ActionType.RollDice,
                Priority = 100,
                Reason = "通常移動"
            };
        }

        /// <summary>
        /// 分岐点での方向を選択
        /// </summary>
        public async Task<Station> SelectDestination(List<Station> options, Player player, AIContext context)
        {
            if (options == null || options.Count == 0)
                throw new ArgumentException("選択可能な駅がありません。", nameof(options));

            var evaluations = new List<(Station station, double score)>();

            foreach (var option in options)
            {
                var score = EvaluateRoute(option, player, context);
                evaluations.Add((option, score));
            }

            // 難易度による選択
            switch (Difficulty)
            {
                case ComDifficulty.Weak:
                    // 弱いAIはランダム性が高い
                    if (_random.Next(100) < 50)
                    {
                        return options[_random.Next(options.Count)];
                    }
                    break;

                case ComDifficulty.Strong:
                    // 強いAIは常に最適な選択
                    return evaluations.OrderByDescending(e => e.score).First().station;
            }

            // 通常は上位の中からランダムに選択
            var topChoices = evaluations
                .OrderByDescending(e => e.score)
                .Take(Math.Max(2, options.Count / 2))
                .ToList();

            var selected = topChoices[_random.Next(topChoices.Count)].station;

            await Task.CompletedTask;
            return selected;
        }

        /// <summary>
        /// 購入する物件を選択
        /// </summary>
        public async Task<List<Property>> SelectPropertiesToBuy(List<Property> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return new List<Property>();

            var selected = new List<Property>();
            var remainingMoney = player.CurrentMoney;
            var analysis = AnalyzeGameState(context, player);

            // 物件を評価してソート
            var evaluated = available
                .Select(p => new
                {
                    Property = p,
                    Score = _strategy.EvaluateProperty(p, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            // 難易度による調整
            var purchaseThreshold = Difficulty switch
            {
                ComDifficulty.Weak => 0.3,    // 30%の確率で購入
                ComDifficulty.Normal => 0.6,  // 60%の確率で購入
                ComDifficulty.Strong => 0.9,  // 90%の確率で購入
                _ => 0.6
            };

            // 購入判定
            foreach (var item in evaluated)
            {
                if (item.Score < 50) break; // スコアが低い物件は購入しない

                if (_random.NextDouble() < purchaseThreshold)
                {
                    if (remainingMoney >= item.Property.CurrentPrice)
                    {
                        selected.Add(item.Property);
                        remainingMoney -= item.Property.CurrentPrice;

                        // 独占可能性チェック
                        if (WouldAchieveMonopoly(player, item.Property))
                        {
                            // 独占のために同じ駅の他の物件も購入
                            var stationProperties = available
                                .Where(p => p.Location == item.Property.Location && !selected.Contains(p))
                                .ToList();

                            foreach (var prop in stationProperties)
                            {
                                if (remainingMoney >= prop.CurrentPrice)
                                {
                                    selected.Add(prop);
                                    remainingMoney -= prop.CurrentPrice;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            await Task.CompletedTask;
            return selected;
        }

        /// <summary>
        /// 使用するカードを選択
        /// </summary>
        public async Task<Card?> SelectCardToUse(List<Card> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return null;

            var analysis = AnalyzeGameState(context, player);
            var usableCards = available.Where(c => c.CanUse(player, context.GameState)).ToList();

            if (!usableCards.Any())
                return null;

            // カードを評価
            var evaluated = usableCards
                .Select(c => new
                {
                    Card = c,
                    Score = _strategy.EvaluateCard(c, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            // 最高スコアのカードを選択（難易度による調整あり）
            var bestCard = evaluated.FirstOrDefault();
            if (bestCard != null && bestCard.Score > 60)
            {
                if (Difficulty == ComDifficulty.Weak && _random.Next(100) < 30)
                {
                    // 弱いAIは30%の確率で使用をやめる
                    return null;
                }

                await Task.CompletedTask;
                return bestCard.Card;
            }

            return null;
        }

        /// <summary>
        /// カード売り場で購入するカードを選択
        /// </summary>
        public async Task<Card?> SelectCardToBuy(List<Card> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return null;

            var analysis = AnalyzeGameState(context, player);

            // 所持カード数チェック
            if (player.Cards.Count >= player.MaxCardCount)
                return null;

            // カードを評価
            var evaluated = available
                .Where(c => player.CurrentMoney >= c.BasePrice)
                .Select(c => new
                {
                    Card = c,
                    Score = _strategy.EvaluateCard(c, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            var bestCard = evaluated.FirstOrDefault();
            if (bestCard != null && bestCard.Score > 70)
            {
                await Task.CompletedTask;
                return bestCard.Card;
            }

            return null;
        }

        /// <summary>
        /// 売却する物件を選択（資金不足時）
        /// </summary>
        public async Task<List<Property>> SelectPropertiesToSell(List<Property> properties, Money targetAmount, Player player)
        {
            if (properties == null || properties.Count == 0)
                return new List<Property>();

            var toSell = new List<Property>();
            var currentAmount = Money.Zero;

            // 収益率の低い物件から売却
            var sorted = properties
                .OrderBy(p => p.IncomeRate)
                .ThenBy(p => p.CurrentPrice)
                .ToList();

            foreach (var property in sorted)
            {
                if (currentAmount >= targetAmount)
                    break;

                // 独占を崩さないようにチェック
                if (!WouldBreakMonopoly(player, property))
                {
                    toSell.Add(property);
                    currentAmount += property.GetSellPrice();
                }
            }

            // 必要額に達しない場合は独占も崩す
            if (currentAmount < targetAmount)
            {
                foreach (var property in sorted.Where(p => !toSell.Contains(p)))
                {
                    if (currentAmount >= targetAmount)
                        break;

                    toSell.Add(property);
                    currentAmount += property.GetSellPrice();
                }
            }

            await Task.CompletedTask;
            return toSell;
        }

        #region Private Methods

        /// <summary>
        /// ゲーム状態を分析
        /// </summary>
        private AIAnalysis AnalyzeGameState(AIContext context, Player player)
        {
            var analysis = new AIAnalysis
            {
                PlayerRank = player.Rank,
                DistanceToDestination = _routeCalculator.GetDistanceToDestination(
                    player.CurrentStation!,
                    context.Destination),
                TurnProgress = context.GameProgress,
                HasBonby = player.AttachedBonby != null
            };

            // 総資産比率を計算
            var totalAssets = context.AllPlayers.Sum(p => p.TotalAssets.Value);
            if (totalAssets > 0)
            {
                analysis.MoneyRatio = (double)player.TotalAssets.Value / totalAssets;
            }

            // 1位との差を計算
            var firstPlayer = context.AllPlayers.OrderByDescending(p => p.TotalAssets).First();
            if (firstPlayer != player)
            {
                analysis.TopPlayerLead = firstPlayer.TotalAssets - player.TotalAssets;
            }

            // 独占可能駅を検出
            var stations = context.StationNetwork.GetAllStations();
            foreach (var station in stations.Where(s => s.Properties.Any()))
            {
                var ownedCount = station.Properties.Count(p => p.Owner == player);
                var totalCount = station.Properties.Count;

                if (ownedCount > 0 && ownedCount < totalCount)
                {
                    analysis.MonopolizableStations.Add(station);
                }
            }

            // 脅威プレイヤーを特定
            analysis.ThreateningPlayers = context.AllPlayers
                .Where(p => p != player && p.TotalAssets > player.TotalAssets)
                .OrderByDescending(p => p.TotalAssets)
                .ToList();

            return analysis;
        }

        /// <summary>
        /// カード使用すべきか判定
        /// </summary>
        private bool ShouldUseCard(AIAnalysis analysis, Player player)
        {
            // 目的地が近い場合
            if (analysis.DistanceToDestination <= 6)
                return true;

            // 劣勢の場合
            if (analysis.PlayerRank > 2 && analysis.TurnProgress > 0.5)
                return true;

            // ボンビーが付いている場合
            if (analysis.HasBonby)
                return true;

            // 絶好調の場合
            if (player.Status == PlayerStatus.SuperLucky)
                return true;

            return false;
        }

        /// <summary>
        /// ルートを評価
        /// </summary>
        private double EvaluateRoute(Station destination, Player player, AIContext context)
        {
            var score = 100.0;

            // 目的地への距離
            var distanceToGoal = _routeCalculator.GetDistanceToDestination(
                destination,
                context.Destination);
            score -= distanceToGoal * 10;

            // 駅の種類による評価
            switch (destination.Type)
            {
                case StationType.Property:
                    if (destination.GetAvailablePropertyCount() > 0)
                        score += 30;
                    if (destination.GetPropertyCountOwnedBy(player) > 0)
                        score += 20; // 既に物件を持っている駅は独占のチャンス
                    break;

                case StationType.Plus:
                    score += 40;
                    break;

                case StationType.Minus:
                    score -= 40;
                    break;

                case StationType.CardShop:
                    if (player.Cards.Count < player.MaxCardCount)
                        score += 25;
                    break;
            }

            return score;
        }

        /// <summary>
        /// 独占を達成するか判定
        /// </summary>
        private bool WouldAchieveMonopoly(Player player, Property property)
        {
            var station = property.Location;
            var ownedCount = station.Properties.Count(p => p.Owner == player);
            var totalCount = station.Properties.Count;

            return ownedCount + 1 == totalCount;
        }

        /// <summary>
        /// 独占を崩すか判定
        /// </summary>
        private bool WouldBreakMonopoly(Player player, Property property)
        {
            var station = property.Location;
            return station.IsMonopolizedBy(player);
        }

        /// <summary>
        /// デフォルト戦略を作成
        /// </summary>
        private IAIStrategy CreateDefaultStrategy(ComDifficulty difficulty)
        {
            return difficulty switch
            {
                ComDifficulty.Weak => new ConservativeAIStrategy(),
                ComDifficulty.Strong => new AggressiveAIStrategy(),
                _ => new BalancedAIStrategy()
            };
        }

        #endregion
    }
}