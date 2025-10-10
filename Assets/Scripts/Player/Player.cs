using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController controller;
    
    [Header("Stats")]
    private float moveSpeed = 5f;
    
    [Header("Grid Data")] 
    private Vector2Int GridPosition;
    
    public void SetUpPlayer(PlayerController playerController, Vector2Int gridPosition)
    {
        controller = playerController;
        SetGridPosition(gridPosition);
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
    
    public Vector2Int GetGridPosition() => GridPosition;
    public float GetMoveSpeed() => moveSpeed;
}
