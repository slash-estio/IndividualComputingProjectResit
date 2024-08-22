using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    // This is used quite commonly to avoid issues with floating point precision.
    public static Vector3Int ToInt(this Vector3 source)
    {
        return new Vector3Int(
            Mathf.RoundToInt(source.x),
            Mathf.RoundToInt(source.y),
            Mathf.RoundToInt(source.z)
        );
    }

    // This is meant to be used on a sum of 2 vectors.
    public static Vector3 AvgPoint(this Vector3 source)
    {
        return new Vector3((source.x) / 2, (source.y) / 2, (source.z) / 2);
    }

    public static Vector3 Direction(this Vector3 source)
    {
        return Vector3.zero
            + new Vector3(
                source.x != 0 ? Math.Abs(source.x) / source.x : 0,
                source.y != 0 ? Math.Abs(source.y) / source.y : 0,
                source.z != 0 ? Math.Abs(source.z) / source.z : 0
            );
    }
}
