using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// 座標を表す値オブジェクト
    /// </summary>
    public struct Coordinate : IEquatable<Coordinate>
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
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 別の座標までの距離を計算（ユークリッド距離）
        /// </summary>
        public double DistanceTo(Coordinate other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 別の座標までのマンハッタン距離を計算
        /// </summary>
        public double ManhattanDistanceTo(Coordinate other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// 座標間のベクトルを取得
        /// </summary>
        public Coordinate VectorTo(Coordinate other)
        {
            return new Coordinate(other.X - X, other.Y - Y);
        }

        /// <summary>
        /// 座標を移動
        /// </summary>
        public Coordinate Translate(double dx, double dy)
        {
            return new Coordinate(X + dx, Y + dy);
        }

        /// <summary>
        /// 別の座標との中点を取得
        /// </summary>
        public Coordinate MidpointTo(Coordinate other)
        {
            return new Coordinate((X + other.X) / 2, (Y + other.Y) / 2);
        }

        /// <summary>
        /// 座標を回転（原点中心）
        /// </summary>
        public Coordinate Rotate(double angleInRadians)
        {
            var cos = Math.Cos(angleInRadians);
            var sin = Math.Sin(angleInRadians);
            return new Coordinate(
                X * cos - Y * sin,
                X * sin + Y * cos
            );
        }

        /// <summary>
        /// 座標を回転（指定した中心点周り）
        /// </summary>
        public Coordinate RotateAround(Coordinate center, double angleInRadians)
        {
            var translated = new Coordinate(X - center.X, Y - center.Y);
            var rotated = translated.Rotate(angleInRadians);
            return new Coordinate(rotated.X + center.X, rotated.Y + center.Y);
        }

        /// <summary>
        /// 加算演算子
        /// </summary>
        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// 減算演算子
        /// </summary>
        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// スカラー倍演算子
        /// </summary>
        public static Coordinate operator *(Coordinate a, double scalar)
        {
            return new Coordinate(a.X * scalar, a.Y * scalar);
        }

        /// <summary>
        /// スカラー除算演算子
        /// </summary>
        public static Coordinate operator /(Coordinate a, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide coordinate by zero.");
            return new Coordinate(a.X / scalar, a.Y / scalar);
        }

        /// <summary>
        /// 等価演算子
        /// </summary>
        public static bool operator ==(Coordinate a, Coordinate b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 非等価演算子
        /// </summary>
        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// IEquatable実装
        /// </summary>
        public bool Equals(Coordinate other)
        {
            const double epsilon = 0.0001;
            return Math.Abs(X - other.X) < epsilon && Math.Abs(Y - other.Y) < epsilon;
        }

        /// <summary>
        /// Equals実装
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Coordinate coordinate && Equals(coordinate);
        }

        /// <summary>
        /// GetHashCode実装
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }

        /// <summary>
        /// ゼロ座標
        /// </summary>
        public static readonly Coordinate Zero = new Coordinate(0, 0);

        /// <summary>
        /// 単位ベクトル
        /// </summary>
        public static readonly Coordinate UnitX = new Coordinate(1, 0);
        public static readonly Coordinate UnitY = new Coordinate(0, 1);
    }
}