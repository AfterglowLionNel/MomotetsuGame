using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// 物件管理サービス
    /// </summary>
    public class PropertyService
    {
        private readonly PropertyMarket _propertyMarket;
        private readonly Random _random;

        public PropertyService(PropertyMarket propertyMarket)
        {
            _propertyMarket = propertyMarket ?? throw new ArgumentNullException(nameof(propertyMarket));
            _random = new Random();
        }

        /// <summary>
        /// 物件を購入
        /// </summary>
        public async Task<bool> PurchasePropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            // 購入可能チェック
            if (!CanPurchase(player, property))
            {
                return false;
            }

            // 購入処理
            return await Task.Run(() => player.PurchaseProperty(property));
        }

        /// <summary>
        /// 複数物件を一括購入
        /// </summary>
        public async Task<PurchaseResult> PurchasePropertiesAsync(Player player, List<Property> properties)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var result = new PurchaseResult
            {
                RequestedProperties = new List<Property>(properties),
                PurchasedProperties = new List<Property>(),
                FailedProperties = new List<Property>(),
                TotalCost = Money.Zero
            };

            // 購入順序を最適化（安い順）
            var sortedProperties = properties.OrderBy(p => p.BasePrice.Value).ToList();

            foreach (var property in sortedProperties)
            {
                if (await PurchasePropertyAsync(player, property))
                {
                    result.PurchasedProperties.Add(property);
                    result.TotalCost += property.BasePrice;
                }
                else
                {
                    result.FailedProperties.Add(property);
                }
            }

            result.Success = result.PurchasedProperties.Count > 0;
            return result;
        }

        /// <summary>
        /// 物件を売却
        /// </summary>
        public async Task<bool> SellPropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            return await Task.Run(() => player.SellProperty(property));
        }

        /// <summary>
        /// 購入可能かチェック
        /// </summary>
        public bool CanPurchase(Player player, Property property)
        {
            // 所有者チェック
            if (property.Owner != null)
                return false;

            // 資金チェック
            if (player.CurrentMoney < property.BasePrice)
                return false;

            return true;
        }

        /// <summary>
        /// 独占チェック
        /// </summary>
        public bool CheckMonopoly(Player player, Station station)
        {
            if (player == null || station == null) return false;
            if (station.Properties.Count == 0) return false;

            return station.IsMonopolizedBy(player);
        }

        /// <summary>
        /// 駅の独占率を計算
        /// </summary>
        public double CalculateMonopolyRate(Player player, Station station)
        {
            if (player == null || station == null) return 0;
            if (station.Properties.Count == 0) return 0;

            var ownedCount = station.Properties.Count(p => p.Owner == player);
            return (double)ownedCount / station.Properties.Count;
        }

        /// <summary>
        /// 物件価値を更新（月次処理）
        /// </summary>
        public void UpdatePropertyValues()
        {
            _propertyMarket.UpdateMarket();

            // すべての物件の価値を更新
            foreach (var property in GetAllProperties())
            {
                var changeRate = _propertyMarket.CalculatePriceChange(property);
                property.UpdatePrice(changeRate);
            }
        }

        /// <summary>
        /// 特別イベントによる物件価値変動
        /// </summary>
        public void ApplySpecialEvent(PropertyEventType eventType, PropertyCategory? targetCategory = null)
        {
            var properties = GetAllProperties();

            if (targetCategory.HasValue)
            {
                properties = properties.Where(p => p.Category == targetCategory.Value).ToList();
            }

            foreach (var property in properties)
            {
                var changeRate = eventType switch
                {
                    PropertyEventType.Boom => 0.3m,          // +30%
                    PropertyEventType.Recession => -0.3m,    // -30%
                    PropertyEventType.Disaster => -0.5m,     // -50%
                    PropertyEventType.Development => 0.5m,   // +50%
                    _ => 0m
                };

                property.UpdatePrice(changeRate);
            }
        }

        /// <summary>
        /// 物件のアップグレード
        /// </summary>
        public async Task<UpgradeResult> UpgradePropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var result = new UpgradeResult
            {
                Property = property,
                Success = false
            };

            // 所有者チェック
            if (property.Owner != player)
            {
                result.Message = "自分の物件ではありません。";
                return result;
            }

            // アップグレード可能チェック
            if (property.TryUpgrade(out var cost))
            {
                // 資金チェック
                if (player.CurrentMoney >= cost)
                {
                    player.CurrentMoney -= cost;
                    result.Success = true;
                    result.Cost = cost;
                    result.Message = $"{property.Name}をアップグレードしました！";
                }
                else
                {
                    result.Message = "資金が不足しています。";
                }
            }
            else
            {
                result.Message = "これ以上アップグレードできません。";
            }

            return result;
        }

        /// <summary>
        /// 推奨物件を取得（AI/ヒント用）
        /// </summary>
        public List<Property> GetRecommendedProperties(Station station, Player player)
        {
            if (station == null || player == null) return new List<Property>();

            var availableProperties = station.GetAvailableProperties();
            var recommendations = new List<(Property property, double score)>();

            foreach (var property in availableProperties)
            {
                double score = CalculatePropertyScore(property, player, station);
                recommendations.Add((property, score));
            }

            return recommendations
                .OrderByDescending(r => r.score)
                .Select(r => r.property)
                .ToList();
        }

        /// <summary>
        /// 物件スコアを計算
        /// </summary>
        private double CalculatePropertyScore(Property property, Player player, Station station)
        {
            double score = 0;

            // 収益率スコア
            score += property.IncomeRate * 100;

            // 価格効率スコア
            var priceEfficiency = (double)property.IncomeRate / (property.BasePrice.Value / 1000000000.0);
            score += priceEfficiency * 50;

            // 独占可能性スコア
            var monopolyRate = CalculateMonopolyRate(player, station);
            if (monopolyRate > 0.5)
            {
                score += monopolyRate * 100;
            }

            // カテゴリボーナス
            score += property.Category switch
            {
                PropertyCategory.Tourism => 20,     // 観光物件は高評価
                PropertyCategory.Industry => 15,    // 工業物件も高評価
                PropertyCategory.Commerce => 10,    // 商業物件は中評価
                PropertyCategory.Fishery => 5,      // 水産物件は低評価
                PropertyCategory.Agriculture => 0,  // 農林物件は基準
                _ => 0
            };

            // 市場トレンドを考慮
            var marketTrend = _propertyMarket.CategoryTrends.GetValueOrDefault(property.Category, 0m);
            score += (double)marketTrend * 50;

            // 資金に対する購入可能性
            var affordabilityRatio = (double)player.CurrentMoney.Value / property.BasePrice.Value;
            if (affordabilityRatio < 2.0)
            {
                score -= 20; // 所持金の半分以上は慎重に
            }

            return score;
        }

        /// <summary>
        /// すべての物件を取得（内部用）
        /// </summary>
        private List<Property> GetAllProperties()
        {
            // 実際の実装では、StationNetworkから全駅の物件を取得
            // ここでは仮実装
            return new List<Property>();
        }
    }

    /// <summary>
    /// 物件購入結果
    /// </summary>
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public List<Property> RequestedProperties { get; set; } = new List<Property>();
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public List<Property> FailedProperties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; } = Money.Zero;
    }

    /// <summary>
    /// 物件アップグレード結果
    /// </summary>
    public class UpgradeResult
    {
        public bool Success { get; set; }
        public Property Property { get; set; } = null!;
        public Money Cost { get; set; } = Money.Zero;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 物件イベントタイプ
    /// </summary>
    public enum PropertyEventType
    {
        Boom,         // 好景気
        Recession,    // 不景気
        Disaster,     // 災害
        Development   // 開発
    }

    /// <summary>
    /// 物件評価サービス（AI用）
    /// </summary>
    public class PropertyEvaluator
    {
        /// <summary>
        /// 物件の投資価値を評価
        /// </summary>
        public double EvaluateInvestmentValue(Property property, Player player, GameState gameState)
        {
            double value = 0;

            // 基本的な収益性
            var annualIncome = property.CalculateIncome();
            var paybackPeriod = property.CurrentPrice.Value / (double)annualIncome.Value;
            value += 100 / paybackPeriod; // 回収期間が短いほど高評価

            // ゲーム残り期間を考慮
            var remainingYears = gameState.MaxYears - gameState.CurrentYear;
            if (paybackPeriod > remainingYears)
            {
                value *= 0.5; // 回収できない場合は評価を下げる
            }

            // 独占による収益倍増を考慮
            if (property.Location != null)
            {
                var monopolyPotential = CalculateMonopolyPotential(player, property.Location);
                value *= (1 + monopolyPotential);
            }

            // アップグレード可能性
            if (property.UpgradeLevel < 3)
            {
                value *= 1.2; // アップグレード余地があれば評価アップ
            }

            return value;
        }

        /// <summary>
        /// 独占可能性を計算
        /// </summary>
        private double CalculateMonopolyPotential(Player player, Station station)
        {
            var totalProperties = station.Properties.Count;
            var ownedProperties = station.Properties.Count(p => p.Owner == player);
            var availableProperties = station.Properties.Count(p => p.Owner == null);

            if (totalProperties == 0) return 0;

            // 既に独占している場合
            if (ownedProperties == totalProperties) return 1.0;

            // 独占可能な場合（他プレイヤーが所有していない）
            if (ownedProperties + availableProperties == totalProperties)
            {
                return (double)ownedProperties / totalProperties;
            }

            // 独占不可能な場合
            return 0;
        }

        /// <summary>
        /// 売却推奨度を評価
        /// </summary>
        public double EvaluateSellRecommendation(Property property, Player player, Money targetAmount)
        {
            double score = 0;

            // 売却価格と購入価格の比率
            var sellPrice = property.GetSellPrice();
            var lossRatio = 1.0 - ((double)sellPrice.Value / property.CurrentPrice.Value);
            score -= lossRatio * 100; // 損失が大きいほど低評価

            // 収益性が低い物件は売却推奨
            if (property.IncomeRate < 0.08m) // 8%未満
            {
                score += 20;
            }

            // 独占を崩す場合は非推奨
            if (property.HasMonopolyBonus)
            {
                score -= 50;
            }

            // 目標額に対する貢献度
            var contributionRatio = (double)sellPrice.Value / targetAmount.Value;
            score += contributionRatio * 30;

            return score;
        }
    }
}