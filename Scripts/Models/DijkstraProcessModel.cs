using System.Collections.Generic;
using System.Linq;
using static Dijkstra3dAnytime;

public struct DijkstraProcessModel
{
    public Dictionary<PositionCoord3d, PositionData3d> AquisitionPoints;
    public HashSet<PositionCoord3d> ArchivedPoints;
    public IsValidNeighborCheckFunc IsValidNeighbor;
    public PositionCoord3d StartPoint;
    public int InitialSeedCutoff;
    public int Scale;
    public float EpochMultiplier;
    public float CurveEmaAlpha;
    public float PopEmaAlpha;
    public float CurvatureCutoff;
    public float BridgeRadius;
    public int PopBufferLag;

    public DijkstraProcessModel NewStart(Dictionary<PositionCoord3d, PositionData3d> aquisition, HashSet<PositionCoord3d> archive)
    {
        return new DijkstraProcessModel()
        {
            AquisitionPoints = aquisition,
            ArchivedPoints = archive,
            IsValidNeighbor = IsValidNeighbor,
            StartPoint = aquisition.First().Key,
            Scale = Scale,
            EpochMultiplier = EpochMultiplier,
            CurveEmaAlpha = CurveEmaAlpha,
            PopEmaAlpha = PopEmaAlpha,
            CurvatureCutoff = CurvatureCutoff,
            BridgeRadius = BridgeRadius,
            InitialSeedCutoff = InitialSeedCutoff,
            PopBufferLag = PopBufferLag,
        };
    }

    
    public DijkstraProcessModel NewStart(PositionData3d point)
    {
        return NewStart(new Dictionary<PositionCoord3d, PositionData3d>() {{ point.Coord, point }}, new HashSet<PositionCoord3d>());
    }

    public DijkstraProcessModel ChangeScale(int scale)
    {
        return new DijkstraProcessModel()
        {
            AquisitionPoints = AquisitionPoints,
            ArchivedPoints = ArchivedPoints,
            IsValidNeighbor = IsValidNeighbor,
            StartPoint = StartPoint,
            Scale = scale,
            EpochMultiplier = EpochMultiplier,
            CurveEmaAlpha = CurveEmaAlpha,
            PopEmaAlpha = PopEmaAlpha,
            CurvatureCutoff = CurvatureCutoff,
            BridgeRadius = BridgeRadius,
            InitialSeedCutoff = InitialSeedCutoff,
            PopBufferLag = PopBufferLag,
        };
    }

    public DijkstraProcessModel ChangeValiditiy(IsValidNeighborCheckFunc newFunction)
    {
        return new DijkstraProcessModel()
        {
            AquisitionPoints = AquisitionPoints,
            ArchivedPoints = ArchivedPoints,
            IsValidNeighbor = newFunction,
            StartPoint = StartPoint,
            Scale = Scale,
            EpochMultiplier = EpochMultiplier,
            CurveEmaAlpha = CurveEmaAlpha,
            PopEmaAlpha = PopEmaAlpha,
            CurvatureCutoff = CurvatureCutoff,
            BridgeRadius = BridgeRadius,
            InitialSeedCutoff = InitialSeedCutoff,
            PopBufferLag = PopBufferLag,
        };
    }

    public DijkstraProcessModel ChangeArchive(HashSet<PositionCoord3d> archive)
    {
        return new DijkstraProcessModel()
        {
            AquisitionPoints = AquisitionPoints,
            ArchivedPoints = archive,
            IsValidNeighbor = IsValidNeighbor,
            StartPoint = StartPoint,
            Scale = Scale,
            EpochMultiplier = EpochMultiplier,
            CurveEmaAlpha = CurveEmaAlpha,
            PopEmaAlpha = PopEmaAlpha,
            CurvatureCutoff = CurvatureCutoff,
            BridgeRadius = BridgeRadius,
            InitialSeedCutoff = InitialSeedCutoff,
            PopBufferLag = PopBufferLag,
        };
    }

    public DijkstraProcessModel ChangeInitialSeedCutoff(int initialSeedCutoff)
    {
        return new DijkstraProcessModel()
        {
            AquisitionPoints = AquisitionPoints,
            ArchivedPoints = ArchivedPoints,
            IsValidNeighbor = IsValidNeighbor,
            StartPoint = StartPoint,
            Scale = Scale,
            EpochMultiplier = EpochMultiplier,
            CurveEmaAlpha = CurveEmaAlpha,
            PopEmaAlpha = PopEmaAlpha,
            CurvatureCutoff = CurvatureCutoff,
            BridgeRadius = BridgeRadius,
            InitialSeedCutoff = initialSeedCutoff,
            PopBufferLag = PopBufferLag,
        };
    }
}