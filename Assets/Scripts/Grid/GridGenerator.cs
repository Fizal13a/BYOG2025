using System;
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

                // Store in array
                grid[x, y] = tile;
            }
        }
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
        foreach (Transform tiles in transform)
        {
            Destroy(tiles.gameObject);
        }

        if(grid != null)
            Array.Clear(grid,0,grid.Length);
        if(gridTiles != null)
            Array.Clear(gridTiles,0,gridTiles.Length);
    }

    #endregion
   
}