using Godot;

public partial class RaycastedPolygon2D : Polygon2D
{
    [Export] private Color _initColor = new Color(0, 0, 0, 0);
    [Export] private CollisionObject2D[] excludedColliders = [];
    [Export] private float reach = 150f;
    [Export] private float startOffset = 20f;
    [Export] private int rayCount = 32;
    [Export] private int[] collisionMasks = [2];

    public override void _Ready()
    {
        Color = _initColor;
    }

    public override void _Process(double delta)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var excludedRids = new Godot.Collections.Array<Rid>();
        foreach (var collider in excludedColliders)
            excludedRids.Add(collider.GetRid());

        uint combinedMask = Utils.GetCombinedMask(collisionMasks);
        var endPoints = new Vector2[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (Mathf.Tau / rayCount);
            var dir = Vector2.Right.Rotated(angle);
            var rayStart = GlobalPosition + dir * startOffset;
            var rayEnd = GlobalPosition + dir * reach;

            var query = PhysicsRayQueryParameters2D.Create(rayStart, rayEnd, combinedMask, excludedRids);
            query.HitFromInside = true;

            var result = spaceState.IntersectRay(query);
            endPoints[i] = ToLocal(result.Count > 0 ? result["position"].AsVector2() : rayEnd);
        }

        Polygon = endPoints;
    }
}