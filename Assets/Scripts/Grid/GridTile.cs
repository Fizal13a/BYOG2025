using UnityEngine;

public class GridTile : MonoBehaviour
{
    private bool isOccupied;
    
    public void SetIsOccupied(bool value)
    {
        isOccupied = value;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }
}
