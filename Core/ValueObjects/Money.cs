using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// ���z��\���l�I�u�W�F�N�g
    /// </summary>
    public readonly struct Money : IComparable<Money>, IEquatable<Money>
    {
        private readonly long _value;

        /// <summary>
        /// �ő���z�i9999��9999��9999�~�j
        /// </summary>
        public static readonly Money Max = new(999999999999L);

        /// <summary>
        /// �[���~
        /// </summary>
        public static readonly Money Zero = new(0);

        /// <summary>
        /// ���z�̒l
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="value">���z</param>
        public Money(long value)
        {
            _value = Math.Min(Math.Max(0, value), Max._value);
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Money operator +(Money a, Money b)
            => new(a._value + b._value);

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Money operator -(Money a, Money b)
            => new(Math.Max(0, a._value - b._value));

        /// <summary>
        /// ��Z���Z�q�i�{���j
        /// </summary>
        public static Money operator *(Money money, decimal multiplier)
            => new((long)(money._value * multiplier));

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();
            return new((long)(money._value / divisor));
        }

        // ��r���Z�q
        public static bool operator >(Money a, Money b) => a._value > b._value;
        public static bool operator <(Money a, Money b) => a._value < b._value;
        public static bool operator >=(Money a, Money b) => a._value >= b._value;
        public static bool operator <=(Money a, Money b) => a._value <= b._value;
        public static bool operator ==(Money a, Money b) => a._value == b._value;
        public static bool operator !=(Money a, Money b) => a._value != b._value;

        /// <summary>
        /// ������\���i���{�~�`���j
        /// </summary>
        public override string ToString()
        {
            if (_value >= 100000000) // 1���ȏ�
            {
                var oku = _value / 100000000;
                var man = (_value % 100000000) / 10000;
                if (man > 0)
                    return $"{oku}��{man}���~";
                return $"{oku}���~";
            }
            else if (_value >= 10000) // 1���ȏ�
            {
                var man = _value / 10000;
                var yen = _value % 10000;
                if (yen > 0)
                    return $"{man}��{yen}�~";
                return $"{man}���~";
            }
            else
            {
                return $"{_value}�~";
            }
        }

        /// <summary>
        /// �Z�k�`���̕�����\��
        /// </summary>
        public string ToShortString()
        {
            if (_value >= 100000000) // 1���ȏ�
                return $"{_value / 100000000}��";
            else if (_value >= 10000) // 1���ȏ�
                return $"{_value / 10000}��";
            else
                return $"{_value}";
        }

        /// <summary>
        /// �p�[�Z���e�[�W���v�Z
        /// </summary>
        public Money CalculatePercentage(decimal percentage)
        {
            return new Money((long)(_value * percentage / 100));
        }

        // IComparable<Money>����
        public int CompareTo(Money other) => _value.CompareTo(other._value);

        // IEquatable<Money>����
        public bool Equals(Money other) => _value == other._value;

        public override bool Equals(object? obj) => obj is Money money && Equals(money);

        public override int GetHashCode() => _value.GetHashCode();
    }
}