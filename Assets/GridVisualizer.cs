using UnityEngine;

public class GridBackground : MonoBehaviour
{
    public int width = 10;
    public int height = 7;
    public float tileSize = 1f;
    public float lineWidth = 0.03f;

    void Start()
    {
        // Only build at runtime
        BuildGridRuntime();
    }

    void BuildGridRuntime()
    {
        // clear old children safely (runtime)
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        // vertical lines
        for (int x = 0; x <= width; x++)
            MakeLine(new Vector3(x * tileSize, 0, 0), new Vector3(x * tileSize, height * tileSize, 0));

        // horizontal lines
        for (int y = 0; y <= height; y++)
            MakeLine(new Vector3(0, y * tileSize, 0), new Vector3(width * tileSize, y * tileSize, 0));
    }

    void MakeLine(Vector3 from, Vector3 to)
    {
        var go = new GameObject("GridLine");
        go.transform.SetParent(transform, false);

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = -10;
    }
}