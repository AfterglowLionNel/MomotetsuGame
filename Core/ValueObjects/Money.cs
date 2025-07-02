using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// ‹àŠz‚ğ•\‚·’lƒIƒuƒWƒFƒNƒg
    /// </summary>
    public readonly struct Money : IComparable<Money>, IEquatable<Money>
    {
        private readonly long _value;

        /// <summary>
        /// Å‘å‹àŠzi9999‰­9999–œ9999‰~j
        /// </summary>
        public static readonly Money Max = new(999999999999L);

        /// <summary>
        /// ƒ[ƒ‰~
        /// </summary>
        public static readonly Money Zero = new(0);

        /// <summary>
        /// ‹àŠz‚Ì’l
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// ƒRƒ“ƒXƒgƒ‰ƒNƒ^
        /// </summary>
        /// <param name="value">‹àŠz</param>
        public Money(long value)
        {
            _value = Math.Min(Math.Max(0, value), Max._value);
        }

        /// <summary>
        /// ‰ÁZ‰‰Zq
        /// </summary>
        public static Money operator +(Money a, Money b)
            => new(a._value + b._value);

        /// <summary>
        /// Œ¸Z‰‰Zq
        /// </summary>
        public static Money operator -(Money a, Money b)
            => new(Math.Max(0, a._value - b._value));

        /// <summary>
        /// æZ‰‰Zqi”{—¦j
        /// </summary>
        public static Money operator *(Money money, decimal multiplier)
            => new((long)(money._value * multiplier));

        /// <summary>
        /// œZ‰‰Zq
        /// </summary>
        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();
            return new((long)(money._value / divisor));
        }

        // ”äŠr‰‰Zq
        public static bool operator >(Money a, Money b) => a._value > b._value;
        public static bool operator <(Money a, Money b) => a._value < b._value;
        public static bool operator >=(Money a, Money b) => a._value >= b._value;
        public static bool operator <=(Money a, Money b) => a._value <= b._value;
        public static bool operator ==(Money a, Money b) => a._value == b._value;
        public static bool operator !=(Money a, Money b) => a._value != b._value;

        /// <summary>
        /// •¶š—ñ•\Œ»i“ú–{‰~Œ`®j
        /// </summary>
        public override string ToString()
        {
            if (_value >= 100000000) // 1‰­ˆÈã
            {
                var oku = _value / 100000000;
                var man = (_value % 100000000) / 10000;
                if (man > 0)
                    return $"{oku}‰­{man}–œ‰~";
                return $"{oku}‰­‰~";
            }
            else if (_value >= 10000) // 1–œˆÈã
            {
                var man = _value / 10000;
                var yen = _value % 10000;
                if (yen > 0)
                    return $"{man}–œ{yen}‰~";
                return $"{man}–œ‰~";
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
                return $"{_value / 100000000}‰­";
            else if (_value >= 10000) // 1–œˆÈã
                return $"{_value / 10000}–œ";
            else
                return $"{_value}";
        }

        /// <summary>
        /// ƒp[ƒZƒ“ƒe[ƒW‚ğŒvZ
        /// </summary>
        public Money CalculatePercentage(decimal percentage)
        {
            return new Money((long)(_value * percentage / 100));
        }

        // IComparable<Money>À‘•
        public int CompareTo(Money other) => _value.CompareTo(other._value);

        // IEquatable<Money>À‘•
        public bool Equals(Money other) => _value == other._value;

        public override bool Equals(object? obj) => obj is Money money && Equals(money);

        public override int GetHashCode() => _value.GetHashCode();
    }
}