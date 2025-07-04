using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// เz๐\ทlIuWFNg
    /// </summary>
    public struct Money : IComparable<Money>, IEquatable<Money>
    {
        private readonly long _value;

        /// <summary>
        /// ลๅli9999ญ99999999~j
        /// </summary>
        public static readonly Money Max = new Money(999999999999L);

        /// <summary>
        /// [
        /// </summary>
        public static readonly Money Zero = new Money(0);

        /// <summary>
        /// li~Pสj
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// RXgN^
        /// </summary>
        public Money(long value)
        {
            _value = Math.Max(0, Math.Min(value, Max._value));
        }

        /// <summary>
        /// มZZq
        /// </summary>
        public static Money operator +(Money a, Money b)
        {
            return new Money(a._value + b._value);
        }

        /// <summary>
        /// ธZZq
        /// </summary>
        public static Money operator -(Money a, Money b)
        {
            return new Money(Math.Max(0, a._value - b._value));
        }

        /// <summary>
        /// ๆZZqidecimalj
        /// </summary>
        public static Money operator *(Money a, decimal multiplier)
        {
            return new Money((long)(a._value * multiplier));
        }

        /// <summary>
        /// ๆZZqiintj
        /// </summary>
        public static Money operator *(Money a, int multiplier)
        {
            return new Money(a._value * multiplier);
        }

        /// <summary>
        /// ไrZq
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
        /// รูIศ^ฯทilong -> Moneyj
        /// </summary>
        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        /// <summary>
        /// IComparableภ
        /// </summary>
        public int CompareTo(Money other)
        {
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// IEquatableภ
        /// </summary>
        public bool Equals(Money other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// Equalsภ
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Money money && Equals(money);
        }

        /// <summary>
        /// GetHashCodeภ
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// ถ๑\ป
        /// </summary>
        public override string ToString()
        {
            if (_value >= 100000000) // 1ญศใ
            {
                var oku = _value / 100000000;
                var man = (_value % 100000000) / 10000;
                var yen = _value % 10000;

                if (man == 0 && yen == 0)
                    return $"{oku}ญ~";
                else if (yen == 0)
                    return $"{oku}ญ{man}~";
                else
                    return $"{oku}ญ{man}{yen}~";
            }
            else if (_value >= 10000) // 1ศใ
            {
                var man = _value / 10000;
                var yen = _value % 10000;

                if (yen == 0)
                    return $"{man}~";
                else
                    return $"{man}{yen}~";
            }
            else
            {
                return $"{_value}~";
            }
        }

        /// <summary>
        /// Zk`ฎฬถ๑\ป
        /// </summary>
        public string ToShortString()
        {
            if (_value >= 100000000) // 1ญศใ
            {
                var oku = _value / 100000000.0;
                return $"{oku:F1}ญ";
            }
            else if (_value >= 10000) // 1ศใ
            {
                var man = _value / 10000.0;
                return $"{man:F0}";
            }
            else
            {
                return $"{_value}";
            }
        }
    }
}