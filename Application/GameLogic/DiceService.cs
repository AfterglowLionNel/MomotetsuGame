using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// �T�C�R���T�[�r�X�̎���
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
        /// �ʏ�̃T�C�R����U��
        /// </summary>
        /// <param name="count">�T�C�R���̐�</param>
        /// <returns>�T�C�R���̌���</returns>
        public DiceResult Roll(int count)
        {
            if (count <= 0)
                throw new ArgumentException("�T�C�R���̐���1�ȏ�ł���K�v������܂��B", nameof(count));

            var values = new List<int>();

            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    values.Add(_random.Next(1, 7)); // 1�`6�̒l
                }
            }

            return new DiceResult(values);
        }

        /// <summary>
        /// ����T�C�R����U��
        /// </summary>
        /// <param name="count">�T�C�R���̐�</param>
        /// <param name="min">�ŏ��l</param>
        /// <param name="max">�ő�l</param>
        /// <returns>�T�C�R���̌���</returns>
        public DiceResult RollSpecial(int count, int min, int max)
        {
            if (count <= 0)
                throw new ArgumentException("�T�C�R���̐���1�ȏ�ł���K�v������܂��B", nameof(count));
            if (min < 1 || min > max || max > 6)
                throw new ArgumentException("�ŏ��l�ƍő�l�͈̔͂��s���ł��B");

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
        /// �Œ�l�̃T�C�R�����ʂ��쐬�i�f�o�b�O/�e�X�g�p�j
        /// </summary>
        public DiceResult CreateFixed(params int[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("�l���w�肳��Ă��܂���B", nameof(values));

            return new DiceResult(values.ToList());
        }

        /// <summary>
        /// ��s�����̃T�C�R����U��i1�`2�̂݁j
        /// </summary>
        public DiceResult RollUnlucky(int count)
        {
            return RollSpecial(count, 1, 2);
        }

        /// <summary>
        /// ������Ԃ̃T�C�R����U��i1�Œ�j
        /// </summary>
        public DiceResult RollCow()
        {
            return new DiceResult(new List<int> { 1 });
        }

        /// <summary>
        /// �T�C�R���A�j���[�V�����p�̃����_���l�𐶐�
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
    /// �T�C�R���֘A�̃��[�e�B���e�B
    /// </summary>
    public static class DiceUtility
    {
        /// <summary>
        /// �T�C�R���̖ڂ̊��Ғl���v�Z
        /// </summary>
        public static double CalculateExpectedValue(int diceCount, int min = 1, int max = 6)
        {
            double singleDiceExpected = (min + max) / 2.0;
            return singleDiceExpected * diceCount;
        }

        /// <summary>
        /// ����̒l�ȏオ�o��m�����v�Z
        /// </summary>
        public static double CalculateProbability(int diceCount, int targetValue)
        {
            if (targetValue <= diceCount) return 1.0;
            if (targetValue > diceCount * 6) return 0.0;

            // �ȈՌv�Z�i���m�Ȍv�Z�͓��I�v��@���K�v�j
            double averageRoll = 3.5 * diceCount;
            double variance = 2.92 * diceCount; // �P��T�C�R���̕��U�͖�2.92
            double standardDeviation = Math.Sqrt(variance);

            // ���K���z�ŋߎ�
            double z = (targetValue - averageRoll) / standardDeviation;
            return 1 - NormalCDF(z);
        }

        /// <summary>
        /// ���K���z�̗ݐϕ��z�֐��i�ȈՔŁj
        /// </summary>
        private static double NormalCDF(double x)
        {
            // �G���[�֐��ɂ��ߎ�
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