using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator instance;

    [Header("Grid Settings")] public Transform gridParent;
    public GridSettings gridSettings;
    public MatchSettings matchSettings;

    // 2D array to store generated tiles
    private GameObject[,] grid;
    private GridTile[,] gridTiles;

    public List<GridTile> highlightedTiles = new List<GridTile>();

    [HideInInspector] public List<GridTile> allTiles = new List<GridTile>();
    public GridTile goalTile;

    #region Initialization

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        MatchManager.matchEvents.AddEvent(MatchEvents.MatchEventType.OnRoundReset, ResetOccupiedTiles);
    }

    public void GenerateGrid()
    {
        if (gridSettings.gridTilePrefab == null)
        {
            Debug.LogWarning("No tile prefab assigned!");
            return;
        }

        ClearGrid();

        int width = gridSettings.gridWidth;
        int height = gridSettings.gridHeight;
        float spacing = gridSettings.spacing;
        GameObject tilePrefab = gridSettings.gridTilePrefab;
        GameObject goalPost = gridSettings.goalPostPrefab;

        // Create ONE shared canvas for all tiles
        Canvas sharedCanvas = CreateSharedCanvas(width, height, spacing);

        grid = new GameObject[width, height];
        gridTiles = new GridTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * spacing, 0f, y * spacing);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.SetParent(gridParent);

                GridTile tileScript = tile.GetComponent<GridTile>();
                gridTiles[x, y] = tileScript;
                tileScript.SetGridPosition(new Vector2Int(x, y));

                // Turn off 3D renderer
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = false;

                // Create UI image on the SHARED canvas
                Image uiImage = CreateUIImageForTile(sharedCanvas, x, y, spacing);
                tileScript.SetUIImage(uiImage); // Direct reference as you wanted!

                if (matchSettings.opponentGoalPosition == new Vector2Int(x, y))
                {
                    position.y = 0.4f;
                    GameObject goal = Instantiate(goalPost, position, Quaternion.identity, transform);
                    goal.transform.SetParent(tile.transform);
                    //GameManager.instance.SetAIGoalTile(tileScript);
                }

                if (matchSettings.teamGoalPosition == new Vector2Int(x, y))
                {
                    position.y = 0.4f;
                    GameObject goal = Instantiate(goalPost, position, Quaternion.identity, transform);
                    goal.transform.rotation = Quaternion.Euler(0, 180, 0);
                    goal.transform.SetParent(tile.transform);
                    //GameManager.instance.SetPlayerGoalTile(tileScript);
                }

                grid[x, y] = tile;
            }
        }
    }

    private Canvas CreateSharedCanvas(int width, int height, float spacing)
    {
        GameObject canvasObj = new GameObject("SharedGridCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // Position at grid center, slightly above
        float centerX = (width - 1) * spacing / 2f;
        float centerZ = (height - 1) * spacing / 2f;
        canvasObj.transform.position = new Vector3(centerX, -0.039f, centerZ);
        canvasObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Size to cover entire grid
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        float totalWidth = width * spacing;
        float totalHeight = height * spacing;
        canvasRect.sizeDelta = new Vector2(totalWidth * 100, totalHeight * 100);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        return canvas;
    }

    private Image CreateUIImageForTile(Canvas parentCanvas, int x, int y, float spacing)
    {
        GameObject imageObj = new GameObject($"UIImage_{x}_{y}");
        imageObj.transform.SetParent(parentCanvas.transform, false);

        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        Image image = imageObj.AddComponent<Image>();

        // Position manually to match 3D grid position
        // Since canvas is centered, calculate offset from center
        int width = gridSettings.gridWidth;
        int height = gridSettings.gridHeight;

        float offsetX = (x - (width - 1) / 2f) * spacing * 100;
        float offsetY = (y - (height - 1) / 2f) * spacing * 100;

        rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);
        rectTransform.sizeDelta = new Vector2(spacing * 100 * 0.95f, spacing * 100 * 0.95f); // 0.95 for gap

        // Style
        image.color = new Color(1f, 1f, 1f, 0.3f);
        return image;
    }

    #endregion

    #region Getters

    public GameObject GetTileObject(int x, int y)
    {
        if (x < 0 || x >= gridSettings.gridWidth || y < 0 || y >= gridSettings.gridHeight) return null;
        return grid[x, y];
    }

    public GridTile GetTile(Vector2Int  position)
    {
        if (position.x < 0 || position.x >= gridSettings.gridWidth || position.y < 0 || position.y >= gridSettings.gridHeight) return null;
        return gridTiles[position.x, position.y];
    }

    #endregion

    #region Setters

    public void ResetOccupiedTiles()
    {
        if (gridTiles == null) return;

        foreach (GridTile tile in gridTiles)
        {
            tile.SetOccupied(false);
        }
    }

    #endregion

    #region Helpers

    public void ClearGrid()
    {
        foreach (Transform tiles in gridParent)
        {
            Destroy(tiles.gameObject);
        }

        if (grid != null) Array.Clear(grid, 0, grid.Length);
        if (gridTiles != null) Array.Clear(gridTiles, 0, gridTiles.Length);
    }

    public void HighlightMoveTiles(Player player)
    {
        DebugLogger.Log($"Highlighting Tiles");
        ClearHighlightedTiles();

        Vector2Int playerGridPos = player.GetGridPosition();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), // right
            new Vector2Int(-1, 0), // left
            new Vector2Int(0, 1), // up
            new Vector2Int(0, -1), // down
            new Vector2Int(1, 1), // top right
            new Vector2Int(-1, 1), // top left
            new Vector2Int(1, -1), // bottom right
            new Vector2Int(-1, -1), // bottom left
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = playerGridPos + dir;
            GridTile tile = GetTile(checkPos);
            if (tile != null && !tile.IsOccupied())
            {
                tile.Highlight(true);
                highlightedTiles.Add(tile);
            }
        }
    }

    public void HighlightPassTiles(Vector2Int position)
    {
        GridTile tile = GetTile(position);
        if (tile != null && tile.IsWalkable)
        {
            tile.Highlight(true);
            highlightedTiles.Add(tile);
        }
    }

    public void ClearHighlightedTiles()
    {
        foreach (var tile in highlightedTiles) tile.Highlight(false);
        highlightedTiles.Clear();
    }

    #endregion
}