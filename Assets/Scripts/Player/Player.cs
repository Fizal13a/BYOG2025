using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private TeamManager teamManager;
    public Transform ballHolderPosition;
    
    [Header("Stats")]
    private float moveSpeed = 5f;
    
    [Header("Grid Data")] 
    private Vector2Int GridPosition;
    private Vector2Int origionalGridPosition;
    
    public void SetUpPlayer(TeamManager team, Vector2Int gridPosition)
    {
        teamManager = team;
        
        //Set position
        Vector3 pos = new Vector3(gridPosition.x, 0, gridPosition.y);
        transform.position = pos;
        
        //Set values
        SetGridPosition(gridPosition);
        SetOrigionalGridPosition(gridPosition);
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        GridTile playerCurrTile = GridGenerator.instance.GetTile(gridPosition);
        if(playerCurrTile != null) playerCurrTile.SetOccupied(true);
    }

    public void SetOrigionalGridPosition(Vector2Int origionalGridPos)
    {
        origionalGridPosition = origionalGridPos;
    }
    
    public Vector2Int GetGridPosition() => GridPosition;
    public Vector2Int GetOrigionalGridPosition() => origionalGridPosition;
    public float GetMoveSpeed() => moveSpeed;
}
