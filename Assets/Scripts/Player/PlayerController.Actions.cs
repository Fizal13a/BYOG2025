using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

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
        GridTile currentTile = GridGenerator.instance.GetTile(currentSelectedPlayer.GetGridPosition().x, currentSelectedPlayer.GetGridPosition().y);
        currentTile.SetOccupied(false);
        targetTile.SetOccupied(true);
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

        if (currentPlayerWithBall != null && currentPlayerWithBall == currentSelectedPlayer)
        {
            GameManager.instance.SetBallPosition(targetTile.GridPosition);
        }
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }

    #endregion
    
    // --- Once the target player is selected, pass the ball to that player ---
    private void PassToPlayer(GridTile tile)
    {
        foreach (var player in players)
        {
            if (player.GetGridPosition() == tile.GridPosition)
            {
                currentPassTargetPlayer  = player;
                break;
            }
        }
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

        StartCoroutine(BallPassMove(tile.transform));
        GameManager.instance.SetBallPosition(currentPlayerWithBall.GetGridPosition());
        currentSelectedPlayer = null;
    }

    IEnumerator BallPassMove(Transform targetTile)
    {
        if (targetTile == null)
            yield break;

        GameObject ball = GameManager.instance.GetBallObject();
        
        Vector3 startPos = ball.transform.position;
        Vector3 endPos = targetTile.position;

        float elapsed = 0f;

        Vector3 forwardDir = (endPos - startPos).normalized;
        Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir); 

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1);

            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, endPos, smoothT);

            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * 0.3f;

            pos.y += heightOffset + 0.3f;

            ball.transform.position = pos;

            yield return null;
        }

        ball.transform.position = new Vector3(endPos.x, endPos.y + 0.3f, endPos.z);
        SetPlayerWithBall(currentPlayerWithBall);
        GameManager.instance.EndTurnEarly();
    }
    
    // --- Once the AI with ball is in the adjustment tile, Can get the ball back ---
    private void Tackle()
    {
        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();
        GameManager.instance.SetBallPosition(playerGridPos);
        SetPlayerWithBall(currentSelectedPlayer);
        HandleStates(PlayerStates.Move);
    }
    // --- Shoot the ball to goal ---
    private void ShootToGoal()
    {
        Vector2Int tileIndex = GameManager.instance.GetPlayerGoalTile().GridPosition;
        GridTile targetTile = GridGenerator.instance.GetTile(tileIndex.x, tileIndex.y);

        Transform targetPos = targetTile.transform;

        StartCoroutine(BallShootToGaoal(targetTile.transform));

        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
    
    IEnumerator BallShootToGaoal(Transform targetTile)
    {
        if (targetTile == null)
            yield break;

        GameObject ball = GameManager.instance.GetBallObject();
        
        Vector3 startPos = ball.transform.position;
        Vector3 endPos = targetTile.position;

        float elapsed = 0f;

        Vector3 forwardDir = (endPos - startPos).normalized;
        Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir); 

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1);

            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, endPos, smoothT);

            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * 0.3f;

            pos.y += heightOffset + 0.3f;

            ball.transform.position = pos;

            yield return null;
        }

        ball.transform.position = new Vector3(endPos.x, endPos.y + 0.3f, endPos.z);
        UIManager.instance.AddPlayerScore(1);
    }
    
    // --- Pass to a player and move at the same time ---
    private void Dash()
    {
        currentSelectedPlayer = null;
        GameManager.instance.EndTurnEarly();
    }
}
