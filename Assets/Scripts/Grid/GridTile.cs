using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    private bool isOccupied;
    
    public Vector2Int GridPosition;
    public bool IsWalkable = true;
    public Vector3 WorldPosition => transform.position;
    
    private Renderer rend;
    
    public List<GridTile> neighbors = new List<GridTile>();

    [HideInInspector] public int gCost;
    [HideInInspector] public int hCost;
    public int FCost => gCost + hCost;
    [HideInInspector] public GridTile parent;

    private void Awake() => rend = GetComponent<Renderer>();

    public void Highlight(bool active)
    {
        rend.material.color = active ? Color.yellow : Color.white;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
    
    public void SetIsOccupied(bool value)
    {
        isOccupied = value;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }
}
