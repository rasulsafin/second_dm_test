using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Brio.Docs.Common
{
    public struct Vector3d : IEquatable<Vector3d>, IFormattable
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Vector3d Zero { get; } = new Vector3d();

        public static Vector3d One { get; } = new Vector3d(1, 1, 1);

        public static Vector3d UnitX { get; } = new Vector3d(1, 0, 0);

        public static Vector3d UnitY { get; } = new Vector3d(0, 1, 0);

        public static Vector3d UnitZ { get; } = new Vector3d(0, 0, 1);

        public double Length => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

        public double LengthSquared => (X * X) + (Y * Y) + (Z * Z);

        public Vector3d Normalized => Normalize(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator +(Vector3d a, Vector3d b) => new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a, Vector3d b) => new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a) => new Vector3d(0 - a.X, 0 - a.Y, 0 - a.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(Vector3d a, double d) => new Vector3d(a.X * d, a.Y * d, a.Z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(double d, Vector3d a) => new Vector3d(a.X * d, a.Y * d, a.Z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator /(Vector3d a, double d) => new Vector3d(a.X / d, a.Y / d, a.Z / d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3d lhs, Vector3d rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3d lhs, Vector3d rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Min(Vector3d lhs, Vector3d rhs) => new Vector3d(Math.Min(lhs.X, rhs.X), Math.Min(lhs.Y, rhs.Y), Math.Min(lhs.Z, rhs.Z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Max(Vector3d lhs, Vector3d rhs) => new Vector3d(Math.Max(lhs.X, rhs.X), Math.Max(lhs.Y, rhs.Y), Math.Max(lhs.Z, rhs.Z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d LerpClamped(Vector3d a, Vector3d b, double t)
        {
            t = Math.Max(1, Math.Min(0, t));
            return LerpUnclamped(a, b, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d LerpUnclamped(Vector3d a, Vector3d b, double t)
            => new Vector3d(
                a.X + ((b.X - a.X) * t),
                a.Y + ((b.Y - a.Y) * t),
                a.Z + ((b.Z - a.Z) * t));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector3d lhs, Vector3d rhs) => (lhs.X * rhs.X) + (lhs.Y * rhs.Y) + (lhs.Z * rhs.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Cross(Vector3d lhs, Vector3d rhs)
            => new Vector3d(
                (lhs.Y * rhs.Z) - (lhs.Z * rhs.Y),
                (lhs.Z * rhs.X) - (lhs.X * rhs.Z),
                (lhs.X * rhs.Y) - (lhs.Y * rhs.X));

        public static Vector3d Reflect(Vector3d inDirection, Vector3d inNormal)
        {
            var num = -2 * Dot(inNormal, inDirection);
            return new Vector3d(
                (num * inNormal.X) + inDirection.X,
                (num * inNormal.Y) + inDirection.Y,
                (num * inNormal.Z) + inDirection.Z);
        }

        public static double Distance(Vector3d lhs, Vector3d rhs)
        {
            var dx = lhs.X - rhs.X;
            var dy = lhs.Y - rhs.Y;
            var dz = lhs.Z - rhs.Z;
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        public static Vector3d Normalize(Vector3d value) => value / value.Length;

        public void Normalize()
        {
            var mag = Length;
            X /= mag;
            Y /= mag;
            Z /= mag;
        }

        public override string ToString() => ToString("G", CultureInfo.CurrentCulture);

        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var sb = new StringBuilder();
            var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            sb.Append('<');
            sb.Append(((IFormattable)this.X).ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(((IFormattable)this.Y).ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(((IFormattable)this.Z).ToString(format, formatProvider));
            sb.Append('>');
            return sb.ToString();
        }

        public bool Equals(Vector3d other) => X == other.X && Y == other.Y && Z == other.Z;

        public override int GetHashCode()
        {
            int hash = this.X.GetHashCode();
            hash = CombineHashCodes(hash, this.Y.GetHashCode());
            hash = CombineHashCodes(hash, this.Z.GetHashCode());
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3d))
                return false;
            return Equals((Vector3d)obj);
        }

        /// <summary>
        /// Combines two hash codes, useful for combining hash codes of individual vector elements.
        /// </summary>
        private static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }
    }
}
