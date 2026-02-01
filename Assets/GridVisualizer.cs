using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class GridBackground : MonoBehaviour
{
    public int width = 10;
    public int height = 7;
    public float tileSize = 1f;

    public float lineWidth = 0.03f;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.positionCount = 0;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // Important: make sure it renders behind sprites
        lr.sortingOrder = -10;

        BuildGrid();
    }

    void BuildGrid()
    {
        // We’ll draw each line as a separate 2-point segment.
        // Easiest: make one LineRenderer per line… but to keep it simple,
        // we’ll create child objects with their own LineRenderers.
        // (Still lightweight for a 7x10 prototype.)
        foreach (Transform child in transform) Destroy(child.gameObject);

        // vertical lines
        for (int x = 0; x <= width; x++)
        {
            MakeLine(new Vector3(x * tileSize - tileSize / 2, -tileSize / 2, 0), new Vector3(x * tileSize - tileSize / 2, height * tileSize - tileSize / 2, 0));
        }
        // horizontal lines
        for (int y = 0; y <= height; y++)
        {
            MakeLine(new Vector3(-tileSize / 2, y * tileSize - tileSize / 2, 0), new Vector3(width * tileSize - tileSize / 2, y * tileSize - tileSize / 2, 0));
        }
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

        // default material (works without setup)
        line.material = new Material(Shader.Find("Sprites/Default"));

        // keep it behind sprites
        line.sortingOrder = -10;
    }
}