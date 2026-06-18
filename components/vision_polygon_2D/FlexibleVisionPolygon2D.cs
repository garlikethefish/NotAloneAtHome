using Godot;
using System;
using System.Collections.Generic;

public enum VisionMode { Circle, Cone }

[Tool]
public partial class FlexibleVisionPolygon2D : Polygon2D
{
    [Export] private CollisionObject2D[] excludedColliders = [];
    [Export] public VisionMode Mode = VisionMode.Circle;
    [Export] public float Reach = 150f;
    [Export] public float ConeAngle = 45f;
    [Export] public float BackRange = 10f;
    [Export] private int _rayCount = 90;
    [Export] private int[] collisionMasks = [];
    [Export] private float _lerpSpeed = 20f;
    [Export] private int _raycastFps = 30; // raycasts per second, decoupled from render fps
    private bool _showInEditor = false;
    [Export] private bool ShowInEditorToggle
    {
        get => _showInEditor;
        set
        {
            _showInEditor = value;
            if (!_showInEditor && Engine.IsEditorHint())
                Polygon = [];
        }
    }

    private Vector2[] _targetPolyPoints = [];
    private Vector2[] _lastRaycastPoints = [];
    private bool[] _lastPointHitWall = [];
    private RidArray _cachedExcludedRids = [];
    private uint _cachedCollisionMask;
    private double _raycastTimer = 0;

    public override void _Ready()
    {
        _cachedCollisionMask = Utils.GetCombinedMask(collisionMasks);
        CacheExcludedRids();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && !_showInEditor) return;

        if (!Engine.IsEditorHint() && Input.IsActionJustPressed("mouse_left"))
            Mode = Mode is VisionMode.Circle ? VisionMode.Cone : VisionMode.Circle;

        // --- Raycast tick (throttled) ---
        _raycastTimer += delta;
        double raycastInterval = 1.0 / _raycastFps;

        if (_raycastTimer >= raycastInterval || _lastRaycastPoints.Length == 0)
        {
            _raycastTimer = 0;

            Vector2[] polyPoints = Mode switch
            {
                VisionMode.Circle => CircleVisionPolygon(_rayCount, Reach),
                VisionMode.Cone   => ConeVisionPolygon(ConeAngle, Reach, BackRange, _rayCount),
                _                 => []
            };

            if (polyPoints.Length == 0) return;

            if (_cachedExcludedRids.Count != excludedColliders.Length)
                CacheExcludedRids();

            var spaceState = GetWorld2D().DirectSpaceState;
            _lastRaycastPoints = AdjustPointsWithCasts(polyPoints, spaceState);
        }

        if (_lastRaycastPoints.Length == 0) return;

        // --- Visual update (every frame) ---
        if (_lerpSpeed > 0)
        {
            _targetPolyPoints = PrepareForTransition(_lastRaycastPoints, _targetPolyPoints);
            for (int i = 0; i < _targetPolyPoints.Length; i++)
            {
                // Wall hits snap instantly, open air lerps smoothly
                if (_lastPointHitWall.Length > i && _lastPointHitWall[i])
                    _targetPolyPoints[i] = _lastRaycastPoints[i];
                else
                    _targetPolyPoints[i] = _targetPolyPoints[i].Lerp(_lastRaycastPoints[i], (float)delta * _lerpSpeed);
            }
        }
        else
        {
            _targetPolyPoints = _lastRaycastPoints;
        }

        Polygon = _targetPolyPoints;
    }

    private void CacheExcludedRids()
    {
        _cachedExcludedRids = [];
        foreach (var collider in excludedColliders)
        {
            if (IsInstanceValid(collider))
                _cachedExcludedRids.Add(collider.GetRid());
        }
    }

    Vector2[] CircleVisionPolygon(int pointCount, float range)
    {
        var points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float angle = 2f * Mathf.Pi * i / pointCount;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * range;
        }
        return points;
    }

    Vector2[] ConeVisionPolygon(float visionAngle, float visionRange, float backRange, int pointCount)
    {
        Vector2 mouseLocal = GetLocalMousePosition();
        if (mouseLocal.Length() < 0.001f) return [];
        if (backRange >= visionRange) return CircleVisionPolygon(pointCount, visionRange);

        float mouseAngle = mouseLocal.Angle();
        float halfCone = Mathf.DegToRad(visionAngle);
        float beta = Mathf.Acos(backRange / visionRange);
        float totalBackArc = Mathf.Tau - (2f * halfCone) - (2f * beta);

        if (totalBackArc <= 0f) return CircleVisionPolygon(pointCount, visionRange);

        var points = new List<Vector2>();

        int frontSegments = Mathf.Max(4, (int)(pointCount * 0.4f));
        int backSegments  = Mathf.Max(4, (int)(pointCount * 0.4f));
        int flankSegments = Mathf.Max(2, (int)(pointCount * 0.1f));

        for (int i = 0; i <= frontSegments; i++)
        {
            float t = (float)i / frontSegments;
            float angle = (mouseAngle - halfCone) + t * (2f * halfCone);
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * visionRange);
        }

        Vector2 flank1Start = points[points.Count - 1];
        float backArcStartAngle = mouseAngle + halfCone + beta;
        Vector2 flank1End = new Vector2(Mathf.Cos(backArcStartAngle), Mathf.Sin(backArcStartAngle)) * backRange;
        for (int i = 1; i < flankSegments; i++)
        {
            float t = (float)i / flankSegments;
            points.Add(flank1Start.Lerp(flank1End, t));
        }

        for (int i = 0; i <= backSegments; i++)
        {
            float t = (float)i / backSegments;
            float angle = backArcStartAngle + t * totalBackArc;
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * backRange);
        }

        Vector2 flank2Start = points[points.Count - 1];
        float frontArcStartAngle = mouseAngle - halfCone;
        Vector2 flank2End = new Vector2(Mathf.Cos(frontArcStartAngle), Mathf.Sin(frontArcStartAngle)) * visionRange;
        for (int i = 1; i < flankSegments; i++)
        {
            float t = (float)i / flankSegments;
            points.Add(flank2Start.Lerp(flank2End, t));
        }

        return points.ToArray();
    }

    Vector2[] AdjustPointsWithCasts(Vector2[] points, PhysicsDirectSpaceState2D spaceState)
    {
        var query = PhysicsRayQueryParameters2D.Create(
            Vector2.Zero, Vector2.Zero, _cachedCollisionMask, _cachedExcludedRids);
        query.HitFromInside = true;

        var adjustedPoints = new Vector2[points.Length];
        _lastPointHitWall = new bool[points.Length];
        Vector2 origin = GlobalPosition;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 target = ToGlobal(points[i]);
            query.From = origin;
            query.To   = target;

            var result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                adjustedPoints[i] = ToLocal(result["position"].AsVector2());
                _lastPointHitWall[i] = true;
            }
            else
            {
                adjustedPoints[i] = points[i];
                _lastPointHitWall[i] = false;
            }
        }

        return adjustedPoints;
    }

    Vector2[] PrepareForTransition(Vector2[] currentPoints, Vector2[] targetPoints)
    {
        if (targetPoints.Length != currentPoints.Length)
        {
            int oldLength = targetPoints.Length;
            Array.Resize(ref targetPoints, currentPoints.Length);
            if (oldLength == 0)
            {
                Array.Copy(currentPoints, targetPoints, currentPoints.Length);
                return targetPoints;
            }
        }

        if (targetPoints.Length > 0)
        {
            int bestStartIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < targetPoints.Length; i++)
            {
                float dist = currentPoints[0].DistanceSquaredTo(targetPoints[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestStartIndex = i;
                }
            }

            if (bestStartIndex != 0)
            {
                Vector2[] alignedTargets = new Vector2[targetPoints.Length];
                for (int i = 0; i < targetPoints.Length; i++)
                    alignedTargets[i] = targetPoints[(i + bestStartIndex) % targetPoints.Length];
                return alignedTargets;
            }
        }

        return targetPoints;
    }
}