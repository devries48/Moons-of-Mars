using System;

/// <summary>
/// Orbit data container.
/// Also contains methods for altering and updating orbit state.
/// </summary>
[Serializable]
public class KeplerOrbitData
{
    internal double GravConst = 1;

    /// <summary>
    /// Normal of ecliptic plane.
    /// </summary>
    public static readonly KeplerVector3d EclipticNormal = new KeplerVector3d(0, 1, 0);  // 0,0,1

    /// <summary>
    /// Up direction on ecliptic plane (y-axis on xy ecliptic plane).
    /// </summary>
    public static readonly KeplerVector3d EclipticUp = new KeplerVector3d(0, 0, 1);  // 0,1,0

    /// <summary>
    /// Right vector on ecliptic plane (x-axis on xy ecliptic plane).
    /// </summary>
    public static readonly KeplerVector3d EclipticRight = new KeplerVector3d(1, 0, 0); //1,0,0

    /// <summary>
    /// Body position relative to attractor or Focal Position.
    /// </summary>
    /// <remarks>
    /// Attractor (focus) is local center of orbit 
    /// </remarks>
    public KeplerVector3d Position;

    /// <summary>
    /// Magnitude of body position vector.
    /// </summary>
    public double AttractorDistance;

    /// <summary>
    /// Attractor point mass.
    /// </summary>
    public double AttractorMass;

    /// <summary>
    /// Body velocity vector relative to attractor.
    /// </summary>
    public KeplerVector3d Velocity;

    /// <summary>
    /// Gravitational parameter of 
    /// </summary>
    private double MG;

    public double SemiMinorAxis;
    public double SemiMajorAxis;
    private double FocalParameter;
    public double Eccentricity;
    public double Period;
    public double TrueAnomaly;
    public double MeanAnomaly;
    public double EccentricAnomaly;
    public double MeanMotion;
    public KeplerVector3d Periapsis;
    public double PeriapsisDistance;
    public KeplerVector3d Apoapsis;
    public double ApoapsisDistance;
    public KeplerVector3d CenterPoint;
    public double OrbitCompressionRatio;
    public KeplerVector3d OrbitNormal;
    public KeplerVector3d SemiMinorAxisBasis;
    public KeplerVector3d SemiMajorAxisBasis;

    /// <summary>
    /// if > 0, then orbit motion is clockwise
    /// </summary>
    public double OrbitNormalDotEclipticNormal;

    public double EnergyTotal
    {
        get { return Velocity.sqrMagnitude - 2 * MG / AttractorDistance; }
    }

    /// <summary>
    /// The orbit inclination in radians relative to ecliptic plane.
    /// </summary>
    public double Inclination
    {
        get
        {
            var dot = KeplerVector3d.Dot(OrbitNormal, EclipticNormal);
            return Math.Acos(dot);
        }
    }

    /// <summary>
    /// Ascending node longitude in radians.
    /// </summary>
    public double AscendingNodeLongitude
    {
        get
        {
            var ascNodeDir = KeplerVector3d.Cross(EclipticNormal, OrbitNormal).normalized;
            var dot = KeplerVector3d.Dot(ascNodeDir, EclipticRight);
            var angle = Math.Acos(dot);
            if (KeplerVector3d.Dot(KeplerVector3d.Cross(ascNodeDir, EclipticRight), EclipticNormal) >= 0)
            {
                angle = KeplerOrbitUtils.PI_2 - angle;
            }

            return angle;
        }
    }

    /// <summary>
    /// Angle between main orbit axis and ecliptic 0 axis in radians.
    /// </summary>
    public double ArgumentOfPerifocus
    {
        get
        {
            var ascNodeDir = KeplerVector3d.Cross(EclipticNormal, OrbitNormal).normalized;
            var dot = KeplerVector3d.Dot(ascNodeDir, SemiMajorAxisBasis.normalized);
            var angle = Math.Acos(dot);
            if (KeplerVector3d.Dot(KeplerVector3d.Cross(ascNodeDir, SemiMajorAxisBasis), OrbitNormal) < 0)
            {
                angle = KeplerOrbitUtils.PI_2 - angle;
            }

            return angle;
        }
    }

    /// <summary>
    /// Is orbit state valid and error-free.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid orbit; otherwise, <c>false</c>.
    /// </value>
    public bool IsValidOrbit
    {
        get
        {
            return Eccentricity >= 0
                   && Period > 0
                   && AttractorMass > 0;
        }
    }

    /// <summary>
    /// Create new orbit state without initialization. Manual orbit initialization is required.
    /// </summary>
    /// <remarks>
    /// To manually initialize orbit, fill known orbital elements and then call CalculateOrbitStateFrom... method.
    /// </remarks>
    public KeplerOrbitData()
    {
    }

    /// <summary>
    /// Create and initialize new orbit state.
    /// </summary>
    /// <param name="position">Body local position, relative to attractor.</param>
    /// <param name="velocity">Body local velocity.</param>
    /// <param name="attractorMass">Attractor mass.</param>
    /// <param name="gConst">Gravitational Constant.</param>
    public KeplerOrbitData(KeplerVector3d position, KeplerVector3d velocity, double attractorMass, double gConst)
    {
        this.Position = position;
        this.Velocity = velocity;
        this.AttractorMass = attractorMass;
        this.GravConst = gConst;
        CalculateOrbitStateFromOrbitalVectors();
    }

    /// <summary>
    /// Create and initialize new orbit state from orbital elements.
    /// </summary>
    /// <param name="eccentricity">Eccentricity.</param>
    /// <param name="semiMajorAxis">Main axis semi width.</param>
    /// <param name="meanAnomalyDeg">Mean anomaly in degrees.</param>
    /// <param name="inclinationDeg">Orbit inclination in degrees.</param>
    /// <param name="argOfPerifocusDeg">Orbit argument of perifocus in degrees.</param>
    /// <param name="ascendingNodeDeg">Longitude of ascending node in degrees.</param>
    /// <param name="attractorMass">Attractor mass.</param>
    /// <param name="gConst">Gravitational constant.</param>
    public KeplerOrbitData(double eccentricity, double semiMajorAxis, double meanAnomalyDeg, double inclinationDeg, double argOfPerifocusDeg, double ascendingNodeDeg, double attractorMass,
        double gConst, double period)
    {
        this.Period = period;
        this.Eccentricity = eccentricity;
        this.SemiMajorAxis = semiMajorAxis;
        if (eccentricity < 1.0)
        {
            this.SemiMinorAxis = SemiMajorAxis * Math.Sqrt(1 - Eccentricity * Eccentricity);
        }
        else if (eccentricity > 1.0)
        {
            this.SemiMinorAxis = SemiMajorAxis * Math.Sqrt(Eccentricity * Eccentricity - 1);
        }
        else
        {
            this.SemiMajorAxis = 0;
        }

        var normal = EclipticNormal.normalized;
        var ascendingNode = EclipticRight.normalized;

        ascendingNodeDeg %= 360;
        if (ascendingNodeDeg > 180) ascendingNodeDeg -= 360;
        inclinationDeg %= 360;
        if (inclinationDeg > 180) inclinationDeg -= 360;
        argOfPerifocusDeg %= 360;
        if (argOfPerifocusDeg > 180) argOfPerifocusDeg -= 360;

        ascendingNode = KeplerOrbitUtils.RotateVectorByAngle(ascendingNode, ascendingNodeDeg * KeplerOrbitUtils.Deg2Rad, normal).normalized;
        normal = KeplerOrbitUtils.RotateVectorByAngle(normal, inclinationDeg * KeplerOrbitUtils.Deg2Rad, ascendingNode).normalized;
        Periapsis = ascendingNode;
        Periapsis = KeplerOrbitUtils.RotateVectorByAngle(Periapsis, argOfPerifocusDeg * KeplerOrbitUtils.Deg2Rad, normal).normalized;

        this.SemiMajorAxisBasis = Periapsis;
        this.SemiMinorAxisBasis = KeplerVector3d.Cross(Periapsis, normal);

        this.MeanAnomaly = meanAnomalyDeg * KeplerOrbitUtils.Deg2Rad;
        this.EccentricAnomaly = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(this.MeanAnomaly, this.Eccentricity);
        this.TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(this.EccentricAnomaly, this.Eccentricity);
        this.AttractorMass = attractorMass;
        this.GravConst = gConst;
        CalculateOrbitStateFromOrbitalElements();
    }

    /// <summary>
    /// Create and initialize new orbit state from orbital elements and main axis vectors.
    /// </summary>
    /// <param name="eccentricity">Eccentricity.</param>
    /// <param name="semiMajorAxis">Semi major axis vector.</param>
    /// <param name="semiMinorAxis">Semi minor axis vector.</param>
    /// <param name="meanAnomalyDeg">Mean anomaly in degrees.</param>
    /// <param name="attractorMass">Attractor mass.</param>
    /// <param name="gConst">Gravitational constant.</param>
    public KeplerOrbitData(double eccentricity, KeplerVector3d semiMajorAxis, KeplerVector3d semiMinorAxis, double meanAnomalyDeg, double attractorMass, double gConst)
    {
        this.Eccentricity = eccentricity;
        this.SemiMajorAxisBasis = semiMajorAxis.normalized;
        this.SemiMinorAxisBasis = semiMinorAxis.normalized;
        this.SemiMajorAxis = semiMajorAxis.magnitude;
        this.SemiMinorAxis = semiMinorAxis.magnitude;

        this.MeanAnomaly = meanAnomalyDeg * KeplerOrbitUtils.Deg2Rad;
        this.EccentricAnomaly = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(this.MeanAnomaly, this.Eccentricity);
        this.TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(this.EccentricAnomaly, this.Eccentricity);
        this.AttractorMass = attractorMass;
        this.GravConst = gConst;
        

        CalculateOrbitStateFromOrbitalElements();
    }

    /// <summary>
    /// Calculates full orbit state from cartesian vectors: current body position, velocity, attractor mass, and gravConstant.
    /// </summary>
    public void CalculateOrbitStateFromOrbitalVectors()
    {
        MG = AttractorMass * GravConst;
        AttractorDistance = Position.magnitude;
        KeplerVector3d angularMomentumVector = KeplerVector3d.Cross(Position, Velocity);
        OrbitNormal = angularMomentumVector.normalized;
        KeplerVector3d eccVector;
        if (OrbitNormal.sqrMagnitude < 0.99)
        {
            OrbitNormal = KeplerVector3d.Cross(Position, EclipticUp).normalized;
            eccVector = new KeplerVector3d();
        }
        else
        {
            eccVector = KeplerVector3d.Cross(Velocity, angularMomentumVector) / MG - Position / AttractorDistance;
        }

        OrbitNormalDotEclipticNormal = KeplerVector3d.Dot(OrbitNormal, EclipticNormal);
        FocalParameter = angularMomentumVector.sqrMagnitude / MG;
        Eccentricity = eccVector.magnitude;
        SemiMinorAxisBasis = KeplerVector3d.Cross(angularMomentumVector, -eccVector).normalized;
        if (SemiMinorAxisBasis.sqrMagnitude < 0.99)
        {
            SemiMinorAxisBasis = KeplerVector3d.Cross(OrbitNormal, Position).normalized;
        }

        SemiMajorAxisBasis = KeplerVector3d.Cross(OrbitNormal, SemiMinorAxisBasis).normalized;
        if (Eccentricity < 1.0)
        {
            OrbitCompressionRatio = 1 - Eccentricity * Eccentricity;
            SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
            CenterPoint = -SemiMajorAxis * eccVector;
            var p = Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / MG);
            Period = KeplerOrbitUtils.PI_2 * p;
            MeanMotion = 1d / p;
            Apoapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            Periapsis = CenterPoint + SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = Apoapsis.magnitude;
            TrueAnomaly = KeplerVector3d.Angle(Position, SemiMajorAxisBasis) * KeplerOrbitUtils.Deg2Rad;
            if (KeplerVector3d.Dot(KeplerVector3d.Cross(Position, -SemiMajorAxisBasis), OrbitNormal) < 0)
            {
                TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
            }

            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
        }
        else if (Eccentricity > 1.0)
        {
            OrbitCompressionRatio = Eccentricity * Eccentricity - 1;
            SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
            CenterPoint = SemiMajorAxis * eccVector;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(SemiMajorAxis, 3));
            Apoapsis = new KeplerVector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Periapsis = CenterPoint - SemiMajorAxisBasis * (SemiMajorAxis);
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = double.PositiveInfinity;
            TrueAnomaly = KeplerVector3d.Angle(Position, eccVector) * KeplerOrbitUtils.Deg2Rad;
            if (KeplerVector3d.Dot(KeplerVector3d.Cross(Position, -SemiMajorAxisBasis), OrbitNormal) < 0)
            {
                TrueAnomaly = -TrueAnomaly;
            }

            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
        }
        else
        {
            OrbitCompressionRatio = 0;
            SemiMajorAxis = 0;
            SemiMinorAxis = 0;
            PeriapsisDistance = angularMomentumVector.sqrMagnitude / MG;
            CenterPoint = new KeplerVector3d();
            Periapsis = -PeriapsisDistance * SemiMinorAxisBasis;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(PeriapsisDistance, 3));
            Apoapsis = new KeplerVector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            ApoapsisDistance = double.PositiveInfinity;
            TrueAnomaly = KeplerVector3d.Angle(Position, eccVector) * KeplerOrbitUtils.Deg2Rad;
            if (KeplerVector3d.Dot(KeplerVector3d.Cross(Position, -SemiMajorAxisBasis), OrbitNormal) < 0)
            {
                TrueAnomaly = -TrueAnomaly;
            }

            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
        }
    }

    /// <summary>
    /// Calculates the full state of orbit from current orbital elements: eccentricity, mean anomaly, semi major and semi minor axis.
    /// </summary>
    /// <remarks>
    /// Update orbital state using known main orbital elements and basis axis vectors.
    /// Can be used for first initialization of orbit state, in this case initial data must be filled before this method call.
    /// Required initial data: eccentricity, mean anomaly, inclination, attractor mass, grav constant, all anomalies, semi minor and semi major axis vectors and magnitudes.
    /// Note that semi minor and semi major axis must be fully precalculated from inclination and argument of periapsis or another source data;
    /// </remarks>
    public void CalculateOrbitStateFromOrbitalElements()
    {
        MG = AttractorMass * GravConst;
        OrbitNormal = -KeplerVector3d.Cross(SemiMajorAxisBasis, SemiMinorAxisBasis).normalized;
        OrbitNormalDotEclipticNormal = KeplerVector3d.Dot(OrbitNormal, EclipticNormal);
        if (Eccentricity < 1.0)
        {
            OrbitCompressionRatio = 1 - Eccentricity * Eccentricity;
            CenterPoint = -SemiMajorAxisBasis * SemiMajorAxis * Eccentricity;
            //Period = KeplerOrbitUtils.PI_2 * Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / MG);
           // UnityEngine.Debug.Log($"Period b: {Period}, SemiMajorAxis: {SemiMajorAxis}, MG: {MG}");

            MeanMotion = KeplerOrbitUtils.PI_2 / Period;
            Apoapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            Periapsis = CenterPoint + SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = Apoapsis.magnitude;
            // All anomalies state already preset.
        }
        else if (Eccentricity > 1.0)
        {
            CenterPoint = SemiMajorAxisBasis * SemiMajorAxis * Eccentricity;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(SemiMajorAxis, 3));
            Apoapsis = new KeplerVector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Periapsis = CenterPoint - SemiMajorAxisBasis * (SemiMajorAxis);
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = double.PositiveInfinity;
        }
        else
        {
            CenterPoint = new KeplerVector3d();
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG * 0.5 / Math.Pow(PeriapsisDistance, 3));
            Apoapsis = new KeplerVector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            PeriapsisDistance = SemiMajorAxis;
            SemiMajorAxis = 0;
            Periapsis = -PeriapsisDistance * SemiMajorAxisBasis;
            ApoapsisDistance = double.PositiveInfinity;
        }

        Position = GetFocalPositionAtEccentricAnomaly(EccentricAnomaly);
        double compresion = Eccentricity < 1 ? (1 - Eccentricity * Eccentricity) : (Eccentricity * Eccentricity - 1);
        FocalParameter = SemiMajorAxis * compresion;
        Velocity = GetVelocityAtTrueAnomaly(this.TrueAnomaly);
        AttractorDistance = Position.magnitude;
    }

    /// <summary>
    /// Gets the velocity vector value at eccentric anomaly.
    /// </summary>
    /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
    /// <returns>Velocity vector.</returns>
    public KeplerVector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
    {
        return GetVelocityAtTrueAnomaly(KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, Eccentricity));
    }

    /// <summary>
    /// Gets the velocity value at true anomaly.
    /// </summary>
    /// <param name="trueAnomaly">The true anomaly.</param>
    /// <returns>Velocity vector.</returns>
    public KeplerVector3d GetVelocityAtTrueAnomaly(double trueAnomaly)
    {
        if (FocalParameter <= 0)
        {
            return new KeplerVector3d();
        }

        double sqrtMGdivP = Math.Sqrt(AttractorMass * GravConst / FocalParameter);
        double vX = sqrtMGdivP * (Eccentricity + Math.Cos(trueAnomaly));
        double vY = sqrtMGdivP * Math.Sin(trueAnomaly);
        return -SemiMinorAxisBasis * vX - SemiMajorAxisBasis * vY;
    }

    /// <summary>
    /// Gets the central position at true anomaly.
    /// </summary>
    /// <param name="trueAnomaly">The true anomaly.</param>
    /// <returns>Position relative to orbit center.</returns>
    /// <remarks>
    /// Note: central position is not same as focal position.
    /// </remarks>
    public KeplerVector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly)
    {
        double ecc = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, Eccentricity);
        return GetCentralPositionAtEccentricAnomaly(ecc);
    }

    /// <summary>
    /// Gets the central position at eccentric anomaly.
    /// </summary>
    /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
    /// <returns>Position relative to orbit center.</returns>
    /// <remarks>
    /// Note: central position is not same as focal position.
    /// </remarks>
    public KeplerVector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly)
    {
        if (Eccentricity < 1.0)
        {
            KeplerVector3d result = new KeplerVector3d(Math.Sin(eccentricAnomaly) * SemiMinorAxis, -Math.Cos(eccentricAnomaly) * SemiMajorAxis);
            return -SemiMinorAxisBasis * result.x - SemiMajorAxisBasis * result.y;
        }
        else if (Eccentricity > 1.0)
        {
            KeplerVector3d result = new KeplerVector3d(Math.Sinh(eccentricAnomaly) * SemiMinorAxis, Math.Cosh(eccentricAnomaly) * SemiMajorAxis);
            return -SemiMinorAxisBasis * result.x - SemiMajorAxisBasis * result.y;
        }
        else
        {
            var pos = new KeplerVector3d(
                PeriapsisDistance * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly)),
                PeriapsisDistance * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly)));
            return -SemiMinorAxisBasis * pos.x + SemiMajorAxisBasis * pos.y;
        }
    }

    /// <summary>
    /// Gets the focal position at eccentric anomaly.
    /// </summary>
    /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
    /// <returns>Position relative to attractor (focus).</returns>
    public KeplerVector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly)
    {
        return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + CenterPoint;
    }

    /// <summary>
    /// Gets the focal position at true anomaly.
    /// </summary>
    /// <param name="trueAnomaly">The true anomaly.</param>
    /// <returns>Position relative to attractor (focus).</returns>
    public KeplerVector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly)
    {
        return GetCentralPositionAtTrueAnomaly(trueAnomaly) + CenterPoint;
    }

    /// <summary>
    /// Gets the central position.
    /// </summary>
    /// <returns>Position relative to orbit center.</returns>
    /// <remarks>
    /// Note: central position is not same as focal position.
    /// Orbit center point is geometric center of orbit ellipse, which is the further from attractor (Focus point) the larger eccentricity is.
    /// </remarks>
    public KeplerVector3d GetCentralPosition()
    {
        return Position - CenterPoint;
    }

    /// <summary>
    /// Gets orbit sample points with defined precision.
    /// </summary>
    /// <param name="pointsCount">The points count.</param>
    /// <param name="maxDistance">The maximum distance of points.</param>
    /// <returns>Array of orbit curve points.</returns>
    public KeplerVector3d[] GetOrbitPoints(int pointsCount = 50, double maxDistance = 1000d)
    {
        return GetOrbitPoints(pointsCount, new KeplerVector3d(), maxDistance);
    }

    /// <summary>
    /// Gets orbit sample points with defined precision.
    /// </summary>
    /// <param name="pointsCount">The points count.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="maxDistance">The maximum distance.</param>
    /// <returns>Array of orbit curve points.</returns>
    public KeplerVector3d[] GetOrbitPoints(int pointsCount, KeplerVector3d origin, double maxDistance = 1000d)
    {
        if (pointsCount < 2)
        {
            return new KeplerVector3d[0];
        }

        KeplerVector3d[] result = new KeplerVector3d[pointsCount];
        if (Eccentricity < 1.0)
        {
            if (ApoapsisDistance < maxDistance)
            {
                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                }
            }
            else if (Eccentricity > 1.0)
            {
                double maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis, PeriapsisDistance);
                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
            else
            {
                double maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, PeriapsisDistance, PeriapsisDistance);
                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
        }
        else
        {
            if (maxDistance < PeriapsisDistance)
            {
                return new KeplerVector3d[0];
            }

            double maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis, PeriapsisDistance);

            for (int i = 0; i < pointsCount; i++)
            {
                result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the orbit sample points without unnecessary memory alloc for resulting array.
    /// However, memory allocation may occur if resulting array has not correct length.
    /// </summary>
    /// <param name="orbitPoints">The orbit points.</param>
    /// <param name="pointsCount">The points count.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="maxDistance">The maximum distance.</param>
    public void GetOrbitPointsNoAlloc(ref KeplerVector3d[] orbitPoints, int pointsCount, KeplerVector3d origin, float maxDistance = 1000f)
    {
        if (pointsCount < 2)
        {
            orbitPoints = new KeplerVector3d[0];
            return;
        }

        if (Eccentricity < 1)
        {
            if (orbitPoints == null || orbitPoints.Length != pointsCount)
            {
                orbitPoints = new KeplerVector3d[pointsCount];
            }

            if (ApoapsisDistance < maxDistance)
            {
                for (int i = 0; i < pointsCount; i++)
                {
                    orbitPoints[i] = GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                }
            }
            else
            {
                double maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis, PeriapsisDistance);
                for (int i = 0; i < pointsCount; i++)
                {
                    orbitPoints[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
        }
        else
        {
            if (maxDistance < PeriapsisDistance)
            {
                orbitPoints = new KeplerVector3d[0];
                return;
            }

            if (orbitPoints == null || orbitPoints.Length != pointsCount)
            {
                orbitPoints = new KeplerVector3d[pointsCount];
            }

            double maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis, PeriapsisDistance);

            for (int i = 0; i < pointsCount; i++)
            {
                orbitPoints[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
            }
        }
    }

    /// <summary>
    /// Gets the ascending node of orbit.
    /// </summary>
    /// <param name="asc">The asc.</param>
    /// <returns><c>true</c> if ascending node exists, otherwise <c>false</c></returns>
    public bool GetAscendingNode(out KeplerVector3d asc)
    {
        KeplerVector3d ascNodeDir = KeplerVector3d.Cross(OrbitNormal, EclipticNormal);
        bool s = KeplerVector3d.Dot(KeplerVector3d.Cross(ascNodeDir, SemiMajorAxisBasis), OrbitNormal) >= 0;
        double trueAnom = KeplerVector3d.Angle(ascNodeDir, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
        double ecc;
        if (Eccentricity < 1.0)
        {
            double cosT = Math.Cos(trueAnom);
            ecc = Math.Acos((Eccentricity + cosT) / (1d + Eccentricity * cosT));
            if (!s)
            {
                ecc = KeplerOrbitUtils.PI_2 - ecc;
            }
        }
        else if (Eccentricity > 1.0)
        {
            trueAnom = KeplerVector3d.Angle(-ascNodeDir, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
            if (trueAnom >= Math.Acos(-1d / Eccentricity))
            {
                asc = new KeplerVector3d();
                return false;
            }

            double cosT = Math.Cos(trueAnom);
            ecc = KeplerOrbitUtils.Acosh((Eccentricity + cosT) / (1 + Eccentricity * cosT)) * (!s ? -1 : 1);
        }
        else
        {
            asc = new KeplerVector3d();
            return false;
        }

        asc = GetFocalPositionAtEccentricAnomaly(ecc);
        return true;
    }

    /// <summary>
    /// Gets the descending node orbit.
    /// </summary>
    /// <param name="desc">The desc.</param>
    /// <returns><c>true</c> if descending node exists, otherwise <c>false</c></returns>
    public bool GetDescendingNode(out KeplerVector3d desc)
    {
        KeplerVector3d norm = KeplerVector3d.Cross(OrbitNormal, EclipticNormal);
        bool s = KeplerVector3d.Dot(KeplerVector3d.Cross(norm, SemiMajorAxisBasis), OrbitNormal) < 0;
        double trueAnom = KeplerVector3d.Angle(norm, -CenterPoint) * KeplerOrbitUtils.Deg2Rad;
        double ecc;
        if (Eccentricity < 1.0)
        {
            double cosT = Math.Cos(trueAnom);
            ecc = Math.Acos((Eccentricity + cosT) / (1d + Eccentricity * cosT));
            if (s)
            {
                ecc = KeplerOrbitUtils.PI_2 - ecc;
            }
        }
        else if (Eccentricity > 1.0)
        {
            trueAnom = KeplerVector3d.Angle(norm, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
            if (trueAnom >= Math.Acos(-1d / Eccentricity))
            {
                desc = new KeplerVector3d();
                return false;
            }

            double cosT = Math.Cos(trueAnom);
            ecc = KeplerOrbitUtils.Acosh((Eccentricity + cosT) / (1 + Eccentricity * cosT)) * (s ? -1 : 1);
        }
        else
        {
            desc = new KeplerVector3d();
            return false;
        }

        desc = GetFocalPositionAtEccentricAnomaly(ecc);
        return true;
    }

    /// <summary>
    /// Updates the kepler orbit dynamic state (anomalies, position, velocity) by defined deltatime.
    /// </summary>
    /// <param name="deltaTime">The delta time.</param>
    public void UpdateOrbitDataByTime(double deltaTime)
    {
        UpdateOrbitAnomaliesByTime(deltaTime);
        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    /// <summary>
    /// Updates the value of orbital anomalies by defined deltatime.
    /// </summary>
    /// <param name="deltaTime">The delta time.</param>
    /// <remarks>
    /// Only anomalies values will be changed. 
    /// Position and velocity states needs to be updated too after this method call.
    /// </remarks>
    public void UpdateOrbitAnomaliesByTime(double deltaTime)
    {
        if (Eccentricity < 1.0)
        {
            MeanAnomaly += MeanMotion * deltaTime;
            MeanAnomaly %= KeplerOrbitUtils.PI_2;

            if (MeanAnomaly < 0)
            {
                MeanAnomaly = KeplerOrbitUtils.PI_2 - MeanAnomaly;
            }

            EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
            double cosE = Math.Cos(EccentricAnomaly);
            TrueAnomaly = Math.Acos((cosE - Eccentricity) / (1 - Eccentricity * cosE));
            if (MeanAnomaly > Math.PI)
            {
                TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
            }
        }
        else if (Eccentricity > 1.0)
        {
            MeanAnomaly += MeanMotion * deltaTime;
            EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
            TrueAnomaly = Math.Atan2(Math.Sqrt(Eccentricity * Eccentricity - 1.0) * Math.Sinh(EccentricAnomaly), Eccentricity - Math.Cosh(EccentricAnomaly));
        }
        else
        {
            MeanAnomaly += MeanMotion * deltaTime;
            EccentricAnomaly = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
            TrueAnomaly = EccentricAnomaly;
        }
    }

    /// <summary>
    /// Get orbit time for elliption orbits at current anomaly. Result is in range [0..Period].
    /// </summary>
    /// <returns>Time, corresponding to current anomaly</returns>
    public double GetCurrentOrbitTime()
    {
        if (Eccentricity < 1.0)
        {
            if (Period > 0 && Period < double.PositiveInfinity)
            {
                var anomaly = MeanAnomaly % KeplerOrbitUtils.PI_2;
                if (anomaly < 0)
                {
                    anomaly = KeplerOrbitUtils.PI_2 - anomaly;
                }

                return anomaly / KeplerOrbitUtils.PI_2 * Period;
            }

            return 0.0;
        }
        else if (Eccentricity > 1.0)
        {
            var meanMotion = MeanMotion;
            if (meanMotion > 0)
            {
                return MeanAnomaly / meanMotion;
            }

            return 0.0;
        }
        else
        {
            var meanMotion = MeanMotion;
            if (meanMotion > 0)
            {
                return MeanAnomaly / meanMotion;
            }

            return 0.0;
        }
    }

    /// <summary>
    /// Updates position from anomaly state.
    /// </summary>
    public void SetPositionByCurrentAnomaly()
    {
        Position = GetFocalPositionAtEccentricAnomaly(EccentricAnomaly);
    }

    /// <summary>
    /// Sets orbit velocity, calculated by current anomaly.
    /// </summary>
    public void SetVelocityByCurrentAnomaly()
    {
        Velocity = GetVelocityAtEccentricAnomaly(EccentricAnomaly);
    }

    /// <summary>
    /// Sets the eccentricity and updates all corresponding orbit state values.
    /// </summary>
    /// <param name="e">The new eccentricity value.</param>
    public void SetEccentricity(double e)
    {
        if (!IsValidOrbit)
        {
            return;
        }

        e = Math.Abs(e);
        double periapsis = PeriapsisDistance; // Periapsis remains constant
        Eccentricity = e;
        double compresion = Eccentricity < 1 ? (1 - Eccentricity * Eccentricity) : (Eccentricity * Eccentricity - 1);
        SemiMajorAxis = Math.Abs(periapsis / (1 - Eccentricity));
        FocalParameter = SemiMajorAxis * compresion;
        SemiMinorAxis = SemiMajorAxis * Math.Sqrt(compresion);
        CenterPoint = SemiMajorAxis * Math.Abs(Eccentricity) * SemiMajorAxisBasis;
        if (Eccentricity < 1.0)
        {
            EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
            double cosE = Math.Cos(EccentricAnomaly);
            TrueAnomaly = Math.Acos((cosE - Eccentricity) / (1 - Eccentricity * cosE));
            if (MeanAnomaly > Math.PI)
            {
                TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
            }
        }
        else if (Eccentricity > 1.0)
        {
            EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
            TrueAnomaly = Math.Atan2(Math.Sqrt(Eccentricity * Eccentricity - 1) * Math.Sinh(EccentricAnomaly), Eccentricity - Math.Cosh(EccentricAnomaly));
        }
        else
        {
            EccentricAnomaly = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
            TrueAnomaly = EccentricAnomaly;
        }

        SetVelocityByCurrentAnomaly();
        SetPositionByCurrentAnomaly();

        CalculateOrbitStateFromOrbitalVectors();
    }

    /// <summary>
    /// Sets the mean anomaly and updates all other anomalies.
    /// </summary>
    /// <param name="m">The m.</param>
    public void SetMeanAnomaly(double m)
    {
        if (!IsValidOrbit)
        {
            return;
        }

        MeanAnomaly = m % KeplerOrbitUtils.PI_2;
        if (Eccentricity < 1.0)
        {
            if (MeanAnomaly < 0)
            {
                MeanAnomaly += KeplerOrbitUtils.PI_2;
            }

            EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
            TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
        }
        else if (Eccentricity > 1.0)
        {
            EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
            TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
        }
        else
        {
            EccentricAnomaly = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
            TrueAnomaly = EccentricAnomaly;
        }

        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    /// <summary>
    /// Sets the true anomaly and updates all other anomalies.
    /// </summary>
    /// <param name="t">The t.</param>
    public void SetTrueAnomaly(double t)
    {
        if (!IsValidOrbit)
        {
            return;
        }

        t %= KeplerOrbitUtils.PI_2;

        if (Eccentricity < 1.0)
        {
            if (t < 0)
            {
                t += KeplerOrbitUtils.PI_2;
            }

            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, Eccentricity);
            MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
        }
        else if (Eccentricity > 1.0)
        {
            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, Eccentricity);
            MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
        }
        else
        {
            EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, Eccentricity);
            MeanAnomaly = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(EccentricAnomaly, Eccentricity);
        }

        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    /// <summary>
    /// Sets the eccentric anomaly and updates all other anomalies.
    /// </summary>
    /// <param name="e">The e.</param>
    public void SetEccentricAnomaly(double e)
    {
        if (!IsValidOrbit)
        {
            return;
        }

        e %= KeplerOrbitUtils.PI_2;

        EccentricAnomaly = e;

        if (Eccentricity < 1.0)
        {
            if (e < 0)
            {
                e = KeplerOrbitUtils.PI_2 + e;
            }

            EccentricAnomaly = e;
            TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, Eccentricity);
            MeanAnomaly = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(e, Eccentricity);
        }
        else if (Eccentricity > 1.0)
        {
            TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, Eccentricity);
            MeanAnomaly = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(e, Eccentricity);
        }
        else
        {
            TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, Eccentricity);
            MeanAnomaly = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(e, Eccentricity);
        }

        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public KeplerOrbitData CloneOrbit()
    {
        return (KeplerOrbitData)MemberwiseClone();
    }

    public static implicit operator KeplerOrbitData(float v)
    {
        throw new NotImplementedException();
    }
}
