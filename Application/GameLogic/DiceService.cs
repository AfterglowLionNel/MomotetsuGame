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
    /// �T�C�R���T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IDiceService
    {
        /// <summary>
        /// �T�C�R����U��
        /// </summary>
        /// <param name="diceCount">�T�C�R���̐�</param>
        /// <returns>�T�C�R���̌���</returns>
        DiceResult Roll(int diceCount);

        /// <summary>
        /// �v���C���[�̏�Ԃ��l�����ăT�C�R����U��
        /// </summary>
        /// <param name="player">�v���C���[</param>
        /// <param name="baseDiceCount">��{�̃T�C�R����</param>
        /// <returns>�T�C�R���̌���</returns>
        Task<DiceResult> RollForPlayerAsync(Player player, int baseDiceCount = 1);

        /// <summary>
        /// �A�j���[�V�����t���ŃT�C�R����U��
        /// </summary>
        /// <param name="diceCount">�T�C�R���̐�</param>
        /// <param name="onRolling">��]���̃R�[���o�b�N</param>
        /// <returns>�T�C�R���̌���</returns>
        Task<DiceResult> RollWithAnimationAsync(int diceCount, Action<List<int>>? onRolling = null);
    }

    /// <summary>
    /// �T�C�R���T�[�r�X�̎���
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
        /// �T�C�R����U��
        /// </summary>
        public DiceResult Roll(int diceCount)
        {
            if (diceCount <= 0)
                throw new ArgumentException("�T�C�R���̐���1�ȏ�ł���K�v������܂��B", nameof(diceCount));

            var values = new List<int>();
            for (int i = 0; i < diceCount; i++)
            {
                values.Add(_random.Next(1, 7));
            }

            return new DiceResult(values);
        }

        /// <summary>
        /// �v���C���[�̏�Ԃ��l�����ăT�C�R����U��
        /// </summary>
        public async Task<DiceResult> RollForPlayerAsync(Player player, int baseDiceCount = 1)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            int actualDiceCount = baseDiceCount;

            // �v���C���[�̏�Ԃɂ��␳
            switch (player.Status)
            {
                case PlayerStatus.Cow:
                    // ������Ԃ͕K��1���o��
                    return new DiceResult(new List<int> { 1 });

                case PlayerStatus.Unlucky:
                    // ��s����1-2�����o�Ȃ�
                    var unluckyValues = new List<int>();
                    for (int i = 0; i < actualDiceCount; i++)
                    {
                        unluckyValues.Add(_random.Next(1, 3));
                    }
                    return new DiceResult(unluckyValues);

                case PlayerStatus.SuperLucky:
                    // ��D����5-6�����o�Ȃ�
                    var luckyValues = new List<int>();
                    for (int i = 0; i < actualDiceCount; i++)
                    {
                        luckyValues.Add(_random.Next(5, 7));
                    }
                    return new DiceResult(luckyValues, isSpecial: true);

                default:
                    // �ʏ���
                    return await Task.Run(() => Roll(actualDiceCount));
            }
        }

        /// <summary>
        /// �A�j���[�V�����t���ŃT�C�R����U��
        /// </summary>
        public async Task<DiceResult> RollWithAnimationAsync(int diceCount, Action<List<int>>? onRolling = null)
        {
            const int animationFrames = 10;
            const int frameDelay = 100; // �~���b

            // �A�j���[�V�������̉��̒l��\��
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

            // �ŏI�I�Ȍ��ʂ�����
            return Roll(diceCount);
        }

        /// <summary>
        /// �ړI�n�␳��K�p
        /// </summary>
        public DiceResult ApplyDestinationCorrection(DiceResult original, int distanceToDestination)
        {
            // �ړI�n���߂��ꍇ�A���傤�Ǔ������₷������
            if (distanceToDestination > 0 && distanceToDestination <= 6)
            {
                // 20%�̊m���ŖړI�n���傤�ǂ̖ڂ��o��
                if (_random.Next(100) < 20)
                {
                    return new DiceResult(new List<int> { distanceToDestination });
                }
            }

            return original;
        }

        /// <summary>
        /// �C�x���g�}�X�̕␳��K�p
        /// </summary>
        public DiceResult ApplyEventSquareCorrection(DiceResult original, List<int> eventSquareDistances)
        {
            // �v���X�w��J�[�h����ꂪ�߂��ꍇ�A�������₷������
            var total = original.Total;

            foreach (var distance in eventSquareDistances)
            {
                if (distance == total && _random.Next(100) < 30) // 30%�̊m���ŕ␳
                {
                    return original; // ���̂܂܎g�p
                }
            }

            // 10%�̊m���Ł}1�̕␳
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

                    // 1-6�͈͓̔��Ɏ��߂�
                    adjustedValues[0] = Math.Max(1, Math.Min(6, adjustedValues[0]));
                    return new DiceResult(adjustedValues);
                }
            }

            return original;
        }
    }

    /// <summary>
    /// �f�o�b�O�p�̌Œ�T�C�R���T�[�r�X
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
                    values.Add(1); // �f�t�H���g�l
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