using System;
using System.Collections.Generic;
using System.Linq;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// サイコロの結果を表す値オブジェクト
    /// </summary>
    public class DiceResult
    {
        /// <summary>
        /// 各サイコロの値
        /// </summary>
        public List<int> Values { get; }

        /// <summary>
        /// 合計値
        /// </summary>
        public int Total => Values.Sum();

        /// <summary>
        /// サイコロの数
        /// </summary>
        public int DiceCount => Values.Count;

        /// <summary>
        /// 特殊なサイコロかどうか
        /// </summary>
        public bool IsSpecial { get; }

        /// <summary>
        /// ゾロ目かどうか
        /// </summary>
        public bool IsDouble => Values.Count >= 2 && Values.All(v => v == Values[0]);

        /// <summary>
        /// 最大値
        /// </summary>
        public int Max => Values.Count > 0 ? Values.Max() : 0;

        /// <summary>
        /// 最小値
        /// </summary>
        public int Min => Values.Count > 0 ? Values.Min() : 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="values">サイコロの値のリスト</param>
        /// <param name="isSpecial">特殊サイコロかどうか</param>
        public DiceResult(List<int> values, bool isSpecial = false)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("サイコロの値が指定されていません。", nameof(values));

            if (values.Any(v => v < 1 || v > 6))
                throw new ArgumentException("サイコロの値は1〜6の範囲である必要があります。", nameof(values));

            Values = new List<int>(values);
            IsSpecial = isSpecial;
        }

        /// <summary>
        /// 単一のサイコロ結果を作成
        /// </summary>
        /// <param name="value">サイコロの値</param>
        /// <returns>DiceResult</returns>
        public static DiceResult Single(int value)
        {
            return new DiceResult(new List<int> { value });
        }

        /// <summary>
        /// 複数のサイコロ結果を作成
        /// </summary>
        /// <param name="diceCount">サイコロの数</param>
        /// <param name="random">乱数生成器</param>
        /// <returns>DiceResult</returns>
        public static DiceResult Roll(int diceCount, Random? random = null)
        {
            random ??= new Random();
            var values = new List<int>();

            for (int i = 0; i < diceCount; i++)
            {
                values.Add(random.Next(1, 7));
            }

            return new DiceResult(values);
        }

        /// <summary>
        /// 特定の値で固定されたサイコロ結果を作成（デバッグ用）
        /// </summary>
        /// <param name="diceCount">サイコロの数</param>
        /// <param name="fixedValue">固定値</param>
        /// <returns>DiceResult</returns>
        public static DiceResult Fixed(int diceCount, int fixedValue)
        {
            var values = Enumerable.Repeat(fixedValue, diceCount).ToList();
            return new DiceResult(values);
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            var result = string.Join(", ", Values);
            var special = IsSpecial ? " (特殊)" : "";
            var doubleBonus = IsDouble ? " ゾロ目！" : "";
            return $"[{result}] = {Total}{special}{doubleBonus}";
        }

        /// <summary>
        /// 短縮形式の文字列表現
        /// </summary>
        public string ToShortString()
        {
            return $"{Total}";
        }
    }
}