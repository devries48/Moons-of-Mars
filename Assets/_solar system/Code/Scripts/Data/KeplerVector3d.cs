using System;
using System.Diagnostics.CodeAnalysis;

namespace MoonsOfMars.SolarSystem
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public struct KeplerVector3d
    {
        public double x;
        public double y;
        public double z;
        private const double EPSILON = 1.401298E-45;


        public KeplerVector3d normalized
        {
            get { return Normalize(this); }
        }

        public double magnitude
        {
            get { return Math.Sqrt(x * x + y * y + z * z); }
        }

        public double sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public static KeplerVector3d zero
        {
            get { return new KeplerVector3d(0d, 0d, 0d); }
        }

        public static KeplerVector3d one
        {
            get { return new KeplerVector3d(1d, 1d, 1d); }
        }

        public KeplerVector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public KeplerVector3d(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public KeplerVector3d(double x, double y)
        {
            this.x = x;
            this.y = y;
            z = 0d;
        }

        public static KeplerVector3d operator +(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static KeplerVector3d operator -(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static KeplerVector3d operator -(KeplerVector3d a)
        {
            return new KeplerVector3d(-a.x, -a.y, -a.z);
        }

        public static KeplerVector3d operator *(KeplerVector3d a, double d)
        {
            return new KeplerVector3d(a.x * d, a.y * d, a.z * d);
        }

        public static KeplerVector3d operator *(double d, KeplerVector3d a)
        {
            return new KeplerVector3d(a.x * d, a.y * d, a.z * d);
        }

        public static KeplerVector3d operator /(KeplerVector3d a, double d)
        {
            return new KeplerVector3d(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(KeplerVector3d lhs, KeplerVector3d rhs)
        {
            return SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
        }

        public static bool operator !=(KeplerVector3d lhs, KeplerVector3d rhs)
        {
            return SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
        }

        public static KeplerVector3d Lerp(KeplerVector3d from, KeplerVector3d to, double t)
        {
            t = t < 0 ? 0 : t > 1.0 ? 1.0 : t;
            return new KeplerVector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
        }

        public static KeplerVector3d MoveTowards(KeplerVector3d current, KeplerVector3d target, double maxDistanceDelta)
        {
            KeplerVector3d vector3 = target - current;
            double magnitude = vector3.magnitude;
            if (magnitude <= maxDistanceDelta || magnitude == 0.0)
            {
                return target;
            }
            else
            {
                return current + vector3 / magnitude * maxDistanceDelta;
            }
        }

        public void Set(double new_x, double new_y, double new_z)
        {
            x = new_x;
            y = new_y;
            z = new_z;
        }

        public static KeplerVector3d Scale(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Scale(KeplerVector3d scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        public static KeplerVector3d Cross(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }

        public override bool Equals(object other)
        {
            if (other is not KeplerVector3d)
            {
                return false;
            }

            KeplerVector3d vector3d = (KeplerVector3d)other;
            if (x.Equals(vector3d.x) && y.Equals(vector3d.y))
            {
                return z.Equals(vector3d.z);
            }
            else
            {
                return false;
            }
        }

        public static KeplerVector3d Reflect(KeplerVector3d inDirection, KeplerVector3d inNormal)
        {
            return -2d * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static KeplerVector3d Normalize(KeplerVector3d value)
        {
            double num = Magnitude(value);
            if (num > EPSILON)
            {
                return value / num;
            }
            else
            {
                return zero;
            }
        }

        public void Normalize()
        {
            double num = Magnitude(this);
            if (num > EPSILON)
            {
                this /= num;
            }
            else
            {
                this = zero;
            }
        }

        public override string ToString()
        {
            return "(" + x + "; " + y + "; " + z + ")";
        }

        public string ToString(string format)
        {
            return "(" + x.ToString(format) + "; " + y.ToString(format) + "; " + z.ToString(format) + ")";
        }

        public static double Dot(KeplerVector3d a, KeplerVector3d b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static KeplerVector3d Project(KeplerVector3d vector, KeplerVector3d onNormal)
        {
            double num = Dot(onNormal, onNormal);
            if (num < 1.40129846432482E-45d)
            {
                return zero;
            }
            else
            {
                return onNormal * Dot(vector, onNormal) / num;
            }
        }

        public static KeplerVector3d Exclude(KeplerVector3d excludeThis, KeplerVector3d fromThat)
        {
            return fromThat - Project(fromThat, excludeThis);
        }

        public static double Distance(KeplerVector3d a, KeplerVector3d b)
        {
            var vector3d = new KeplerVector3d(a.x - b.x, a.y - b.y, a.z - b.z);
            return Math.Sqrt(vector3d.x * vector3d.x + vector3d.y * vector3d.y + vector3d.z * vector3d.z);
        }

        public static KeplerVector3d ClampMagnitude(KeplerVector3d vector, double maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength)
            {
                return vector.normalized * maxLength;
            }
            else
            {
                return vector;
            }
        }

        public static double Angle(KeplerVector3d from, KeplerVector3d to)
        {
            double dot = Dot(from.normalized, to.normalized);
            return Math.Acos(dot < -1.0 ? -1.0 : dot > 1.0 ? 1.0 : dot) * 57.29578d;
        }

        public static double Magnitude(KeplerVector3d a)
        {
            return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        }

        public static double SqrMagnitude(KeplerVector3d a)
        {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }

        public static KeplerVector3d Min(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        public static KeplerVector3d Max(KeplerVector3d a, KeplerVector3d b)
        {
            return new KeplerVector3d(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }
    }
}