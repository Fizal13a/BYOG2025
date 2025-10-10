using UnityEngine;

public class Grid_Manager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 5;
    public int cols = 5;
    public float cellSize = 1f;

    [Header("Tile Options")]
    public GameObject cellPrefab;   
    public bool useQuad = true;     
    public bool centerGrid = true;
    public bool generateOnStart = true;

    void Start()
    {
        if (generateOnStart)
            GenerateGrid();
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        ClearGrid();

        float step = cellSize;
        Vector3 offset = Vector3.zero;
        if (centerGrid)
        {
            float width = (cols - 1) * step;
            float height = (rows - 1) * step;
            offset = new Vector3(width * 0.5f, 0f, height * 0.5f);
        }
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(c * step, 0f, r * step) - offset;
                GameObject cell;

                if (cellPrefab != null)
                {
                    cell = Instantiate(cellPrefab, transform);
                    cell.transform.localPosition = pos;
                    cell.transform.localScale = Vector3.one * cellSize;
                }
                else
                {
                    if (useQuad)
                    {
                        cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        cell.transform.SetParent(transform, false);
                        cell.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                        cell.transform.localPosition = pos;
                        cell.transform.localScale = Vector3.one * cellSize;
                    }
                    else
                    {
                        cell = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        cell.transform.SetParent(transform, false);
                        float planeScale = cellSize / 10f;
                        cell.transform.localScale = new Vector3(planeScale, 1f, planeScale);
                        Vector3 planePos = new Vector3(c * (10f * planeScale), 0f, r * (10f * planeScale)) - offset;
                        cell.transform.localPosition = planePos;
                    }
                }

                cell.name = $"Cell_{r}_{c}";
            }
        }
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
