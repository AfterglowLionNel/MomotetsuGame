using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.Services
{
    /// <summary>
    /// サイコロサービスのインターフェース
    /// </summary>
    public interface IDiceService
    {
        /// <summary>
        /// サイコロを振る
        /// </summary>
        /// <param name="diceCount">サイコロの数</param>
        /// <returns>サイコロの結果</returns>
        DiceResult Roll(int diceCount);

        /// <summary>
        /// プレイヤーの状態を考慮してサイコロを振る
        /// </summary>
        /// <param name="player">プレイヤー</param>
        /// <param name="baseDiceCount">基本のサイコロ数</param>
        /// <returns>サイコロの結果</returns>
        Task<DiceResult> RollForPlayerAsync(Player player, int baseDiceCount = 1);

        /// <summary>
        /// アニメーション付きでサイコロを振る
        /// </summary>
        /// <param name="diceCount">サイコロの数</param>
        /// <param name="onRolling">回転中のコールバック</param>
        /// <returns>サイコロの結果</returns>
        Task<DiceResult> RollWithAnimationAsync(int diceCount, Action<List<int>>? onRolling = null);
    }

    /// <summary>
    /// サイコロサービスの実装
    /// </summary>
    public class DiceService : IDiceService
    {
        private readonly Random _random;

        public DiceService()
        {
            _random = new Random();
        }

        public DiceService(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// サイコロを振る
        /// </summary>
        public DiceResult Roll(int diceCount)
        {
            if (diceCount <= 0)
                throw new ArgumentException("サイコロの数は1個以上である必要があります。", nameof(diceCount));

            var values = new List<int>();
            for (int i = 0; i < diceCount; i++)
            {
                values.Add(_random.Next(1, 7));
            }

            return new DiceResult(values);
        }

        /// <summary>
        /// プレイヤーの状態を考慮してサイコロを振る
        /// </summary>
        public async Task<DiceResult> RollForPlayerAsync(Player player, int baseDiceCount = 1)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            int actualDiceCount = baseDiceCount;

            // プレイヤーの状態による補正
            switch (player.Status)
            {
                case PlayerStatus.Cow:
                    // 牛歩状態は必ず1が出る
                    return new DiceResult(new List<int> { 1 });

                case PlayerStatus.Unlucky:
                    // 絶不調は1-2しか出ない
                    var unluckyValues = new List<int>();
                    for (int i = 0; i < actualDiceCount; i++)
                    {
                        unluckyValues.Add(_random.Next(1, 3));
                    }
                    return new DiceResult(unluckyValues);

                case PlayerStatus.SuperLucky:
                    // 絶好調は5-6しか出ない
                    var luckyValues = new List<int>();
                    for (int i = 0; i < actualDiceCount; i++)
                    {
                        luckyValues.Add(_random.Next(5, 7));
                    }
                    return new DiceResult(luckyValues, isSpecial: true);

                default:
                    // 通常状態
                    return await Task.Run(() => Roll(actualDiceCount));
            }
        }

        /// <summary>
        /// アニメーション付きでサイコロを振る
        /// </summary>
        public async Task<DiceResult> RollWithAnimationAsync(int diceCount, Action<List<int>>? onRolling = null)
        {
            const int animationFrames = 10;
            const int frameDelay = 100; // ミリ秒

            // アニメーション中の仮の値を表示
            for (int frame = 0; frame < animationFrames; frame++)
            {
                var tempValues = new List<int>();
                for (int i = 0; i < diceCount; i++)
                {
                    tempValues.Add(_random.Next(1, 7));
                }

                onRolling?.Invoke(tempValues);
                await Task.Delay(frameDelay);
            }

            // 最終的な結果を決定
            return Roll(diceCount);
        }

        /// <summary>
        /// 目的地補正を適用
        /// </summary>
        public DiceResult ApplyDestinationCorrection(DiceResult original, int distanceToDestination)
        {
            // 目的地が近い場合、ちょうど到着しやすくする
            if (distanceToDestination > 0 && distanceToDestination <= 6)
            {
                // 20%の確率で目的地ちょうどの目が出る
                if (_random.Next(100) < 20)
                {
                    return new DiceResult(new List<int> { distanceToDestination });
                }
            }

            return original;
        }

        /// <summary>
        /// イベントマスの補正を適用
        /// </summary>
        public DiceResult ApplyEventSquareCorrection(DiceResult original, List<int> eventSquareDistances)
        {
            // プラス駅やカード売り場が近い場合、到着しやすくする
            var total = original.Total;

            foreach (var distance in eventSquareDistances)
            {
                if (distance == total && _random.Next(100) < 30) // 30%の確率で補正
                {
                    return original; // そのまま使用
                }
            }

            // 10%の確率で±1の補正
            if (_random.Next(100) < 10)
            {
                var nearestEvent = eventSquareDistances.OrderBy(d => Math.Abs(d - total)).FirstOrDefault();
                if (nearestEvent > 0 && Math.Abs(nearestEvent - total) == 1)
                {
                    var adjustedValues = original.Values.ToList();
                    if (nearestEvent > total)
                        adjustedValues[0]++; // +1
                    else
                        adjustedValues[0]--; // -1

                    // 1-6の範囲内に収める
                    adjustedValues[0] = Math.Max(1, Math.Min(6, adjustedValues[0]));
                    return new DiceResult(adjustedValues);
                }
            }

            return original;
        }
    }

    /// <summary>
    /// デバッグ用の固定サイコロサービス
    /// </summary>
    public class FixedDiceService : IDiceService
    {
        private readonly Queue<int> _fixedValues;

        public FixedDiceService(params int[] values)
        {
            _fixedValues = new Queue<int>(values);
        }

        public DiceResult Roll(int diceCount)
        {
            var values = new List<int>();
            for (int i = 0; i < diceCount; i++)
            {
                if (_fixedValues.Count > 0)
                    values.Add(_fixedValues.Dequeue());
                else
                    values.Add(1); // デフォルト値
            }
            return new DiceResult(values);
        }

        public Task<DiceResult> RollForPlayerAsync(Player player, int baseDiceCount = 1)
        {
            return Task.FromResult(Roll(baseDiceCount));
        }

        public Task<DiceResult> RollWithAnimationAsync(int diceCount, Action<List<int>>? onRolling = null)
        {
            return Task.FromResult(Roll(diceCount));
        }
    }
}