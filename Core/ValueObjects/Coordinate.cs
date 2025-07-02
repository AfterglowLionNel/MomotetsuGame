using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// 2D座標を表す値オブジェクト
    /// </summary>
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 他の座標との距離を計算
        /// </summary>
        /// <param name="other">比較対象の座標</param>
        /// <returns>ユークリッド距離</returns>
        public double DistanceTo(Coordinate other)
            => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

        /// <summary>
        /// 座標を移動
        /// </summary>
        /// <param name="deltaX">X方向の移動量</param>
        /// <param name="deltaY">Y方向の移動量</param>
        /// <returns>新しい座標</returns>
        public Coordinate Move(double deltaX, double deltaY)
            => new(X + deltaX, Y + deltaY);

        /// <summary>
        /// 中点を計算
        /// </summary>
        /// <param name="other">もう一つの座標</param>
        /// <returns>中点の座標</returns>
        public Coordinate MidPoint(Coordinate other)
            => new((X + other.X) / 2, (Y + other.Y) / 2);

        /// <summary>
        /// 角度を計算（ラジアン）
        /// </summary>
        /// <param name="other">対象座標</param>
        /// <returns>角度（ラジアン）</returns>
        public double AngleTo(Coordinate other)
            => Math.Atan2(other.Y - Y, other.X - X);

        /// <summary>
        /// 加算演算子
        /// </summary>
        public static Coordinate operator +(Coordinate a, Coordinate b)
            => new(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// 減算演算子
        /// </summary>
        public static Coordinate operator -(Coordinate a, Coordinate b)
            => new(a.X - b.X, a.Y - b.Y);

        /// <summary>
        /// スカラー倍演算子
        /// </summary>
        public static Coordinate operator *(Coordinate coord, double scalar)
            => new(coord.X * scalar, coord.Y * scalar);

        /// <summary>
        /// スカラー除算演算子
        /// </summary>
        public static Coordinate operator /(Coordinate coord, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException();
            return new(coord.X / scalar, coord.Y / scalar);
        }

        // IEquatable<Coordinate>実装
        public bool Equals(Coordinate other)
            => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object? obj)
            => obj is Coordinate coordinate && Equals(coordinate);

        public override int GetHashCode()
            => HashCode.Combine(X, Y);

        public override string ToString()
            => $"({X:F2}, {Y:F2})";

        /// <summary>
        /// ゼロ座標
        /// </summary>
        public static readonly Coordinate Zero = new(0, 0);
    }
}