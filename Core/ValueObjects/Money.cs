using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// ‹àŠz‚ğ•\‚·’lƒIƒuƒWƒFƒNƒg
    /// </summary>
    public struct Money : IComparable<Money>, IEquatable<Money>
    {
        private readonly long _value;

        /// <summary>
        /// Å‘å’li9999‰­9999–œ9999‰~j
        /// </summary>
        public static readonly Money Max = new Money(999999999999L);

        /// <summary>
        /// ƒ[ƒ
        /// </summary>
        public static readonly Money Zero = new Money(0);

        /// <summary>
        /// ’li‰~’PˆÊj
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// ƒRƒ“ƒXƒgƒ‰ƒNƒ^
        /// </summary>
        public Money(long value)
        {
            _value = Math.Max(0, Math.Min(value, Max._value));
        }

        /// <summary>
        /// ‰ÁZ‰‰Zq
        /// </summary>
        public static Money operator +(Money a, Money b)
        {
            return new Money(a._value + b._value);
        }

        /// <summary>
        /// Œ¸Z‰‰Zq
        /// </summary>
        public static Money operator -(Money a, Money b)
        {
            return new Money(Math.Max(0, a._value - b._value));
        }

        /// <summary>
        /// æZ‰‰Zqidecimalj
        /// </summary>
        public static Money operator *(Money a, decimal multiplier)
        {
            return new Money((long)(a._value * multiplier));
        }

        /// <summary>
        /// æZ‰‰Zqiintj
        /// </summary>
        public static Money operator *(Money a, int multiplier)
        {
            return new Money(a._value * multiplier);
        }

        /// <summary>
        /// ”äŠr‰‰Zq
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
        /// ˆÃ–Ù“I‚ÈŒ^•ÏŠ·ilong -> Moneyj
        /// </summary>
        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        /// <summary>
        /// IComparableÀ‘•
        /// </summary>
        public int CompareTo(Money other)
        {
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// IEquatableÀ‘•
        /// </summary>
        public bool Equals(Money other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// EqualsÀ‘•
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Money money && Equals(money);
        }

        /// <summary>
        /// GetHashCodeÀ‘•
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// •¶š—ñ•\Œ»
        /// </summary>
        public override string ToString()
        {
            if (_value >= 100000000) // 1‰­ˆÈã
            {
                var oku = _value / 100000000;
                var man = (_value % 100000000) / 10000;
                var yen = _value % 10000;

                if (man == 0 && yen == 0)
                    return $"{oku}‰­‰~";
                else if (yen == 0)
                    return $"{oku}‰­{man}–œ‰~";
                else
                    return $"{oku}‰­{man}–œ{yen}‰~";
            }
            else if (_value >= 10000) // 1–œˆÈã
            {
                var man = _value / 10000;
                var yen = _value % 10000;

                if (yen == 0)
                    return $"{man}–œ‰~";
                else
                    return $"{man}–œ{yen}‰~";
            }
            else
            {
                return $"{_value}‰~";
            }
        }

        /// <summary>
        /// ’ZkŒ`®‚Ì•¶š—ñ•\Œ»
        /// </summary>
        public string ToShortString()
        {
            if (_value >= 100000000) // 1‰­ˆÈã
            {
                var oku = _value / 100000000.0;
                return $"{oku:F1}‰­";
            }
            else if (_value >= 10000) // 1–œˆÈã
            {
                var man = _value / 10000.0;
                return $"{man:F0}–œ";
            }
            else
            {
                return $"{_value}";
            }
        }
    }
}