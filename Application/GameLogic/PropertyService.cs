using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.Services
{
    /// <summary>
    /// 物件サービスのインターフェース
    /// </summary>
    public interface IPropertyService
    {
        /// <summary>
        /// 物件を購入
        /// </summary>
        Task<PropertyTransactionResult> PurchasePropertyAsync(Player player, Property property);

        /// <summary>
        /// 複数の物件を購入
        /// </summary>
        Task<PropertyTransactionResult> PurchasePropertiesAsync(Player player, List<Property> properties);

        /// <summary>
        /// 物件を売却
        /// </summary>
        Task<PropertyTransactionResult> SellPropertyAsync(Player player, Property property);

        /// <summary>
        /// 独占チェック
        /// </summary>
        bool CheckMonopoly(Player player, Station station);

        /// <summary>
        /// 物件収益を計算
        /// </summary>
        Money CalculateIncome(Player player);

        /// <summary>
        /// 物件価値を更新
        /// </summary>
        void UpdatePropertyValues(PropertyMarket market);

        /// <summary>
        /// プレイヤーの物件統計を取得
        /// </summary>
        PropertyStatistics GetPropertyStatistics(Player player);
    }

    /// <summary>
    /// 物件サービスの実装
    /// </summary>
    public class PropertyService : IPropertyService
    {
        /// <summary>
        /// 物件購入イベント
        /// </summary>
        public event EventHandler<PropertyTransactionEventArgs>? PropertyPurchased;

        /// <summary>
        /// 物件売却イベント
        /// </summary>
        public event EventHandler<PropertyTransactionEventArgs>? PropertySold;

        /// <summary>
        /// 独占達成イベント
        /// </summary>
        public event EventHandler<MonopolyAchievedEventArgs>? MonopolyAchieved;

        /// <summary>
        /// 物件を購入
        /// </summary>
        public async Task<PropertyTransactionResult> PurchasePropertyAsync(Player player, Property property)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var result = new PropertyTransactionResult();

            // 購入可能チェック
            if (property.Owner != null)
            {
                result.Success = false;
                result.Message = $"{property.Name}は既に{property.Owner.Name}が所有しています。";
                return result;
            }

            // 資金チェック
            if (player.CurrentMoney < property.CurrentPrice)
            {
                result.Success = false;
                result.Message = "所持金が不足しています。";
                result.RequiredMoney = property.CurrentPrice;
                result.ShortAmount = property.CurrentPrice - player.CurrentMoney;
                return result;
            }

            // 購入処理
            player.Pay(property.CurrentPrice);
            property.Owner = player;
            player.OwnedProperties.Add(property);

            result.Success = true;
            result.TransactionType = TransactionType.Purchase;
            result.Properties.Add(property);
            result.TotalAmount = property.CurrentPrice;
            result.Message = $"{property.Name}を購入しました！";

            // イベント発火
            PropertyPurchased?.Invoke(this, new PropertyTransactionEventArgs
            {
                Player = player,
                Property = property,
                Amount = property.CurrentPrice
            });

            // 独占チェック
            if (CheckMonopoly(player, property.Location))
            {
                result.AchievedMonopoly = true;
                result.Message += $"\n{property.Location.Name}を独占しました！";

                MonopolyAchieved?.Invoke(this, new MonopolyAchievedEventArgs
                {
                    Player = player,
                    Station = property.Location
                });
            }

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// 複数の物件を購入
        /// </summary>
        public async Task<PropertyTransactionResult> PurchasePropertiesAsync(Player player, List<Property> properties)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (properties == null || properties.Count == 0)
                throw new ArgumentException("購入する物件が指定されていません。", nameof(properties));

            var result = new PropertyTransactionResult
            {
                TransactionType = TransactionType.Purchase
            };

            // 購入可能チェック
            var availableProperties = properties.Where(p => p.Owner == null).ToList();
            if (availableProperties.Count == 0)
            {
                result.Success = false;
                result.Message = "購入可能な物件がありません。";
                return result;
            }

            // 合計金額計算
            var totalPrice = availableProperties.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice);

            // 資金チェック
            if (player.CurrentMoney < totalPrice)
            {
                result.Success = false;
                result.Message = "所持金が不足しています。";
                result.RequiredMoney = totalPrice;
                result.ShortAmount = totalPrice - player.CurrentMoney;
                return result;
            }

            // 購入処理
            foreach (var property in availableProperties)
            {
                player.Pay(property.CurrentPrice);
                property.Owner = player;
                player.OwnedProperties.Add(property);
                result.Properties.Add(property);

                PropertyPurchased?.Invoke(this, new PropertyTransactionEventArgs
                {
                    Player = player,
                    Property = property,
                    Amount = property.CurrentPrice
                });
            }

            result.Success = true;
            result.TotalAmount = totalPrice;
            result.Message = $"{availableProperties.Count}件の物件を購入しました！";

            // 独占チェック（駅ごとに）
            var stationGroups = availableProperties.GroupBy(p => p.Location);
            foreach (var group in stationGroups)
            {
                if (CheckMonopoly(player, group.Key))
                {
                    result.AchievedMonopoly = true;
                    result.Message += $"\n{group.Key.Name}を独占しました！";

                    MonopolyAchieved?.Invoke(this, new MonopolyAchievedEventArgs
                    {
                        Player = player,
                        Station = group.Key
                    });
                }
            }

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// 物件を売却
        /// </summary>
        public async Task<PropertyTransactionResult> SellPropertyAsync(Player player, Property property)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var result = new PropertyTransactionResult
            {
                TransactionType = TransactionType.Sale
            };

            // 所有チェック
            if (property.Owner != player)
            {
                result.Success = false;
                result.Message = "この物件を所有していません。";
                return result;
            }

            // 売却価格計算（購入価格の70%）
            var sellPrice = property.GetSellPrice();

            // 売却処理
            property.Owner = null;
            player.OwnedProperties.Remove(property);
            player.Receive(sellPrice);

            result.Success = true;
            result.Properties.Add(property);
            result.TotalAmount = sellPrice;
            result.Message = $"{property.Name}を{sellPrice}で売却しました。";

            // イベント発火
            PropertySold?.Invoke(this, new PropertyTransactionEventArgs
            {
                Player = player,
                Property = property,
                Amount = sellPrice
            });

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// 独占チェック
        /// </summary>
        public bool CheckMonopoly(Player player, Station station)
        {
            if (player == null || station == null)
                return false;

            // 物件駅でない場合は独占不可
            if (station.Properties.Count == 0)
                return false;

            // 全ての物件を所有しているかチェック
            return station.Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// 物件収益を計算
        /// </summary>
        public Money CalculateIncome(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            Money totalIncome = Money.Zero;

            // 駅ごとにグループ化
            var stationGroups = player.OwnedProperties.GroupBy(p => p.Location);

            foreach (var group in stationGroups)
            {
                var station = group.Key;
                var isMonopoly = CheckMonopoly(player, station);

                foreach (var property in group)
                {
                    // 独占ボーナスを適用
                    property.HasMonopolyBonus = isMonopoly;
                    var income = property.CalculateIncome();
                    totalIncome += income;
                }
            }

            return totalIncome;
        }

        /// <summary>
        /// 物件価値を更新
        /// </summary>
        public void UpdatePropertyValues(PropertyMarket market)
        {
            if (market == null)
                throw new ArgumentNullException(nameof(market));

            // 全物件の価値を市場状況に応じて更新
            // （実際のゲームでは、各駅の物件リストを取得して更新する）
        }

        /// <summary>
        /// プレイヤーの物件統計を取得
        /// </summary>
        public PropertyStatistics GetPropertyStatistics(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var stats = new PropertyStatistics
            {
                TotalCount = player.OwnedProperties.Count,
                TotalValue = player.OwnedProperties.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice)
            };

            // カテゴリ別集計
            var categoryGroups = player.OwnedProperties.GroupBy(p => p.Category);
            foreach (var group in categoryGroups)
            {
                stats.CountByCategory[group.Key] = group.Count();
                stats.ValueByCategory[group.Key] = group.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice);
            }

            // 地域別集計
            var regionGroups = player.OwnedProperties.GroupBy(p => p.Location.Region);
            foreach (var group in regionGroups)
            {
                stats.CountByRegion[group.Key] = group.Count();
            }

            // 独占駅数
            var stationGroups = player.OwnedProperties.GroupBy(p => p.Location);
            stats.MonopolyStationCount = stationGroups.Count(g => CheckMonopoly(player, g.Key));

            // 最高額物件
            stats.MostExpensiveProperty = player.OwnedProperties.OrderByDescending(p => p.CurrentPrice).FirstOrDefault();

            return stats;
        }
    }

    /// <summary>
    /// 物件取引結果
    /// </summary>
    public class PropertyTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public List<Property> Properties { get; set; } = new List<Property>();
        public Money TotalAmount { get; set; }
        public Money? RequiredMoney { get; set; }
        public Money? ShortAmount { get; set; }
        public bool AchievedMonopoly { get; set; }
    }

    /// <summary>
    /// 取引タイプ
    /// </summary>
    public enum TransactionType
    {
        Purchase,
        Sale
    }

    /// <summary>
    /// 物件統計
    /// </summary>
    public class PropertyStatistics
    {
        public int TotalCount { get; set; }
        public Money TotalValue { get; set; }
        public Dictionary<PropertyCategory, int> CountByCategory { get; set; } = new Dictionary<PropertyCategory, int>();
        public Dictionary<PropertyCategory, Money> ValueByCategory { get; set; } = new Dictionary<PropertyCategory, Money>();
        public Dictionary<Region, int> CountByRegion { get; set; } = new Dictionary<Region, int>();
        public int MonopolyStationCount { get; set; }
        public Property? MostExpensiveProperty { get; set; }
    }

    /// <summary>
    /// 物件取引イベント引数
    /// </summary>
    public class PropertyTransactionEventArgs : EventArgs
    {
        public Player Player { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public Money Amount { get; set; }
    }

    /// <summary>
    /// 独占達成イベント引数
    /// </summary>
    public class MonopolyAchievedEventArgs : EventArgs
    {
        public Player Player { get; set; } = null!;
        public Station Station { get; set; } = null!;
    }
}