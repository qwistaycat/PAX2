using UnityEngine;
using System.Collections.Generic;

public class InfluencePoint : MonoBehaviour
{
    public static List<InfluencePoint> All = new();
    public float radius = 5f;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);
}
