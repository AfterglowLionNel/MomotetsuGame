using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// サイコロサービスの実装
    /// </summary>
    public class DiceService : IDiceService
    {
        private readonly Random _random;
        private readonly object _lock = new object();

        public DiceService()
        {
            _random = new Random();
        }

        /// <summary>
        /// 通常のサイコロを振る
        /// </summary>
        /// <param name="count">サイコロの数</param>
        /// <returns>サイコロの結果</returns>
        public DiceResult Roll(int count)
        {
            if (count <= 0)
                throw new ArgumentException("サイコロの数は1以上である必要があります。", nameof(count));

            var values = new List<int>();

            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    values.Add(_random.Next(1, 7)); // 1～6の値
                }
            }

            return new DiceResult(values);
        }

        /// <summary>
        /// 特殊サイコロを振る
        /// </summary>
        /// <param name="count">サイコロの数</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>サイコロの結果</returns>
        public DiceResult RollSpecial(int count, int min, int max)
        {
            if (count <= 0)
                throw new ArgumentException("サイコロの数は1以上である必要があります。", nameof(count));
            if (min < 1 || min > max || max > 6)
                throw new ArgumentException("最小値と最大値の範囲が不正です。");

            var values = new List<int>();

            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    values.Add(_random.Next(min, max + 1));
                }
            }

            return new DiceResult(values, isSpecial: true);
        }

        /// <summary>
        /// 固定値のサイコロ結果を作成（デバッグ/テスト用）
        /// </summary>
        public DiceResult CreateFixed(params int[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("値が指定されていません。", nameof(values));

            return new DiceResult(values.ToList());
        }

        /// <summary>
        /// 絶不調時のサイコロを振る（1～2のみ）
        /// </summary>
        public DiceResult RollUnlucky(int count)
        {
            return RollSpecial(count, 1, 2);
        }

        /// <summary>
        /// 牛歩状態のサイコロを振る（1固定）
        /// </summary>
        public DiceResult RollCow()
        {
            return new DiceResult(new List<int> { 1 });
        }

        /// <summary>
        /// サイコロアニメーション用のランダム値を生成
        /// </summary>
        public async Task<List<int>> GenerateAnimationValuesAsync(int diceCount, int frames = 30)
        {
            var values = new List<int>();

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    for (int i = 0; i < frames; i++)
                    {
                        values.Add(_random.Next(1, 7));
                    }
                }
            });

            return values;
        }
    }

    /// <summary>
    /// サイコロ関連のユーティリティ
    /// </summary>
    public static class DiceUtility
    {
        /// <summary>
        /// サイコロの目の期待値を計算
        /// </summary>
        public static double CalculateExpectedValue(int diceCount, int min = 1, int max = 6)
        {
            double singleDiceExpected = (min + max) / 2.0;
            return singleDiceExpected * diceCount;
        }

        /// <summary>
        /// 特定の値以上が出る確率を計算
        /// </summary>
        public static double CalculateProbability(int diceCount, int targetValue)
        {
            if (targetValue <= diceCount) return 1.0;
            if (targetValue > diceCount * 6) return 0.0;

            // 簡易計算（正確な計算は動的計画法が必要）
            double averageRoll = 3.5 * diceCount;
            double variance = 2.92 * diceCount; // 単一サイコロの分散は約2.92
            double standardDeviation = Math.Sqrt(variance);

            // 正規分布で近似
            double z = (targetValue - averageRoll) / standardDeviation;
            return 1 - NormalCDF(z);
        }

        /// <summary>
        /// 正規分布の累積分布関数（簡易版）
        /// </summary>
        private static double NormalCDF(double x)
        {
            // エラー関数による近似
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }
    }
}