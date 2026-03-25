using UnityEngine;

public class HouseBuilder : MonoBehaviour
{
    [Header("Materials")]
    public Material wallMaterial;
    public Material roofMaterial;

    public const float wallHeight = 4.5f;
    public const float wallThickness = 0.2f;
    public const float houseWidth = 16f;
    public const float houseDepth = 20f;
    public const float doorWidth = 2f;
    public const float doorHeight = 2.8f;
    public const float roofPeak = 3.5f;
    public const float roofOverhang = 0.6f;

    const int gableSlices = 12;

    void Start()
    {
        BuildWalls();
        BuildGables();
        BuildRoof();
    }

    void BuildWalls()
    {
        float halfW = houseWidth * 0.5f;
        float halfD = houseDepth * 0.5f;
        float wy = wallHeight * 0.5f + wallThickness;

        MakeWall("Wall_Left",
            new Vector3(-halfW + wallThickness * 0.5f, wy, 0f),
            new Vector3(wallThickness, wallHeight, houseDepth));

        MakeWall("Wall_Right",
            new Vector3(halfW - wallThickness * 0.5f, wy, 0f),
            new Vector3(wallThickness, wallHeight, houseDepth));

        MakeWall("Wall_Back",
            new Vector3(0f, wy, -halfD + wallThickness * 0.5f),
            new Vector3(houseWidth, wallHeight, wallThickness));

        BuildFrontWall(halfW, halfD, wy);
    }

    void BuildFrontWall(float halfW, float halfD, float wy)
    {
        float frontZ = halfD - wallThickness * 0.5f;
        float sideWidth = (houseWidth - doorWidth) * 0.5f;

        MakeWall("Wall_Front_Left",
            new Vector3(-halfW + sideWidth * 0.5f, wy, frontZ),
            new Vector3(sideWidth, wallHeight, wallThickness));

        MakeWall("Wall_Front_Right",
            new Vector3(halfW - sideWidth * 0.5f, wy, frontZ),
            new Vector3(sideWidth, wallHeight, wallThickness));

        float lintelHeight = wallHeight - doorHeight;
        MakeWall("Wall_Front_Top",
            new Vector3(0f, doorHeight + lintelHeight * 0.5f + wallThickness, frontZ),
            new Vector3(doorWidth, lintelHeight, wallThickness));
    }

    // Fill the triangular gap between the top of the wall and the peak of the roof
    // on both the front and back faces using stacked horizontal slices.
    void BuildGables()
    {
        float halfW = houseWidth * 0.5f;
        float halfD = houseDepth * 0.5f;
        float baseY = wallHeight + wallThickness;
        float frontZ = halfD - wallThickness * 0.5f;
        float backZ = -halfD + wallThickness * 0.5f;

        for (int i = 0; i < gableSlices; i++)
        {
            float t0 = (float)i / gableSlices;
            float t1 = (float)(i + 1) / gableSlices;
            float y0 = baseY + t0 * roofPeak;
            float y1 = baseY + t1 * roofPeak;
            float sliceH = y1 - y0;
            float sliceY = (y0 + y1) * 0.5f;

            float w0 = Mathf.Lerp(houseWidth, 0f, t0);
            float w1 = Mathf.Lerp(houseWidth, 0f, t1);
            float sliceW = (w0 + w1) * 0.5f;

            if (sliceW < 0.01f) continue;

            MakeWall($"Gable_Front_{i}",
                new Vector3(0f, sliceY, frontZ),
                new Vector3(sliceW, sliceH, wallThickness));

            MakeWall($"Gable_Back_{i}",
                new Vector3(0f, sliceY, backZ),
                new Vector3(sliceW, sliceH, wallThickness));
        }
    }

    void MakeWall(string wallName, Vector3 localPos, Vector3 localScale)
    {
        var wall = MakeBox(wallName, localPos, localScale);
        if (wallMaterial) wall.GetComponent<Renderer>().material = wallMaterial;
    }

    void BuildRoof()
    {
        float halfSpan = houseWidth * 0.5f + roofOverhang;
        float panelLen = houseDepth + roofOverhang * 2f;
        float panelWidth = Mathf.Sqrt(halfSpan * halfSpan + roofPeak * roofPeak);
        float angle = Mathf.Atan2(roofPeak, halfSpan) * Mathf.Rad2Deg;
        float baseY = wallHeight + wallThickness;

        MakeRoofPanel("Roof_Left",
            new Vector3(-halfSpan * 0.5f, baseY + roofPeak * 0.5f, 0f),
            new Vector3(panelWidth, 0.12f, panelLen),
            angle);

        MakeRoofPanel("Roof_Right",
            new Vector3(halfSpan * 0.5f, baseY + roofPeak * 0.5f, 0f),
            new Vector3(panelWidth, 0.12f, panelLen),
            -angle);
    }

    void MakeRoofPanel(string panelName, Vector3 localPos, Vector3 localScale, float zAngle)
    {
        var panel = MakeBox(panelName, localPos, localScale);
        panel.transform.localEulerAngles = new Vector3(0f, 0f, zAngle);
        panel.transform.localPosition = localPos;
        if (roofMaterial) panel.GetComponent<Renderer>().material = roofMaterial;
    }

    GameObject MakeBox(string objName, Vector3 localPos, Vector3 localScale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.isStatic = true;
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        return go;
    }
}
