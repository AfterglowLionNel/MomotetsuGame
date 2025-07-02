using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// ���z��\���l�I�u�W�F�N�g
    /// </summary>
    public struct Money : IComparable<Money>, IEquatable<Money>
    {
        private readonly long _value;

        /// <summary>
        /// �ő�l�i9999��9999��9999�~�j
        /// </summary>
        public static readonly Money Max = new Money(999999999999L);

        /// <summary>
        /// �[��
        /// </summary>
        public static readonly Money Zero = new Money(0);

        /// <summary>
        /// �l�i�~�P�ʁj
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Money(long value)
        {
            _value = Math.Max(0, Math.Min(value, Max._value));
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Money operator +(Money a, Money b)
        {
            return new Money(a._value + b._value);
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Money operator -(Money a, Money b)
        {
            return new Money(Math.Max(0, a._value - b._value));
        }

        /// <summary>
        /// ��Z���Z�q�idecimal�j
        /// </summary>
        public static Money operator *(Money a, decimal multiplier)
        {
            return new Money((long)(a._value * multiplier));
        }

        /// <summary>
        /// ��Z���Z�q�iint�j
        /// </summary>
        public static Money operator *(Money a, int multiplier)
        {
            return new Money(a._value * multiplier);
        }

        /// <summary>
        /// ��r���Z�q
        /// </summary>
        public static bool operator >(Money a, Money b)
        {
            return a._value > b._value;
        }

        public static bool operator <(Money a, Money b)
        {
            return a._value < b._value;
        }

        public static bool operator >=(Money a, Money b)
        {
            return a._value >= b._value;
        }

        public static bool operator <=(Money a, Money b)
        {
            return a._value <= b._value;
        }

        public static bool operator ==(Money a, Money b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(Money a, Money b)
        {
            return a._value != b._value;
        }

        /// <summary>
        /// �ÖٓI�Ȍ^�ϊ��ilong -> Money�j
        /// </summary>
        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        /// <summary>
        /// IComparable����
        /// </summary>
        public int CompareTo(Money other)
        {
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// IEquatable����
        /// </summary>
        public bool Equals(Money other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// Equals����
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Money money && Equals(money);
        }

        /// <summary>
        /// GetHashCode����
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// ������\��
        /// </summary>
        public override string ToString()
        {
            if (_value >= 100000000) // 1���ȏ�
            {
                var oku = _value / 100000000;
                var man = (_value % 100000000) / 10000;
                var yen = _value % 10000;

                if (man == 0 && yen == 0)
                    return $"{oku}���~";
                else if (yen == 0)
                    return $"{oku}��{man}���~";
                else
                    return $"{oku}��{man}��{yen}�~";
            }
            else if (_value >= 10000) // 1���ȏ�
            {
                var man = _value / 10000;
                var yen = _value % 10000;

                if (yen == 0)
                    return $"{man}���~";
                else
                    return $"{man}��{yen}�~";
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
            {
                var oku = _value / 100000000.0;
                return $"{oku:F1}��";
            }
            else if (_value >= 10000) // 1���ȏ�
            {
                var man = _value / 10000.0;
                return $"{man:F0}��";
            }
            else
            {
                return $"{_value}";
            }
        }
    }
}