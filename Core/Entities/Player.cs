using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using Newtonsoft.Json.Linq;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// プレイヤーエンティティ
    /// </summary>
    public class Player : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private Money _currentMoney;
        private Money _debt;
        private Station? _currentStation;
        private PlayerStatus _status;
        private Hero? _assignedHero;
        private Bonby? _attachedBonby;
        private int _rank;
        private bool _isHuman;

        /// <summary>
        /// プレイヤーID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 現在の所持金
        /// </summary>
        public Money CurrentMoney
        {
            get => _currentMoney;
            set => SetProperty(ref _currentMoney, value);
        }

        /// <summary>
        /// 借金額
        /// </summary>
        public Money Debt
        {
            get => _debt;
            set => SetProperty(ref _debt, value);
        }

        /// <summary>
        /// 現在いる駅
        /// </summary>
        public Station? CurrentStation
        {
            get => _currentStation;
            set => SetProperty(ref _currentStation, value);
        }

        /// <summary>
        /// 所有物件リスト
        /// </summary>
        public List<Property> OwnedProperties { get; set; }

        /// <summary>
        /// 所持カードコレクション
        /// </summary>
        public CardCollection Cards { get; set; }

        /// <summary>
        /// プレイヤーステータス
        /// </summary>
        public PlayerStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 割り当てられたヒーロー
        /// </summary>
        public Hero? AssignedHero
        {
            get => _assignedHero;
            set => SetProperty(ref _assignedHero, value);
        }

        /// <summary>
        /// 付着しているボンビー
        /// </summary>
        public Bonby? AttachedBonby
        {
            get => _attachedBonby;
            set => SetProperty(ref _attachedBonby, value);
        }

        /// <summary>
        /// 総資産（所持金＋物件評価額）
        /// </summary>
        public Money TotalAssets => CalculateTotalAssets();

        /// <summary>
        /// 人間プレイヤーかどうか
        /// </summary>
        public bool IsHuman
        {
            get => _isHuman;
            set => SetProperty(ref _isHuman, value);
        }

        /// <summary>
        /// 現在の順位
        /// </summary>
        public int Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        /// <summary>
        /// 色（UI表示用）
        /// </summary>
        public PlayerColor Color { get; set; }

        /// <summary>
        /// アイコンパス（UI表示用）
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// 年間収益（決算用）
        /// </summary>
        public Money YearlyIncome { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Player()
        {
            Id = Guid.NewGuid();
            OwnedProperties = new List<Property>();
            Cards = new CardCollection();
            CurrentMoney = new Money(100000000); // 初期資金1億円
            Debt = Money.Zero;
            Status = PlayerStatus.Normal;
            Rank = 1;
            IsHuman = false;
        }

        /// <summary>
        /// 総資産を計算
        /// </summary>
        private Money CalculateTotalAssets()
        {
            var propertyValue = OwnedProperties
                .Where(p => p != null)
                .Aggregate(Money.Zero, (sum, prop) => sum + prop.CurrentPrice);

            return CurrentMoney + propertyValue - Debt;
        }

        /// <summary>
        /// 物件を購入
        /// </summary>
        public bool PurchaseProperty(Property property)
        {
            if (property == null || property.Owner != null)
                return false;

            if (CurrentMoney < property.BasePrice)
                return false;

            CurrentMoney -= property.BasePrice;
            property.Owner = this;
            OwnedProperties.Add(property);

            OnPropertyChanged(nameof(TotalAssets));
            return true;
        }

        /// <summary>
        /// 物件を売却
        /// </summary>
        public bool SellProperty(Property property)
        {
            if (property == null || !OwnedProperties.Contains(property))
                return false;

            var sellPrice = property.CurrentPrice * 0.7m; // 売却額は70%
            CurrentMoney += sellPrice;
            property.Owner = null;
            OwnedProperties.Remove(property);

            OnPropertyChanged(nameof(TotalAssets));
            return true;
        }

        /// <summary>
        /// カードを使用
        /// </summary>
        public bool UseCard(Card card)
        {
            if (card == null || !Cards.Contains(card))
                return false;

            if (Status == PlayerStatus.Sealed)
                return false;

            return Cards.Use(card);
        }

        /// <summary>
        /// 借金をする
        /// </summary>
        public void Borrow(Money amount)
        {
            Debt += amount;
            CurrentMoney += amount;
        }

        /// <summary>
        /// 借金を返済
        /// </summary>
        public bool RepayDebt(Money amount)
        {
            if (CurrentMoney < amount)
                return false;

            var repayAmount = amount > Debt ? Debt : amount;
            Debt -= repayAmount;
            CurrentMoney -= repayAmount;

            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// カードコレクション
    /// </summary>
    public class CardCollection : List<Card>
    {
        public const int MaxCards = 8;
        public const int MaxBankCards = 16;

        /// <summary>
        /// 通常の所持カード
        /// </summary>
        public List<Card> HandCards => this.Take(MaxCards).ToList();

        /// <summary>
        /// カードバンクのカード
        /// </summary>
        public List<Card> BankCards => this.Skip(MaxCards).Take(MaxBankCards).ToList();

        /// <summary>
        /// カードを追加（所持上限チェック付き）
        /// </summary>
        public bool TryAdd(Card card)
        {
            if (Count >= MaxCards + MaxBankCards)
                return false;

            Add(card);
            return true;
        }

        /// <summary>
        /// カードを使用
        /// </summary>
        public bool Use(Card card)
        {
            if (!Contains(card))
                return false;

            card.UsageCount--;
            if (card.UsageCount <= 0)
            {
                Remove(card);
            }

            return true;
        }
    }

    /// <summary>
    /// ヒーロークラス（歴史上の偉人）
    /// </summary>
    public class Hero
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public HeroType Type { get; set; }
        public Station RequiredStation { get; set; } = null!;

        /// <summary>
        /// ヒーロー効果を適用
        /// </summary>
        public virtual void ApplyEffect(Player player, GameContext context)
        {
            // 派生クラスで実装
        }
    }

    /// <summary>
    /// ボンビークラス
    /// </summary>
    public class Bonby
    {
        public string Name { get; set; } = string.Empty;
        public BonbyType Type { get; set; }
        public int TurnsRemaining { get; set; }

        /// <summary>
        /// ボンビーの悪行を実行
        /// </summary>
        public virtual void ExecuteMischief(Player player, GameContext context)
        {
            // 派生クラスで実装
        }
    }

    /// <summary>
    /// ゲームコンテキスト（仮）
    /// </summary>
    public class GameContext
    {
        // 後で実装
    }
}