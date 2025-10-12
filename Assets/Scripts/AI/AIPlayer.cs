using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private AIPlayerController controller;
    public Transform ballHolderPosition;
    
    [Header("Stats")]
    private float moveSpeed = 2f;
    
    [Header("Grid Data")] 
    private Vector2Int GridPosition;
    
    public void SetUpPlayer(AIPlayerController playerController, Vector2Int gridPosition)
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
