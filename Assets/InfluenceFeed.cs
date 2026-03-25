using UnityEngine;

public class InfluenceFeed : MonoBehaviour
{
    static readonly int PointsID = Shader.PropertyToID("_Points");
    static readonly int CountID = Shader.PropertyToID("_PointCount");

    Material mat;
    Vector4[] buffer = new Vector4[16];

    void Start() => mat = GetComponent<Renderer>().material;

    void Update()
    {
        int count = Mathf.Min(InfluencePoint.All.Count, buffer.Length);
        for (int i = 0; i < count; i++)
        {
            var p = InfluencePoint.All[i];
            buffer[i] = new Vector4(
                p.transform.position.x,
                p.transform.position.y,
                p.transform.position.z,
                p.radius
            );
        }
        mat.SetVectorArray(PointsID, buffer);
        mat.SetInt(CountID, count);
    }
}
