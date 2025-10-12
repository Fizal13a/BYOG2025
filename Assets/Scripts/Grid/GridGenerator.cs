using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator instance;

    [Header("Grid Settings")] 
    public Transform gridParent;
    public GameSettings gameSettings;
    
    // 2D array to store generated tiles
    private GameObject[,] grid;
    private GridTile[,] gridTiles;
    
    public List<GridTile> highlightedTiles =  new List<GridTile>();
    
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

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (gameSettings.tilePrefab == null)
        {
            Debug.LogWarning("No tile prefab assigned!");
            return;
        }
        
        ClearGrid();
        
        int width = gameSettings.gridWidth;
        int height = gameSettings.gridHeight;
        float spacing = gameSettings.spacing;
        GameObject tilePrefab = gameSettings.tilePrefab;

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
                tileScript.SetGridPosition(new Vector2Int(x,y));

                if (GameManager.instance.playerGoalTilePos == new Vector2Int(x, y))
                {
                    GameManager.instance.SetPlayerGoalTile(tileScript);
                }
                
                if (GameManager.instance.aiGoalTilePos == new Vector2Int(x, y))
                {
                    GameManager.instance.SetAIGoalTile(tileScript);
                }

                // Store in array
                grid[x, y] = tile;
            }
        }

        SetTileNeighbors();
    }

    #endregion
    
    #region Getters

    public GameObject GetTileObject(int x, int y)
    {
        if (x < 0 || x >= gameSettings.gridWidth || y < 0 || y >= gameSettings.gridHeight)
            return null;
        return grid[x, y];
    }

    public GridTile GetTile(int x, int y)
    {
        if (x < 0 || x >= gameSettings.gridWidth || y < 0 || y >= gameSettings.gridHeight)
            return null;
        return gridTiles[x, y];
    }

    #endregion

    #region Helpers

    public void ClearGrid()
    {
        foreach (Transform tiles in gridParent)
        {
            Destroy(tiles.gameObject);
        }

        if(grid != null)
            Array.Clear(grid,0,grid.Length);
        if(gridTiles != null)
            Array.Clear(gridTiles,0,gridTiles.Length);
    }
    
    public void HighlightMoveTiles(Player player)
    {
        DebugLogger.Log($"Highlighting Tiles");
        ClearHighlightedTiles();

        Vector2Int playerGridPos = player.GetGridPosition(); 
    
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // right
            new Vector2Int(-1, 0),  // left
            new Vector2Int(0, 1),   // up
            new Vector2Int(0, -1),  // down
            new Vector2Int(1, 1),   // top right
            new Vector2Int(-1, 1),  // top left
            new Vector2Int(1, -1),  // bottom right
            new Vector2Int(-1, -1), // bottom left
        };
    
        foreach (var dir in directions)
        {
            Vector2Int checkPos = playerGridPos + dir;
            GridTile tile = GetTile(checkPos.x, checkPos.y);
            if (tile != null && !tile.IsOccupied())
            {
                tile.Highlight(true);
                highlightedTiles.Add(tile);
            }
        }
    }

    public void HighlightPassTiles(int x, int y)
    {

        GridTile tile = GetTile(x, y);
        if (tile != null && tile.IsWalkable)
        {
            tile.Highlight(true);
            highlightedTiles.Add(tile);
        }
    }

    public void ClearHighlightedTiles()
    {
        foreach (var tile in highlightedTiles)
            tile.Highlight(false);
        highlightedTiles.Clear();
    }


    #endregion

    #region PathFinding

     // 🔹 Converts a world position to a GridTile
    public GridTile GetTileFromWorld(Vector3 worldPos)
    {
        GridTile closestTile = null;
        float minDist = Mathf.Infinity;

        foreach (var tile in allTiles)
        {
            float dist = Vector3.Distance(worldPos, tile.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestTile = tile;
            }
        }
        return closestTile;
    }

    // 🔹 Finds a path using A* algorithm
    public List<GridTile> GetPath(GridTile startTile, GridTile targetTile)
    {
        if (startTile == null || targetTile == null)
            return null;

        List<GridTile> openSet = new List<GridTile>();
        HashSet<GridTile> closedSet = new HashSet<GridTile>();

        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            GridTile current = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < current.FCost ||
                    (openSet[i].FCost == current.FCost && openSet[i].hCost < current.hCost))
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetTile)
                return RetracePath(startTile, targetTile);

            foreach (GridTile neighbor in current.neighbors)
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    continue;

                int newCostToNeighbor = current.gCost + GetDistance(current, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetTile);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // no path found
    }

    private List<GridTile> RetracePath(GridTile start, GridTile end)
    {
        List<GridTile> path = new List<GridTile>();
        GridTile current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    private int GetDistance(GridTile a, GridTile b)
    {
        int dstX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dstY = Mathf.Abs(a.GridPosition.x - b.GridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
    public void SetTileNeighbors()
    {
        foreach (GridTile tile in allTiles)
        {
            tile.neighbors.Clear();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = tile.GridPosition.x + x;
                    int checkY = tile.GridPosition.y + y;

                    // Make sure within grid bounds
                    if (checkX >= 0 && checkY >= 0 && checkX < gameSettings.gridWidth && checkY < gameSettings.gridWidth)
                    {
                        GridTile neighbor = gridTiles[checkX, checkY];
                        if (neighbor != null && neighbor.IsWalkable)
                        {
                            tile.neighbors.Add(neighbor);
                        }
                    }
                }
            }
        }

        DebugLogger.Log("✅ Tile neighbors successfully assigned!", "green");
    }


    #endregion
   
}