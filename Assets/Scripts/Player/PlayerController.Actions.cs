using System.Collections;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    // --- Once the target tile is selected, move to that tile ---

    #region Movement

    private void MoveToTile(GridTile targetTile)
    {
        DebugLogger.Log($"Moving to tile {targetTile.gameObject.name}");
        StartCoroutine(MoveToTileRoutine(targetTile));
    }
    
    private IEnumerator MoveToTileRoutine(GridTile targetTile)
    {
        Vector3 start = currentSelectedPlayer.transform.position;
        Vector3 end = targetTile.WorldPosition;
        end.y = start.y;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * currentSelectedPlayer.GetMoveSpeed();
            currentSelectedPlayer.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        currentSelectedPlayer.SetGridPosition(targetTile.GridPosition);
        DebugLogger.Log($"Target Tile {targetTile.GridPosition}, Ball Position {GameManager.instance.GetCurrentBallPosition()}", "red");
        if (targetTile.GridPosition == GameManager.instance.GetCurrentBallPosition())
        {
            SetPlayerWithBall(currentSelectedPlayer);
        }
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }

    #endregion
    
    // --- Once the target player is selected, pass the ball to that player ---
    private void PassToPlayer(GridTile tile)
    {
        GameObject ball = GameManager.instance.GetBallObject();
        DebugLogger.Log(ball.gameObject.name, "yellow");
        ball.transform.SetParent(null);
        
        foreach(var player in players)
        {
            if(player.GetGridPosition()==tile.GridPosition)
            {
                currentPlayerWithBall = player;
                break;
            }
        }

        BallController.instance.MoveBall(tile.transform);
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
    // --- Once the AI with ball is in the adjustment tile, Can get the ball back ---
    private void Tackle()
    {
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
    // --- Shoot the ball to goal ---
    private void ShootToGoal()
    {
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
    // --- Pass to a player and move at the same time ---
    private void Dash()
    {
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
}
