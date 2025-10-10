using UnityEngine;

public class GridTile : MonoBehaviour
{
    private bool isOccupied;
    
    public Vector2Int GridPosition;
    public bool IsWalkable = true;
    public Vector3 WorldPosition => transform.position;
    
    private Renderer rend;

    private void Awake() => rend = GetComponent<Renderer>();

    public void Highlight(bool active)
    {
        rend.material.color = active ? Color.yellow : Color.white;
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
