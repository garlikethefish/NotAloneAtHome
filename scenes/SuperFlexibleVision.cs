using Godot;
using System;
using System.Collections.Generic;

public enum VisionMode { Circle, Cone }

[Tool]
public partial class SuperFlexibleVision : Polygon2D
{
    [Export] private CollisionObject2D[] excludedColliders = [];
    [Export] public VisionMode Mode = VisionMode.Circle;
    [Export] private float _reach = 150f;
    [Export] private int _rayCount = 60; // Increased slightly to accommodate side rays smoothly
    [Export] private int[] collisionMasks = [];
    [Export] private float _coneAngle = 45f;
    [Export] private float _backRange = 10f;

    private Vector2[] _targetPolyPoints = [];
    private RidArray _cachedExcludedRids = [];

    public override void _Ready() 
    {
        CacheExcludedRids();
    }

    public override void _Process(double delta)
    {
        Vector2[] polyPoints = [];
        if (Mode is VisionMode.Circle)
        {
            polyPoints = CircleVisionPolygon(_rayCount, _reach);
        }
        else if (Mode is VisionMode.Cone)
        {
            polyPoints = ConeVisionPolygon(_coneAngle, _reach, _backRange, _rayCount);
        }

        if (polyPoints.Length == 0) return;

        if (_cachedExcludedRids.Count != excludedColliders.Length)
        {
            CacheExcludedRids();
        }

        var combinedMasks = Utils.GetCombinedMask(collisionMasks);
        _targetPolyPoints = AdjustPointsWithCasts(polyPoints, combinedMasks, _cachedExcludedRids);
        Polygon = _targetPolyPoints;
    }

    private void CacheExcludedRids()
    {
        _cachedExcludedRids = new RidArray();
        foreach (var collider in excludedColliders)
        {
            if (IsInstanceValid(collider))
            {
                _cachedExcludedRids.Add(collider.GetRid());
            }
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

    Vector2[] ConeVisionPolygon(float visionAngle, float visionRange, float backRange, int totalSegments)
    {
        Vector2 mouseLocal = GetLocalMousePosition();
        if (mouseLocal.Length() < 0.001f) return [];

        if (backRange >= visionRange) 
            return CircleVisionPolygon(totalSegments, visionRange);

        float mouseAngle = mouseLocal.Angle();
        float halfCone = Mathf.DegToRad(visionAngle);
        
        float beta = Mathf.Acos(backRange / visionRange);
        float totalBackArc = Mathf.Tau - (2f * halfCone) - (2f * beta);

        if (totalBackArc <= 0f) 
            return CircleVisionPolygon(totalSegments, visionRange);

        var points = new List<Vector2>();

        // Dynamically distribute rays across the 4 sections of the shape
        int frontSegments = Mathf.Max(4, (int)(totalSegments * 0.4f));
        int backSegments = Mathf.Max(4, (int)(totalSegments * 0.4f));
        int flankSegments = Mathf.Max(2, (int)(totalSegments * 0.1f));

        // 1. Front Vision Arc
        for (int i = 0; i <= frontSegments; i++)
        {
            float t = (float)i / frontSegments;
            float angle = (mouseAngle - halfCone) + t * (2f * halfCone);
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * visionRange);
        }

        // 2. Right Side Flank (Subdivided straight line so raycasts check it)
        Vector2 flank1Start = points[points.Count - 1];
        float backArcStartAngle = mouseAngle + halfCone + beta;
        Vector2 flank1End = new Vector2(Mathf.Cos(backArcStartAngle), Mathf.Sin(backArcStartAngle)) * backRange;
        for (int i = 1; i < flankSegments; i++)
        {
            float t = (float)i / flankSegments;
            points.Add(flank1Start.Lerp(flank1End, t));
        }

        // 3. Back Awareness Arc
        for (int i = 0; i <= backSegments; i++)
        {
            float t = (float)i / backSegments;
            float angle = backArcStartAngle + t * totalBackArc;
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * backRange);
        }

        // 4. Left Side Flank (Subdivided straight line)
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

    Vector2[] AdjustPointsWithCasts(Vector2[] points, uint collisionMasks, RidArray excludedColliderRids)
    {
        var adjustedPoints = new Vector2[points.Length];
        var spaceState = GetWorld2D().DirectSpaceState;

        for (int i = 0; i < points.Length; i++)
        {
            adjustedPoints[i] = RaycastAdjustedVector(
                spaceState,
                GlobalPosition,
                ToGlobal(points[i]),
                collisionMasks,
                excludedColliderRids
            );
        }
        return adjustedPoints;
    }

    Vector2 RaycastAdjustedVector(
        PhysicsDirectSpaceState2D spaceState,
        Vector2 rayStart,
        Vector2 rayEnd,
        uint collisionMasks,
        RidArray excludedColliderRids)
    {
        var query = PhysicsRayQueryParameters2D.Create(rayStart, rayEnd, collisionMasks, excludedColliderRids);
        query.HitFromInside = true;

        var result = spaceState.IntersectRay(query);
        return ToLocal(result.Count > 0 ? result["position"].AsVector2() : rayEnd);
    }
}