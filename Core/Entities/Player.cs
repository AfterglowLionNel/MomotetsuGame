using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// プレイヤーエンティティ
    /// </summary>
    public class Player : INotifyPropertyChanged
    {
        private Money _currentMoney;
        private Money _debt;
        private Station? _currentStation;
        private PlayerStatus _status;
        private int _rank;

        /// <summary>
        /// プレイヤーID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// 所持カードリスト
        /// </summary>
        public List<Card> Cards { get; set; }

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
        public Hero? AssignedHero { get; set; }

        /// <summary>
        /// 付着しているボンビー
        /// </summary>
        public Bonby? AttachedBonby { get; set; }

        /// <summary>
        /// 総資産（所持金＋物件評価額）
        /// </summary>
        public Money TotalAssets => CalculateTotalAssets();

        /// <summary>
        /// 人間プレイヤーかどうか
        /// </summary>
        public bool IsHuman { get; set; }

        /// <summary>
        /// 順位
        /// </summary>
        public int Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        /// <summary>
        /// プレイヤーカラー
        /// </summary>
        public PlayerColor Color { get; set; }

        /// <summary>
        /// 年間収益（決算時に設定）
        /// </summary>
        public Money YearlyIncome { get; set; }

        /// <summary>
        /// カード所持上限
        /// </summary>
        public int MaxCardCount => 8;

        /// <summary>
        /// ステータス異常の残りターン数
        /// </summary>
        public int StatusDuration { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Player()
        {
            Id = Guid.NewGuid();
            OwnedProperties = new List<Property>();
            Cards = new List<Card>();
            CurrentMoney = new Money(100000000); // 初期資金1億円
            Debt = Money.Zero;
            Status = PlayerStatus.Normal;
            IsHuman = false;
        }

        /// <summary>
        /// プレイヤーを作成（ファクトリメソッド）
        /// </summary>
        public static Player Create(string name, bool isHuman, PlayerColor color)
        {
            return new Player
            {
                Name = name,
                IsHuman = isHuman,
                Color = color
            };
        }

        /// <summary>
        /// 総資産を計算
        /// </summary>
        private Money CalculateTotalAssets()
        {
            var propertyValue = OwnedProperties
                .Aggregate(Money.Zero, (total, prop) => total + prop.CurrentPrice);

            return CurrentMoney + propertyValue - Debt;
        }

        /// <summary>
        /// お金を支払う（不足時は借金）
        /// </summary>
        public bool Pay(Money amount)
        {
            if (CurrentMoney >= amount)
            {
                CurrentMoney -= amount;
                return true;
            }
            else
            {
                var shortage = amount - CurrentMoney;
                CurrentMoney = Money.Zero;
                Debt += shortage;
                return false; // 借金が発生
            }
        }

        /// <summary>
        /// お金を受け取る（借金返済優先）
        /// </summary>
        public void Receive(Money amount)
        {
            if (Debt > Money.Zero)
            {
                if (amount >= Debt)
                {
                    amount -= Debt;
                    Debt = Money.Zero;
                }
                else
                {
                    Debt -= amount;
                    amount = Money.Zero;
                }
            }

            CurrentMoney += amount;
        }

        /// <summary>
        /// カードを追加（上限チェック付き）
        /// </summary>
        public bool AddCard(Card card)
        {
            if (Cards.Count >= MaxCardCount)
                return false;

            Cards.Add(card);
            return true;
        }

        /// <summary>
        /// カードを使用
        /// </summary>
        public void UseCard(Card card)
        {
            card.UsageCount--;
            if (card.UsageCount <= 0)
            {
                Cards.Remove(card);
            }
        }

        /// <summary>
        /// ステータス異常を設定
        /// </summary>
        public void SetStatus(PlayerStatus status, int duration)
        {
            Status = status;
            StatusDuration = duration;
        }

        /// <summary>
        /// ターン終了時の処理
        /// </summary>
        public void ProcessTurnEnd()
        {
            // ステータス異常の期間を減らす
            if (StatusDuration > 0)
            {
                StatusDuration--;
                if (StatusDuration == 0)
                {
                    Status = PlayerStatus.Normal;
                }
            }
        }

        /// <summary>
        /// プレイヤーカラーの16進数表現を取得
        /// </summary>
        public string GetColorHex()
        {
            return Color switch
            {
                PlayerColor.Blue => "#0000FF",
                PlayerColor.Red => "#FF0000",
                PlayerColor.Green => "#00FF00",
                PlayerColor.Yellow => "#FFFF00",
                PlayerColor.Purple => "#800080",
                PlayerColor.Orange => "#FFA500",
                PlayerColor.Pink => "#FFC0CB",
                PlayerColor.Cyan => "#00FFFF",
                _ => "#808080"
            };
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{Name} - 総資産: {TotalAssets}, 順位: {Rank}位";
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
    /// ヒーロー（歴史上の偉人）
    /// </summary>
    public class Hero
    {
        /// <summary>
        /// ヒーローID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ヒーロー名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 獲得条件の説明
        /// </summary>
        public string AcquisitionCondition { get; set; } = string.Empty;

        /// <summary>
        /// 効果の説明
        /// </summary>
        public string EffectDescription { get; set; } = string.Empty;

        /// <summary>
        /// アイコンパス
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// 効果を適用するメソッド（後で詳細実装）
        /// </summary>
        public virtual void ApplyEffect(Player player, GameState gameState)
        {
            // 各ヒーローごとの効果を実装
        }
    }

    /// <summary>
    /// ボンビー
    /// </summary>
    public class Bonby
    {
        /// <summary>
        /// ボンビーの種類
        /// </summary>
        public BonbyType Type { get; set; }

        /// <summary>
        /// ボンビーの名前
        /// </summary>
        public string Name => GetBonbyName();

        /// <summary>
        /// アイコンパス
        /// </summary>
        public string IconPath => GetIconPath();

        /// <summary>
        /// 悪行を実行
        /// </summary>
        public virtual void ExecuteMischief(Player player, GameState gameState)
        {
            // ボンビーの種類に応じた悪行を実装
        }

        /// <summary>
        /// ボンビーの名前を取得
        /// </summary>
        private string GetBonbyName()
        {
            return Type switch
            {
                BonbyType.Mini => "ミニボンビー",
                BonbyType.Normal => "ボンビー",
                BonbyType.King => "キングボンビー",
                BonbyType.Pokon => "ポコン",
                BonbyType.BonbyrasAlien => "ボンビラス星",
                _ => "ボンビー"
            };
        }

        /// <summary>
        /// アイコンパスを取得
        /// </summary>
        private string GetIconPath()
        {
            return $"/Resources/Images/Bonby/{Type}.png";
        }
    }

    /// <summary>
    /// ボンビーの種類
    /// </summary>
    public enum BonbyType
    {
        Mini,           // ミニボンビー
        Normal,         // ボンビー
        King,           // キングボンビー
        Pokon,          // ポコン
        BonbyrasAlien   // ボンビラス星
    }
}