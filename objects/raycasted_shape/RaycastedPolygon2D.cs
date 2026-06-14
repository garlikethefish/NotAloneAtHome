using Godot;

[Tool]
public partial class RaycastedPolygon2D : Polygon2D
{
    [Export] private Color _initColor = new(1, 1, 1, 1);
    [Export] private CollisionObject2D[] excludedColliders = [];
    private float _reach = 150f;
    [Export] private float Reach
    {
        get => _reach;
        set { _reach = value; ShootRays(); QueueRedraw(); }
    }

    private int _rayCount = 32;
    [Export] private int RayCount
    {
        get => _rayCount;
        set { _rayCount = value; ShootRays(); QueueRedraw(); }
    }
    [Export] private bool _refresh
    {
        get => false;
        set { ShootRays(); QueueRedraw(); }
    }
    [Export] private float startOffset = 0;
    [Export] private int[] collisionMasks = [];
    [Export] private bool _castOnlyOnce;

    public override void _Ready()
    {
        Color = _initColor;
        if (!Engine.IsEditorHint())
            ShootRays();
        else
        {
            CallDeferred(MethodName.ShootRays);
            CallDeferred(MethodName.ShootRays);
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            ShootRays();
            return;
        }
        if (_castOnlyOnce) return;
        ShootRays();
    }

    void ShootRays()
    {
        if (!IsInsideTree()) return;
        var world = GetWorld2D();
        if (world == null) return;
        var spaceState = world.DirectSpaceState;
        if (spaceState == null) return;  // can still be null in editor

        var excludedRids = new RidArray();
        foreach (var collider in excludedColliders)
            excludedRids.Add(collider.GetRid());

        uint combinedMask = Utils.GetCombinedMask(collisionMasks);
        var endPoints = new Vector2[_rayCount];

        for (int i = 0; i < _rayCount; i++)
        {
            float angle = i * (Mathf.Tau / _rayCount);
            var dir = Vector2.Right.Rotated(angle);
            var rayStart = GlobalPosition + dir * startOffset;
            var rayEnd = GlobalPosition + dir * _reach;

            var query = PhysicsRayQueryParameters2D.Create(rayStart, rayEnd, combinedMask, excludedRids);
            query.HitFromInside = true;

            var result = spaceState.IntersectRay(query);
            endPoints[i] = ToLocal(result.Count > 0 ? result["position"].AsVector2() : rayEnd);
        }

        Polygon = endPoints;
    }
}