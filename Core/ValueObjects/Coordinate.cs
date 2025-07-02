using System;

namespace MomotetsuGame.Core.ValueObjects
{
    /// <summary>
    /// 2D���W��\���l�I�u�W�F�N�g
    /// </summary>
    public readonly struct Coordinate : IEquatable<Coordinate>
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
        /// <param name="x">X���W</param>
        /// <param name="y">Y���W</param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// ���̍��W�Ƃ̋������v�Z
        /// </summary>
        /// <param name="other">��r�Ώۂ̍��W</param>
        /// <returns>���[�N���b�h����</returns>
        public double DistanceTo(Coordinate other)
            => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

        /// <summary>
        /// ���W���ړ�
        /// </summary>
        /// <param name="deltaX">X�����̈ړ���</param>
        /// <param name="deltaY">Y�����̈ړ���</param>
        /// <returns>�V�������W</returns>
        public Coordinate Move(double deltaX, double deltaY)
            => new(X + deltaX, Y + deltaY);

        /// <summary>
        /// ���_���v�Z
        /// </summary>
        /// <param name="other">������̍��W</param>
        /// <returns>���_�̍��W</returns>
        public Coordinate MidPoint(Coordinate other)
            => new((X + other.X) / 2, (Y + other.Y) / 2);

        /// <summary>
        /// �p�x���v�Z�i���W�A���j
        /// </summary>
        /// <param name="other">�Ώۍ��W</param>
        /// <returns>�p�x�i���W�A���j</returns>
        public double AngleTo(Coordinate other)
            => Math.Atan2(other.Y - Y, other.X - X);

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Coordinate operator +(Coordinate a, Coordinate b)
            => new(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        public static Coordinate operator -(Coordinate a, Coordinate b)
            => new(a.X - b.X, a.Y - b.Y);

        /// <summary>
        /// �X�J���[�{���Z�q
        /// </summary>
        public static Coordinate operator *(Coordinate coord, double scalar)
            => new(coord.X * scalar, coord.Y * scalar);

        /// <summary>
        /// �X�J���[���Z���Z�q
        /// </summary>
        public static Coordinate operator /(Coordinate coord, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException();
            return new(coord.X / scalar, coord.Y / scalar);
        }

        // IEquatable<Coordinate>����
        public bool Equals(Coordinate other)
            => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object? obj)
            => obj is Coordinate coordinate && Equals(coordinate);

        public override int GetHashCode()
            => HashCode.Combine(X, Y);

        public override string ToString()
            => $"({X:F2}, {Y:F2})";

        /// <summary>
        /// �[�����W
        /// </summary>
        public static readonly Coordinate Zero = new(0, 0);
    }
}