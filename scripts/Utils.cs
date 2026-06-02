global using RidArray = Godot.Collections.Array<Godot.Rid>;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;



public static class NodeExtensions
{
    public static bool HasChild<T>(this Node node) where T : Node
    {
        return node.GetChildren().OfType<T>().Any();
    }

    public static bool ParentHas<T>(this Node node) where T : Node
    {
        return ParentHas(node, out T _);
    }

    public static bool ParentHas<T>(this Node node, out T component) where T : Node
    {
        if (node is T self)
        {
            component = self;
            return true;
        }
        var parent = node.GetParent();
        if (parent == null)
        {
            component = null;
            return false;
        }

        foreach (var child in parent.GetChildren())
        {
            GD.Print($"Checking child {child.Name} of {parent.Name} for {typeof(T).Name}");
            if (child is T match)
            {
                component = match;
                return true;
            }
        }
        component = null;
        return false;
    }
}

public static class HelperExtensions
{
    public static T GetSibling<T>(this Node node) where T : Node =>
        node.GetParent().GetChildren().OfType<T>().FirstOrDefault();
}

public static class Extensions
{
    public static bool TryNextItem<T>(this List<T> list, T current, out T next)
    {
        var index = list.IndexOf(current);
        if (index >= 0 && index < list.Count - 1)
        {
            next = list[index + 1];
            return true;
        }
        next = default;
        return false;
    }

    public static bool TryPreviousItem<T>(this List<T> list, T current, out T previous)
    {
        var index = list.IndexOf(current);
        if (index > 0)
        {
            previous = list[index - 1];
            return true;
        }
        previous = default;
        return false;
    }
}

public static class Utils
{
    public static uint GetCombinedMask(IEnumerable<int> masks) =>
        (uint)masks.Aggregate(0, (combined, mask) => combined | (1 << (mask - 1)));

    public static bool TryGetRoot<T>(this Node componentNode, out T result) where T : class
    {
        // var root = (componentNode as IComponentInterface)?.Root;
        if (componentNode.GetParent() == null)
        {
            result = null;
            return false;
        }
        result = componentNode.GetParent() as T;
        return result != null;
    }
}

public static class Casters
{
    /// <summary>
    /// Takes given shape and world space center coords. Shoots rays in a circle from center to max range outwards.
    /// Does raycasting clockwise. Returns coords where shape fits.
    /// </summary>
    public static Vector2 RadialyFindAValidLocationWhichFitsGivenShape(
        PhysicsDirectSpaceState2D directSpaceState,
        CollisionShape2D shapeToCheck,
        Vector2 center,
        float maxRange,
        float stepSize,
        float startOffset,
        int rayCount,
        uint collisionMask,
        RidArray excludedRids
    )
    {
        // _debugPoints.Clear();
        // _debugRays.Clear();

        var currentBest = Vector2.Inf;

        var directions = Enumerable.Range(0, rayCount)
            .Select(i => Vector2.Right.Rotated(i * (Mathf.Tau / rayCount)))
            .ToList();

        foreach (var dir in directions)
        {
            var rayStart = center + dir * startOffset;
            var rayEnd   = center + dir * maxRange;

            var rayQuery = PhysicsRayQueryParameters2D.Create(rayStart, rayEnd, collisionMask, excludedRids);
            rayQuery.HitFromInside = true;

            var rayResult        = directSpaceState.IntersectRay(rayQuery);
            var rayTraveledDist  = rayResult.Count > 0
                ? center.DistanceTo(rayResult["position"].AsVector2())
                : maxRange;

            // _debugRays.Add((rayStart, center + dir * rayTraveledDist));

            for (float dist = startOffset; dist < rayTraveledDist; dist += stepSize)
            {
                var testPos = center + dir * dist;

                var shapeQuery = new PhysicsShapeQueryParameters2D();
                shapeQuery.Shape          = shapeToCheck.Shape;
                shapeQuery.Transform      = new Transform2D(0, testPos);
                shapeQuery.CollisionMask  = collisionMask;
                shapeQuery.Exclude        = excludedRids;

                var overlaps = directSpaceState.IntersectShape(shapeQuery);
                var isFree   = overlaps.Count == 0;

                // _debugPoints.Add((testPos, isFree));

                if (isFree && currentBest.DistanceTo(center) > testPos.DistanceTo(center))
                    currentBest = testPos;
            }
        }

        return currentBest;
    }
}