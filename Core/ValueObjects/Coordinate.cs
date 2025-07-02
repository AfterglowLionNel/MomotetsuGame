using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// ���W��\���l�I�u�W�F�N�g
    /// </summary>
    public struct Coordinate : IEquatable<Coordinate>
    {
        /// <summary>
        /// X���W
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y���W
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// �ʂ̍��W�܂ł̋������v�Z�i���[�N���b�h�����j
        /// </summary>
        public double DistanceTo(Coordinate other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// �ʂ̍��W�܂ł̃}���n�b�^���������v�Z
        /// </summary>
        public double ManhattanDistanceTo(Coordinate other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// ���W�Ԃ̃x�N�g�����擾
        /// </summary>
        public Coordinate VectorTo(Coordinate other)
        {
            return new Coordinate(other.X - X, other.Y - Y);
        }

        /// <summary>
        /// ���W���ړ�
        /// </summary>
        public Coordinate Translate(double dx, double dy)
        {
            return new Coordinate(X + dx, Y + dy);
        }

        /// <summary>
        /// �ʂ̍��W�Ƃ̒��_���擾
        /// </summary>
        public Coordinate MidpointTo(Coordinate other)
        {
            return new Coordinate((X + other.X) / 2, (Y + other.Y) / 2);
        }

        /// <summary>
        /// ���W����]�i���_���S�j
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
        /// ���W����]�i�w�肵�����S�_����j
        /// </summary>
        public Coordinate RotateAround(Coordinate center, double angleInRadians)
        {
            var translated = new Coordinate(X - center.X, Y - center.Y);
            var rotated = translated.Rotate(angleInRadians);
            return new Coordinate(rotated.X + center.X, rotated.Y + center.Y);
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// �X�J���[�{���Z�q
        /// </summary>
        public static Coordinate operator *(Coordinate a, double scalar)
        {
            return new Coordinate(a.X * scalar, a.Y * scalar);
        }

        /// <summary>
        /// �X�J���[���Z���Z�q
        /// </summary>
        public static Coordinate operator /(Coordinate a, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide coordinate by zero.");
            return new Coordinate(a.X / scalar, a.Y / scalar);
        }

        /// <summary>
        /// �������Z�q
        /// </summary>
        public static bool operator ==(Coordinate a, Coordinate b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// �񓙉����Z�q
        /// </summary>
        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// IEquatable����
        /// </summary>
        public bool Equals(Coordinate other)
        {
            const double epsilon = 0.0001;
            return Math.Abs(X - other.X) < epsilon && Math.Abs(Y - other.Y) < epsilon;
        }

        /// <summary>
        /// Equals����
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Coordinate coordinate && Equals(coordinate);
        }

        /// <summary>
        /// GetHashCode����
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// ������\��
        /// </summary>
        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }

        /// <summary>
        /// �[�����W
        /// </summary>
        public static readonly Coordinate Zero = new Coordinate(0, 0);

        /// <summary>
        /// �P�ʃx�N�g��
        /// </summary>
        public static readonly Coordinate UnitX = new Coordinate(1, 0);
        public static readonly Coordinate UnitY = new Coordinate(0, 1);
    }
}