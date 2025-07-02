using System;
using System.Collections.Generic;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// カードエンティティ
    /// </summary>
    public class Card
    {
        /// <summary>
        /// カードID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// カード名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// カードの種類
        /// </summary>
        public CardType Type { get; set; }

        /// <summary>
        /// レア度
        /// </summary>
        public CardRarity Rarity { get; set; }

        /// <summary>
        /// 使用可能回数
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// 最大使用回数
        /// </summary>
        public int MaxUsageCount { get; set; }

        /// <summary>
        /// カード効果
        /// </summary>
        public ICardEffect? Effect { get; set; }

        /// <summary>
        /// 行動後使用可能かどうか
        /// </summary>
        public bool CanUseAfterAction { get; set; }

        /// <summary>
        /// カードの説明文
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 基本価格（カード売り場での価格）
        /// </summary>
        public Money BasePrice { get; set; }

        /// <summary>
        /// アイコンパス（UI表示用）
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// 使用制限（特定の条件下でのみ使用可能）
        /// </summary>
        public CardUsageRestriction Restriction { get; set; }

        /// <summary>
        /// カード効果のパラメータ
        /// </summary>
        public Dictionary<string, object> EffectParameters { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Card()
        {
            EffectParameters = new Dictionary<string, object>();
            Restriction = CardUsageRestriction.None;
            UsageCount = 1;
            MaxUsageCount = 1;
        }

        /// <summary>
        /// カードを作成（ファクトリメソッド）
        /// </summary>
        public static Card Create(int id, string name, CardType type, CardRarity rarity, long price)
        {
            return new Card
            {
                Id = id,
                Name = name,
                Type = type,
                Rarity = rarity,
                BasePrice = new Money(price),
                UsageCount = 1,
                MaxUsageCount = 1
            };
        }

        /// <summary>
        /// 周遊カードを作成
        /// </summary>
        public static Card CreateMultiUse(int id, string name, CardType type, CardRarity rarity, long price, int usageCount)
        {
            return new Card
            {
                Id = id,
                Name = name,
                Type = type,
                Rarity = rarity,
                BasePrice = new Money(price),
                UsageCount = usageCount,
                MaxUsageCount = usageCount
            };
        }

        /// <summary>
        /// カードが使用可能かチェック
        /// </summary>
        public bool CanUse(Player player, GameState gameState)
        {
            // 使用回数チェック
            if (UsageCount <= 0)
                return false;

            // プレイヤー状態チェック
            if (player.Status == PlayerStatus.Sealed && Type != CardType.Defense)
                return false;

            // 使用制限チェック
            switch (Restriction)
            {
                case CardUsageRestriction.BeforeDecember:
                    if (gameState.CurrentMonth == 12)
                        return false;
                    break;
                case CardUsageRestriction.AfterYear16:
                    if (gameState.CurrentYear < 16)
                        return false;
                    break;
                case CardUsageRestriction.NearDestination:
                    // 目的地から10マス以内でのみ使用可能
                    // TODO: 距離計算の実装
                    break;
            }

            return true;
        }

        /// <summary>
        /// レア度に応じた色を取得（UI表示用）
        /// </summary>
        public string GetRarityColor()
        {
            return Rarity switch
            {
                CardRarity.C => "#C0C0C0",    // Silver
                CardRarity.B => "#87CEEB",    // SkyBlue
                CardRarity.A => "#FFD700",    // Gold
                CardRarity.S => "#FF69B4",    // HotPink
                CardRarity.SS => "#FF1493",   // DeepPink
                _ => "#808080"                // Gray
            };
        }

        /// <summary>
        /// 使用回数テキストを取得
        /// </summary>
        public string GetUsageText()
        {
            if (MaxUsageCount == 1)
                return "1回";
            else
                return $"{UsageCount}/{MaxUsageCount}回";
        }

        /// <summary>
        /// カードのコピーを作成（ダビングカード用）
        /// </summary>
        public Card Clone()
        {
            return new Card
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Rarity = this.Rarity,
                UsageCount = this.MaxUsageCount, // 新品状態でコピー
                MaxUsageCount = this.MaxUsageCount,
                Effect = this.Effect,
                CanUseAfterAction = this.CanUseAfterAction,
                Description = this.Description,
                BasePrice = this.BasePrice,
                IconPath = this.IconPath,
                Restriction = this.Restriction,
                EffectParameters = new Dictionary<string, object>(this.EffectParameters)
            };
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Type}) [{Rarity}] - {GetUsageText()}";
        }
    }

    /// <summary>
    /// カード使用制限
    /// </summary>
    public enum CardUsageRestriction
    {
        None,              // 制限なし
        BeforeDecember,    // 12月になると消滅（シンデレラカード）
        AfterYear16,       // 16年目以降のみ（カードバンクカード）
        NearDestination,   // 目的地付近でのみ
        NoDebt,           // 借金がない時のみ
        HasDebt           // 借金がある時のみ
    }

    /// <summary>
    /// カードマスターデータ
    /// </summary>
    public static class CardMaster
    {
        private static readonly Dictionary<int, Func<Card>> _cardFactories = new()
        {
            // 移動系カード
            { 1, () => CreateMovementCard(1, "急行カード", CardRarity.C, 40000000, 2) },
            { 2, () => CreateMovementCard(2, "特急カード", CardRarity.B, 80000000, 3) },
            { 3, () => CreateMultiUseMovementCard(3, "急行周遊カード", CardRarity.B, 120000000, 2, 8) },
            { 4, () => CreateMultiUseMovementCard(4, "特急周遊カード", CardRarity.A, 240000000, 3, 8) },
            { 5, () => CreateMovementCard(5, "のぞみカード", CardRarity.A, 300000000, 6) },
            { 6, () => CreateMultiUseMovementCard(6, "のぞみ周遊カード", CardRarity.S, 600000000, 6, 10) },
            { 7, () => CreateMovementCard(7, "リニアカード", CardRarity.S, 500000000, 8) },
            { 8, () => CreateMultiUseMovementCard(8, "リニア周遊カード", CardRarity.SS, 1000000000, 8, 12) },

            // 便利系カード
            { 20, () => CreateUtilityCard(20, "ダビングカード", CardRarity.S, 300000000, "Duplicate") },
            { 21, () => CreateUtilityCard(21, "シンデレラカード", CardRarity.A, 200000000, "Cinderella", CardUsageRestriction.BeforeDecember) },
            { 22, () => CreateUtilityCard(22, "ゴールドカード", CardRarity.A, 250000000, "Gold") },
            { 23, () => CreateUtilityCard(23, "刀狩りカード", CardRarity.A, 200000000, "StealCard") },
            { 24, () => CreateUtilityCard(24, "カードバンクカード", CardRarity.B, 100000000, "CardBank", CardUsageRestriction.AfterYear16) },
            { 25, () => CreateUtilityCard(25, "ブックマークカード", CardRarity.B, 150000000, "Bookmark") },

            // 攻撃系カード
            { 40, () => CreateAttackCard(40, "牛歩カード", CardRarity.B, 100000000, "Cow", 88) },
            { 41, () => CreateAttackCard(41, "豪速球カード", CardRarity.A, 200000000, "FastBall", 88) },
            { 42, () => CreateAttackCard(42, "ふういんカード", CardRarity.B, 120000000, "Seal", 90) },
            { 43, () => CreateAttackCard(43, "絶不調カード", CardRarity.B, 150000000, "Unlucky", 85) },
        };

        /// <summary>
        /// IDからカードを生成
        /// </summary>
        public static Card? CreateCard(int id)
        {
            return _cardFactories.TryGetValue(id, out var factory) ? factory() : null;
        }

        /// <summary>
        /// 移動系カードを作成
        /// </summary>
        private static Card CreateMovementCard(int id, string name, CardRarity rarity, long price, int diceCount)
        {
            var card = Card.Create(id, name, CardType.Movement, rarity, price);
            card.EffectParameters["DiceCount"] = diceCount;
            card.Description = $"サイコロを{diceCount}個振って進めます。";
            return card;
        }

        /// <summary>
        /// 周遊移動カードを作成
        /// </summary>
        private static Card CreateMultiUseMovementCard(int id, string name, CardRarity rarity, long price, int diceCount, int usageCount)
        {
            var card = Card.CreateMultiUse(id, name, CardType.Movement, rarity, price, usageCount);
            card.EffectParameters["DiceCount"] = diceCount;
            card.Description = $"サイコロを{diceCount}個振って進めます。（{usageCount}回使用可能）";
            return card;
        }

        /// <summary>
        /// 便利系カードを作成
        /// </summary>
        private static Card CreateUtilityCard(int id, string name, CardRarity rarity, long price, string effectType,
            CardUsageRestriction restriction = CardUsageRestriction.None)
        {
            var card = Card.Create(id, name, CardType.Convenience, rarity, price);
            card.EffectParameters["EffectType"] = effectType;
            card.Restriction = restriction;
            card.CanUseAfterAction = effectType == "Duplicate"; // ダビングカードは行動後使用可能

            card.Description = effectType switch
            {
                "Duplicate" => "手持ちのカードを1枚選んで複製し、すぐに行動できます。",
                "Cinderella" => "好きな物件を1件無料で手に入れます。（12月になると消滅）",
                "Gold" => "今いる駅の全物件を10%の価格で購入できます。",
                "StealCard" => "他のプレイヤーからカードを1枚奪います。",
                "CardBank" => "カードバンクにアクセスできます。（16年目以降）",
                "Bookmark" => "指定した駅にブックマークを設定し、いつでも移動できるようになります。",
                _ => ""
            };

            return card;
        }

        /// <summary>
        /// 攻撃系カードを作成
        /// </summary>
        private static Card CreateAttackCard(int id, string name, CardRarity rarity, long price, string effectType, int successRate)
        {
            var card = Card.Create(id, name, CardType.Attack, rarity, price);
            card.EffectParameters["EffectType"] = effectType;
            card.EffectParameters["SuccessRate"] = successRate;

            card.Description = effectType switch
            {
                "Cow" => $"相手を牛歩状態にします。（成功率{successRate}%）",
                "FastBall" => $"全員のカードを2〜8枚破壊します。（成功率{successRate}%）",
                "Seal" => $"相手を数ターン封印状態にします。（成功率{successRate}%）",
                "Unlucky" => $"相手を6ヶ月間絶不調にします。（成功率{successRate}%）",
                _ => ""
            };

            return card;
        }
    }
}