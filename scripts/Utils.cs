global using RidArray = Godot.Collections.Array<Godot.Rid>;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;



public static class NodeExtensions
{
    public static bool Is<T>(this Node node, out T result) where T : class
    {
        result = node as T;
        return result != null;
    }

    public static T GetComponentOfType<T>(this Node node) where T : Node
    {
        foreach (Node child in node.GetChildren())
            if (child is T match) return match;
        return null;
    }
    public static void InvokeOrLog(this Action action,  string name = "Action")
    {
        if (action == null) GD.Print($"{name} has no subscribers");
        else action();
    }

    public static void InvokeOrLog<T1>(this Action<T1> action, T1 arg1, string name = "Action")
    {
        if (action == null) GD.Print($"{name} has no subscribers");
        else action(arg1);
    }

    public static void InvokeOrLog<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2, string name = "Action")
    {
        if (action == null) GD.Print($"{name} has no subscribers");
        else action(arg1, arg2);
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
        var root = (componentNode as IComponentInterface)?.Root;
        if (root == null)
        {
            result = null;
            return false;
        }
        result = root as T;
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