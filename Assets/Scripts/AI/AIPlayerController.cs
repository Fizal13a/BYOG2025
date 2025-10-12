using System;
using System.Collections;
using UnityEngine;

public partial class AIPlayerController : MonoBehaviour
{
    public enum AIStates
    {
        Move, Pass, Tackle, Shoot, Dash
    }

    [Header("Game Data")] 
    public LayerMask playerLayer;
    
    [Header("AIs Data")]
    private AIPlayer[] ais;
    private bool hasBall = false;
    public AIPlayer currentAIWithBall = null;
    public AIPlayer currentSelectedAI = null;
    public AIPlayer currentPassTargetAI = null;
    [SerializeField] private GridTile goalTile;
    private bool isAITurn = false;

    private void OnEnable()
    {
    }

    public void SetUpAIs()
    {
        ais = new AIPlayer[3];
        goalTile = GameManager.instance.GetAIGoalTile();
    }

    public void AddAI(AIPlayer player, Vector2Int gridPos, int index)
    {
        if(index < 3)
        {
            ais[index] = player;
            player.SetUpPlayer(this, gridPos);
        }
    }

    public void HasBall(bool status)
    {
        hasBall = status;
    }

    public void SetPlayerWithBall(AIPlayer player)
    {
        GameObject ball = GameManager.instance.GetBallObject();
        ball.transform.SetParent(player.ballHolderPosition);
        ball.transform.localPosition = Vector3.zero;
        GameManager.instance.SetBallPosition(player.GetGridPosition());
        currentAIWithBall = player;
        hasBall = true;
        currentPassTargetAI = null;
    }

    public void SetSelectedPlayer(AIPlayer player)
    {
        currentSelectedAI = player;
    }

    public void SetUpTurn(bool turn)
    {
        StartCoroutine(DelayAITurnStart(turn));
    }

    IEnumerator DelayAITurnStart(bool turn)
    {
        yield return new WaitForSeconds(1);
        isAITurn = turn;
        if (isAITurn)
        {
            AIStates selectedState = SelectAIState();
            ExecuteState(selectedState);
        }
    }
    
    private AIStates SelectAIState()
    {
        // Check if any AI has the ball
        UpdateBallStatus();
        
        if (hasBall && currentAIWithBall != null)
        {
            // AI has ball - check shoot condition first
            GridTile currentTile = GridGenerator.instance.GetTile(currentAIWithBall.GetGridPosition().x, currentAIWithBall.GetGridPosition().y);
            int distanceToGoal = GetTileDistance(currentTile, goalTile);
            
            if (distanceToGoal <= 2)
            {
                return AIStates.Shoot;
            }
            
            // Check if any enemy players within 2 tiles - ALWAYS pass if enemies nearby
            if (AreEnemiesNearby(currentAIWithBall, 1))
            {
                return AIStates.Pass;
            }
            else
            {
                return AIStates.Move; // Move towards goal
            }
        }
        else
        {
            // AI doesn't have ball
            // Check if ball is within 1 tile of any AI for tackle
            
            GridTile ballTile = GridGenerator.instance.GetTile(GameManager.instance.GetCurrentBallPosition().x, GameManager.instance.GetCurrentBallPosition().y);
            AIPlayer closestAIToBall = GetClosestAIToBall(ballTile);
            
            if (closestAIToBall != null)
            {
                GridTile currentTile = GridGenerator.instance.GetTile(closestAIToBall.GetGridPosition().x, closestAIToBall.GetGridPosition().y);
                int xDist = Mathf.Abs(currentTile.GridPosition.x - ballTile.GridPosition.x);
                int yDist = Mathf.Abs(currentTile.GridPosition.y - ballTile.GridPosition.y);

                if (xDist <= 1 && yDist <= 1)
                {
                    currentSelectedAI = closestAIToBall;
                    return AIStates.Tackle;
                }
            }
            
            // Select AI nearest to ball or player with ball and move
            currentSelectedAI = GetBestAIToSelect(ballTile);
            return AIStates.Move;
        }
    }
    
    private void ExecuteState(AIStates state)
    {
        switch (state)
        {
            case AIStates.Move:
                ExecuteMove();
                break;
            case AIStates.Pass:
                ExecutePass();
                break;
            case AIStates.Tackle:
                ExecuteTackle();
                break;
            case AIStates.Shoot:
                ExecuteShoot();
                break;
            case AIStates.Dash:
                // Implement later
                break;
        }
    }

    IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1f);
        GameManager.instance.EndTurnEarly();
    }
    
    private void ExecuteMove()
    {
        if (hasBall && currentAIWithBall != null)
        {
            // Double check for enemies before moving - if found, pass instead
            if (AreEnemiesNearby(currentAIWithBall, 1))
            {
                Debug.Log($"{currentAIWithBall.name} detected enemy, switching to pass");
                ExecutePass();
                return;
            }

            // Move 1 tile towards goal
            Debug.Log($"{currentAIWithBall.name} moving towards goal");
            GridTile currentTile = GridGenerator.instance.GetTile(currentAIWithBall.GetGridPosition().x, currentAIWithBall.GetGridPosition().y);
            GridTile nextTile = GetNextTileTowards(currentTile, goalTile);
            
            if (nextTile != null)
            {
                MoveToTile(currentAIWithBall, nextTile);
            }
            else
            {
                Debug.LogWarning($"{currentAIWithBall.name} has no valid move - ending turn");
                StartCoroutine(EndTurn());
            }
        }
        else if (currentSelectedAI != null)
        {
            // Move 1 tile towards ball
            GridTile ballTile = GridGenerator.instance.GetTile(GameManager.instance.GetCurrentBallPosition().x, GameManager.instance.GetCurrentBallPosition().y);
            Debug.Log($"{currentSelectedAI.name} moving towards ball");
            GridTile currentTile = GridGenerator.instance.GetTile(currentSelectedAI.GetGridPosition().x, currentSelectedAI.GetGridPosition().y);
            GridTile nextTile = GetNextTileTowards(currentTile, ballTile);
            
            if (nextTile != null)
            {
                MoveToTile(currentSelectedAI, nextTile);
            }
            else
            {
                Debug.LogWarning($"{currentSelectedAI.name} has no valid move - ending turn");
                StartCoroutine(EndTurn());
            }
        }
    }
    
    private void ExecutePass()
    {
        if (currentAIWithBall != null)
        {
            AIPlayer bestReceiver = FindBestPassReceiver();
            if (bestReceiver != null)
            {
                Debug.Log($"{currentAIWithBall.name} passing to {bestReceiver.name}");
                GridTile targetTile = GridGenerator.instance.GetTile(bestReceiver.GetGridPosition().x, bestReceiver.GetGridPosition().y);
                currentPassTargetAI = bestReceiver;
                currentAIWithBall = bestReceiver;
                StartCoroutine(MoveBallInArc(BallController.instance.gameObject.transform, targetTile.transform.position));
            }
            else
            {
                Debug.LogWarning("No valid pass receiver - ending turn");
                StartCoroutine(EndTurn());
            }
        }
    }

    public IEnumerator MoveBallInArc(Transform ball, Vector3 targetPosition, float arcHeight = 2f, float duration = 1f)
    {
        DebugLogger.Log("Moving ball to target", "red");
        Vector3 startPos = ball.transform.position;
        Vector3 endPos = targetPosition;

        float elapsed = 0f;

        Vector3 forwardDir = (endPos - startPos).normalized;
        Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir); 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, endPos, smoothT);

            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;
            
            pos.y += heightOffset + 0.3f;

            ball.position = pos;

            yield return null;
        }

        SetPlayerWithBall(currentPassTargetAI);
        StartCoroutine(EndTurn());
        // Ensure we end exactly at target
        ball.position = targetPosition;
    }
    
    private void ExecuteTackle()
    {
        if (currentSelectedAI != null)
        {
            Debug.Log($"{currentSelectedAI.name} attempting tackle");
            SetPlayerWithBall(currentSelectedAI);
            StartCoroutine(DelayAfterTackle());
        }
    }
    
    private void ExecuteShoot()
    {
        if (currentAIWithBall != null)
        {
            Debug.Log($"{currentAIWithBall.name} shooting at goal");
            // Implement shoot logic here
            StartCoroutine(EndTurn());
        }
    }
    
    // Helper methods
    private void UpdateBallStatus()
    {
        // This should be called to update which AI has the ball
        // You'll need to implement this based on your ball system
        // For now, just check the hasBall boolean you're already setting
    }
    
    private bool AreEnemiesNearby(AIPlayer ai, int range)
    {
        GridTile aiTile = GridGenerator.instance.GetTile(ai.GetGridPosition().x, ai.GetGridPosition().y);
        int x = aiTile.GridPosition.x;
        int y = aiTile.GridPosition.y;
        
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
        
        DebugLogger.Log($"AI with ball position {ai.GetGridPosition()}", "blue");
    
        foreach (var dir in directions)
        {
            Vector2Int checkPos = ai.GetGridPosition() + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos.x, checkPos.y);
            if (tile != null && tile.IsOccupied())
            {
                return true;
            }
        }

        return false;
    }
    
    private AIPlayer GetClosestAIToBall(GridTile ballTile)
    {
        AIPlayer closest = null;
        int minDist = int.MaxValue;
        
        foreach (AIPlayer ai in ais)
        {
            if (ai == null) continue;
            
            GridTile aiTile = GridGenerator.instance.GetTile(ai.GetGridPosition().x, ai.GetGridPosition().y);
            int dist = GetTileDistance(aiTile, ballTile);
            if (dist < minDist)
            {
                minDist = dist;
                closest = ai;
            }
        }
        
        return closest;
    }
    
    private AIPlayer GetBestAIToSelect(GridTile ballTile)
    {
        // Select AI nearest to ball
        return GetClosestAIToBall(ballTile);
    }
    
    private AIPlayer FindBestPassReceiver()
    {
        AIPlayer bestReceiver = null;
        int bestScore = int.MinValue;
        
        foreach (AIPlayer ai in ais)
        {
            if (ai == null || ai == currentAIWithBall) continue;
            
            // Calculate score based on distance to goal and being open
            GridTile aiTile = GridGenerator.instance.GetTile(ai.GetGridPosition().x, ai.GetGridPosition().y);
            int distToGoal = GetTileDistance(aiTile, goalTile);
            int score = 100 - distToGoal; // Closer to goal = better
            
            if (score > bestScore)
            {
                bestScore = score;
                bestReceiver = ai;
            }
        }
        
        return bestReceiver;
    }
    
    // Grid-based pathfinding helpers
    private GridTile GetNextTileTowards(GridTile from, GridTile to)
    {
        int dx = to.GridPosition.x - from.GridPosition.x;
        int dy = to.GridPosition.y - from.GridPosition.y;
        
        // Normalize to get direction (-1, 0, or 1)
        int moveX = dx != 0 ? (dx > 0 ? 1 : -1) : 0;
        int moveY = dy != 0 ? (dy > 0 ? 1 : -1) : 0;
        
        // Try diagonal first if both x and y need movement
        if (moveX != 0 && moveY != 0)
        {
            GridTile diagonalTile = GridGenerator.instance.GetTile(from.GridPosition.x + moveX, from.GridPosition.y + moveY);
            if (diagonalTile != null && !diagonalTile.IsOccupied())
            {
                return diagonalTile;
            }
        }
        
        // Try horizontal movement
        if (moveX != 0)
        {
            GridTile horizontalTile = GridGenerator.instance.GetTile(from.GridPosition.x + moveX, from.GridPosition.y);
            if (horizontalTile != null && !horizontalTile.IsOccupied())
            {
                return horizontalTile;
            }
        }
        
        // Try vertical movement
        if (moveY != 0)
        {
            GridTile verticalTile = GridGenerator.instance.GetTile(from.GridPosition.x, from.GridPosition.y + moveY);
            if (verticalTile != null && !verticalTile.IsOccupied())
            {
                return verticalTile;
            }
        }
        
        return null; // No valid tile found
    }
    
    private int GetTileDistance(GridTile tile1, GridTile tile2)
    {
        // Manhattan distance
        int dx = Mathf.Abs(tile1.GridPosition.x - tile2.GridPosition.x);
        int dy = Mathf.Abs(tile1.GridPosition.y - tile2.GridPosition.y);
        return dx + dy;
    }
    
    IEnumerator DelayAfterTackle()
    {
        yield return new WaitForSeconds(0.5f);
        ExecuteState(AIStates.Pass);
    }
}