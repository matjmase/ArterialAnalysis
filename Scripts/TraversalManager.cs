using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class TraversalManager : Node3D
{
	[Export]
	private Node3D _seedPosition;
	[Export]
	private Resource _ballPrefab;
	[Export]
	private Resource _flagPrefab;
    [Export]
    private IterableAlgorithm _algorithm;
    [Export]
    private bool _displayAquisition;
    [Export]
    private bool _displayArchive;
    [Export]
    private bool _displaySplit;
    [Export]
    private bool _displayEndPoint;
    [Export]
    private ScaleModification _scale;
	[Export]
	private float __distScale = 1f;
	[Export]
	private float _timeout = 0.1f;
	[Export]
	private int _iterations = 20;
    [Export]
    private float __bridgeRadius = 5f;

    [Export]
    private int _initialSeedMax = 50;
    [Export]
    private float _epochMultiplier = 0.5f;
    [Export]
    private float _curveEmaAlpha = 0.5f;
    [Export]
    private float _curvatureCutoff = 0.5f;
    [Export]
    private float _popEmaAlpha = 0.5f;
    [Export]
    private float _PopBufferLag = 5;
    
	private float _distScale;

    // constants

    private readonly Dictionary<ScaleModification, int> _scaleDictionary = new Dictionary<ScaleModification, int>()
    {
        { ScaleModification.x1, 1 },
        { ScaleModification.x2, 2 },
        { ScaleModification.x3, 3 },
        { ScaleModification.x4, 4 },
        { ScaleModification.x5, 5 },
        { ScaleModification.x6, 6 },
    };

    // state
    private bool _isSubscribed;
	private IIteratableProcess _traversing;
    private bool _spacePause = true;

    // Keeping up with mesh instances
	private Dictionary<IIteratableProcess, TraversalManagerPointsModel> _aquisitions = new Dictionary<IIteratableProcess, TraversalManagerPointsModel>();
    private HashSet<MeshInstance3D> _archived = new HashSet<MeshInstance3D>();
    private HashSet<PositionCoord3d> _archivedPoints = new HashSet<PositionCoord3d>();

    private HashSet<MeshInstance3D> _triangles = new HashSet<MeshInstance3D>();

    public override async void _Ready()
    {
        base._Ready();

        _distScale = __distScale;
        var _bridgeRadius = __bridgeRadius / _distScale;

        await ToSignal(GetTree().CreateTimer(_timeout), "timeout");

        _traversing = AlgorithmSelector();
        Subscribe(_traversing);
        
        _traversing.Initialize(new DijkstraProcessModel()
        {
            StartPoint = new PositionCoord3d(),
            AquisitionPoints = new Dictionary<PositionCoord3d, PositionData3d>() {{ new PositionCoord3d(), new PositionData3d() }},
            ArchivedPoints = new HashSet<PositionCoord3d>(),
            IsValidNeighbor = IsValidNeighbor,
            Scale = _scaleDictionary[_scale],
            CurveEmaAlpha = _curveEmaAlpha,
            PopEmaAlpha = _popEmaAlpha,
            CurvatureCutoff = _curvatureCutoff,
            EpochMultiplier = _epochMultiplier,
            BridgeRadius = _bridgeRadius,
            InitialSeedCutoff = _initialSeedMax,
            PopBufferLag = Mathf.CeilToInt(_PopBufferLag / _epochMultiplier)
        });

        await StartCoroutineAsync(_traversing);
    }

    public override void _ExitTree()
    {
        Unsubscribe(_traversing);
        base._ExitTree();
    }

    public override void _Input(InputEvent @event)
	{
		if(@event is InputEventKey ek && ek.Pressed && ek.Keycode == Key.Space)
		{
			_spacePause = !_spacePause;
		}
	}


    private IIteratableProcess AlgorithmSelector()
    {
        switch(_algorithm)
        {
            case IterableAlgorithm.Dijkstra:
                return new Dijkstra3dAnytime();
            case IterableAlgorithm.IslandDijkstra:
                return new IslandDijkstra();
            case IterableAlgorithm.ChokeChainDijkstra:
                return new ChokeChainDijkstra();
            case IterableAlgorithm.MultiPassDijkstra:
                return new MultiPassDijkstra();
            case IterableAlgorithm.BridgeDijkstra:
                return new BridgeDijkstra();
            case IterableAlgorithm.ArterialDijkstra:
                return new ArterialDijkstra();
            default:
                throw new NotImplementedException("Need Algorithm constructor for enum - " + _algorithm.ToString());
        }
    }

    private async Task StartCoroutineAsync(IIteratableProcess process)
    {

        while(!_traversing.IsComplete)
        {
            await ToSignal(GetTree().CreateTimer(_timeout), "timeout");

			if(!_spacePause)
			{
                for(var i = 0; i < _iterations && !_traversing.IsComplete; i++)
                {
                    _traversing.Iterate();
                }

			}
        }
                
        Debug.WriteLine("Traversal is complete");
    }

    // Check function that we will DI
    private bool IsValidNeighbor(PositionCoord3d from, PositionCoord3d to)
	{
		// use global coordinates, not local to node
		var spaceState = GetWorld3D().DirectSpaceState;
		var fromVect = TransformToWorld(from.ToFloat());
		var toVect = TransformToWorld(to.ToFloat());

        // raycast both directions
		var query = PhysicsRayQueryParameters3D.Create(fromVect, toVect);
		var result1 = spaceState.IntersectRay(query);
		query = PhysicsRayQueryParameters3D.Create(toVect, fromVect);
		var result2 = spaceState.IntersectRay(query);

		return result1.Count == 0 && result2.Count == 0;
	}

    private Vector3 TransformToWorld(PositionCoord3dFloat vect)
	{
        // Using seed for offset and scale the vector
		return _seedPosition.Position + new Vector3(vect.X, vect.Y, vect.Z) * _distScale;
	}

    private Color GenerateRandomColor()
    {
        return new Color(new Random().NextSingle(), new Random().NextSingle(), new Random().NextSingle(), 1f);
    }

    private void ConfigureMeshInstance(ref MeshInstance3D instance, Vector3 location, Color color, float scale)
    {
        var material = new OrmMaterial3D
        {
            AlbedoColor = color
        };

        instance.SetSurfaceOverrideMaterial(0, material);
		instance.Scale *= scale;
		instance.Position = location;
    }

    private MeshInstance3D GenerateAndAddPrefab(Resource prefab, PositionCoord3dFloat position, Color color, float scale)
    {
        var location = TransformToWorld(position);
		var resourse = ResourceLoader.Load(prefab.ResourcePath) as PackedScene;
		var instance = resourse.Instantiate<MeshInstance3D>();

        ConfigureMeshInstance(ref instance, location, color, _distScale * scale);
        
		GetTree().Root.AddChild(instance);

        return instance;
    }

    private void DrawTriangle(Vector3 center, Vector3 vert1, Vector3 vert2, Color color)
	{
        var mesh3d = new MeshInstance3D();
        var triangleMesh = new ImmediateMesh();
        var material = new OrmMaterial3D();
        
        mesh3d.Mesh = triangleMesh;

        triangleMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, material);
        triangleMesh.SurfaceAddVertex(new Vector3(0,0,0));
        triangleMesh.SurfaceAddVertex(vert1 - center);
        triangleMesh.SurfaceAddVertex(vert2 - center);
        triangleMesh.SurfaceEnd();
        
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        material.AlbedoColor = color;
        
        mesh3d.Position = center;

        _triangles.Add(mesh3d);
        GetTree().Root.AddChild(mesh3d);
	}

    private void Subscribe(IIteratableProcess process)
    {
        process.OnTraversalCreated += OnTraversalCreated;
        process.OnTraversalDestroy += OnTraversalDestroyed;

        process.OnPointEnterAquisition += OnPointEnterAquisition;
        process.OnPointEnterArchive += OnPointArchived;
        process.OnPointLastOfAquisition += OnPointLastOfAquisition;

        process.OnHemorrhage += OnHemorrhage;
        process.OnBlockage += OnBlockage;

        // process.OnTraversalSplit += TraversalSplit;




        // process.OnTraversalReassingment += TraversalReassignment;

        // process.OnAquisitionMaxExceed += AquisitionMaxExceed;

        // process.OnTraversalsMerge += TraversalMerge;

        // process.OnBleedOut += BleedOut;
        // process.OnBlockage += Blockage;
    }

    

    private void Unsubscribe(IIteratableProcess process)
    {
        process.OnTraversalCreated -= OnTraversalCreated;
        process.OnTraversalDestroy -= OnTraversalDestroyed;

        process.OnPointEnterAquisition -= OnPointEnterAquisition;
        process.OnPointEnterArchive -= OnPointArchived;
        process.OnPointLastOfAquisition -= OnPointLastOfAquisition;
        
        process.OnHemorrhage -= OnHemorrhage;
        process.OnBlockage -= OnBlockage;

        // process.OnTraversalSplit -= TraversalSplit;

        
        // process.OnTraversalReassingment -= TraversalReassignment;

        // process.OnAquisitionMaxExceed -= AquisitionMaxExceed;

        // process.OnTraversalsMerge -= TraversalMerge;

        // process.OnBleedOut -= BleedOut;
        // process.OnBlockage -= Blockage;
    }

    private void OnTraversalDestroyed(IIteratableProcess traversal)
    {
        if(_aquisitions.ContainsKey(traversal))
        {
            foreach(var pointMesh in _aquisitions[traversal].Points)
            {
                GetTree().Root.RemoveChild(pointMesh.Value);
            }

            _aquisitions.Remove(traversal);
        }
    }

    private void OnTraversalCreated(IIteratableProcess traversal)
    {
        if(traversal is Dijkstra3dAnytime)
        {
            _aquisitions.Add(traversal, new TraversalManagerPointsModel(){
                Scale = 1.0f,
                Color = GenerateRandomColor(),
                Points = new Dictionary<PositionCoord3d, MeshInstance3D>()
            });
        }
    }

    private void OnPointArchived(IIteratableProcess traversal, PositionData3d point)
    {
        _archivedPoints.Add(point.Coord);

        if(_aquisitions.ContainsKey(traversal) && _aquisitions[traversal].Points.ContainsKey(point.Coord))
        {
            var aquistion = _aquisitions[traversal];
            var mesh = aquistion.Points[point.Coord];

            aquistion.Points.Remove(point.Coord);
            
            GetTree().Root.RemoveChild(mesh);

            // TODO: add Displayflags
            if(_displayArchive)
            {
                var instance = GenerateAndAddPrefab(_ballPrefab, point.Coord.ToFloat(), new Color(1f, 0, 0), _distScale);
                _archived.Add(instance);
            }
        }
    }

    private void OnPointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        var aquisition = _aquisitions[traversal];

        var instance = GenerateAndAddPrefab(_ballPrefab, point.Coord.ToFloat(), aquisition.Color, aquisition.Scale);
		aquisition.Points.Add(point.Coord, instance);
    }

    private void OnPointLastOfAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        if(_displayEndPoint)
        {
            GenerateAndAddPrefab(_ballPrefab, point.Coord.ToFloat(), new Color(0,0,0), 3.0f);
        }
    }

    private void TraversalSplit(HashSet<IIteratableProcess> traversals, PositionCoord3dFloat centroid)
    {
        if(_displaySplit)
        {
            GenerateAndAddPrefab(_ballPrefab, centroid, new Color(1.0f, 1.0f, 1.0f), 2.0f);
        }
    }

    private void TraversalMerge(HashSet<IIteratableProcess> traversals, IIteratableProcess traversal)
    {
    }

    private void OnBlockage(IIteratableProcess traversal, PositionCoord3dFloat location)
    {
        GenerateAndAddPrefab(_flagPrefab, location, new Color(0, 0, 1.0f), 2.0f);
    }

    private void OnHemorrhage(IIteratableProcess traversal, PositionCoord3dFloat location)
    {
        GenerateAndAddPrefab(_flagPrefab, location, new Color(1.0f, 0, 0), 2.0f);
    }
}

public enum IterableAlgorithm
{
    Dijkstra,
    IslandDijkstra,
    ChokeChainDijkstra,
    MultiPassDijkstra,
    BridgeDijkstra,
    ArterialDijkstra,
}

public enum ScaleModification
{
    x1,
    x2,
    x3,
    x4,
    x5,
    x6,
}