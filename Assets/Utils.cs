using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    private static float FiInternal = 0f;
    public static float Fi
    {
        get
        {
            if (FiInternal == 0f)
                FiInternal = (1f + Mathf.Sqrt(5f)) / 2;
            return FiInternal;
        }
    }
}
