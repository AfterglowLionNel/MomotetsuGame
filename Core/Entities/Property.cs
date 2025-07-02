using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// 物件エンティティ
    /// </summary>
    public class Property : INotifyPropertyChanged
    {
        private Player? _owner;
        private Money _currentPrice;
        private decimal _incomeRate;
        private int _upgradeLevel;

        /// <summary>
        /// 物件ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 物件名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 物件カテゴリ
        /// </summary>
        public PropertyCategory Category { get; set; }

        /// <summary>
        /// 基本価格（初期価格）
        /// </summary>
        public Money BasePrice { get; set; }

        /// <summary>
        /// 現在価格
        /// </summary>
        public Money CurrentPrice
        {
            get => _currentPrice;
            set => SetProperty(ref _currentPrice, value);
        }

        /// <summary>
        /// 収益率（年間収益 = 現在価格 × 収益率）
        /// </summary>
        public decimal IncomeRate
        {
            get => _incomeRate;
            set => SetProperty(ref _incomeRate, value);
        }

        /// <summary>
        /// 所有者
        /// </summary>
        public Player? Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        /// <summary>
        /// 物件がある駅
        /// </summary>
        public Station Location { get; set; } = null!;

        /// <summary>
        /// アップグレードレベル（0〜3）
        /// </summary>
        public int UpgradeLevel
        {
            get => _upgradeLevel;
            set => SetProperty(ref _upgradeLevel, Math.Clamp(value, 0, 3));
        }

        /// <summary>
        /// 物件の説明
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// アイコンパス（UI表示用）
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// 特別イベント対象かどうか
        /// </summary>
        public bool IsSpecialEvent { get; set; }

        /// <summary>
        /// 独占ボーナスが適用されているか
        /// </summary>
        public bool HasMonopolyBonus { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Property()
        {
            Id = Guid.NewGuid();
            UpgradeLevel = 0;
        }

        /// <summary>
        /// 物件を作成（ファクトリメソッド）
        /// </summary>
        public static Property Create(string name, PropertyCategory category, long basePrice, decimal incomeRate)
        {
            return new Property
            {
                Name = name,
                Category = category,
                BasePrice = new Money(basePrice),
                CurrentPrice = new Money(basePrice),
                IncomeRate = incomeRate
            };
        }

        /// <summary>
        /// 年間収益を計算
        /// </summary>
        public Money CalculateIncome()
        {
            var baseIncome = CurrentPrice * IncomeRate;

            // アップグレードボーナス（レベル毎に10%増加）
            var upgradeBonus = 1.0m + (UpgradeLevel * 0.1m);

            // 独占ボーナス（2倍）
            var monopolyBonus = HasMonopolyBonus ? 2.0m : 1.0m;

            // カテゴリ別の季節ボーナス（後で実装予定）
            var seasonalBonus = 1.0m;

            return baseIncome * upgradeBonus * monopolyBonus * seasonalBonus;
        }

        /// <summary>
        /// 物件価値を更新（市場変動）
        /// </summary>
        public void UpdatePrice(decimal changeRate)
        {
            var newPrice = CurrentPrice * (1 + changeRate);

            // 最低価格は基本価格の50%
            var minPrice = BasePrice * 0.5m;

            // 最高価格は基本価格の300%
            var maxPrice = BasePrice * 3.0m;

            CurrentPrice = new Money(Math.Clamp(newPrice.Value, minPrice.Value, maxPrice.Value));
        }

        /// <summary>
        /// 物件をアップグレード
        /// </summary>
        public bool TryUpgrade(out Money cost)
        {
            cost = Money.Zero;

            if (UpgradeLevel >= 3)
                return false;

            // アップグレード費用は現在価格の50%
            cost = CurrentPrice * 0.5m;

            UpgradeLevel++;

            // アップグレードにより価値も上昇
            CurrentPrice = CurrentPrice * 1.2m;

            return true;
        }

        /// <summary>
        /// カテゴリに応じた基本収益率を取得
        /// </summary>
        public static decimal GetBasicIncomeRate(PropertyCategory category)
        {
            return category switch
            {
                PropertyCategory.Agriculture => 0.06m,  // 6%
                PropertyCategory.Fishery => 0.08m,      // 8%
                PropertyCategory.Commerce => 0.10m,     // 10%
                PropertyCategory.Industry => 0.12m,     // 12%
                PropertyCategory.Tourism => 0.15m,      // 15%
                _ => 0.05m
            };
        }

        /// <summary>
        /// カテゴリに応じた色を取得（UI表示用）
        /// </summary>
        public string GetCategoryColor()
        {
            return Category switch
            {
                PropertyCategory.Agriculture => "#228B22",  // ForestGreen
                PropertyCategory.Fishery => "#4682B4",      // SteelBlue
                PropertyCategory.Commerce => "#FFD700",     // Gold
                PropertyCategory.Industry => "#708090",     // SlateGray
                PropertyCategory.Tourism => "#FF6347",      // Tomato
                _ => "#808080"                              // Gray
            };
        }

        /// <summary>
        /// 売却価格を計算（購入価格の70%）
        /// </summary>
        public Money GetSellPrice()
        {
            return CurrentPrice * 0.7m;
        }

        /// <summary>
        /// 物件情報の文字列表現
        /// </summary>
        public override string ToString()
        {
            var ownerName = Owner?.Name ?? "なし";
            return $"{Name} ({Category}) - 価格: {CurrentPrice}, 収益率: {IncomeRate:P0}, 所有者: {ownerName}";
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// 物件マーケット情報
    /// </summary>
    public class PropertyMarket
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// 市場の景気状態
        /// </summary>
        public MarketCondition Condition { get; set; }

        /// <summary>
        /// カテゴリ別の市場トレンド
        /// </summary>
        public Dictionary<PropertyCategory, decimal> CategoryTrends { get; set; }

        public PropertyMarket()
        {
            Condition = MarketCondition.Normal;
            CategoryTrends = new Dictionary<PropertyCategory, decimal>();
            InitializeTrends();
        }

        /// <summary>
        /// 市場トレンドを初期化
        /// </summary>
        private void InitializeTrends()
        {
            foreach (PropertyCategory category in Enum.GetValues<PropertyCategory>())
            {
                CategoryTrends[category] = 0m;
            }
        }

        /// <summary>
        /// 月次で市場を更新
        /// </summary>
        public void UpdateMarket()
        {
            // 景気状態をランダムに変更（10%の確率）
            if (_random.Next(100) < 10)
            {
                var conditions = Enum.GetValues<MarketCondition>();
                Condition = conditions[_random.Next(conditions.Length)];
            }

            // カテゴリ別トレンドを更新
            foreach (var category in Enum.GetValues<PropertyCategory>())
            {
                var change = (_random.NextDouble() - 0.5) * 0.1; // -5%〜+5%
                CategoryTrends[category] = (decimal)change;
            }
        }

        /// <summary>
        /// 物件の価格変動率を計算
        /// </summary>
        public decimal CalculatePriceChange(Property property)
        {
            var baseChange = Condition switch
            {
                MarketCondition.Boom => 0.05m,       // +5%
                MarketCondition.Normal => 0m,
                MarketCondition.Recession => -0.05m, // -5%
                _ => 0m
            };

            var categoryChange = CategoryTrends.GetValueOrDefault(property.Category, 0m);

            return baseChange + categoryChange;
        }
    }

    /// <summary>
    /// 市場の景気状態
    /// </summary>
    public enum MarketCondition
    {
        Boom,      // 好景気
        Normal,    // 通常
        Recession  // 不景気
    }
}