using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Grid/GridSettings")]
public class GridSettings : ScriptableObject
{
    [Header("Grid")]
    public GameObject gridTilePrefab;
    public GameObject goalPostPrefab;
    public int gridWidth;
    public int gridHeight;
    public float spacing;

    [Header("Tile")] 
    public Sprite normalSprite;
    public Sprite selectedSprite;
}
