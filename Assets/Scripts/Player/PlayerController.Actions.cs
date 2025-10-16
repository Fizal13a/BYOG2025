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
        
        ToggleUI(false);
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

        DebugLogger.Log($"Target Tile {targetTile.GridPosition}, Ball Position {GameManager.instance.GetCurrentBallPosition()}", "red");
        currentSelectedPlayer.SetGridPosition(targetTile.GridPosition);
        if (targetTile.GridPosition == GameManager.instance.GetCurrentBallPosition())
        {
            //Player got the ball
            SetPlayerWithBall(currentSelectedPlayer);
        }

        if (currentPlayerWithBall != null && currentPlayerWithBall == currentSelectedPlayer)
        {
            GameManager.instance.SetBallPosition(targetTile.GridPosition);
        }
        
        currentSelectedPlayer = null;
    }

    #endregion
    
    // --- Once the target player is selected, pass the ball to that player ---

    #region Pass

    private void PassToPlayer(GridTile tile)
    {
        foreach (var player in players)
        {
            if (player.GetGridPosition() == tile.GridPosition)
            {
                currentPassTargetPlayer  = player;
                currentPlayerWithBall = player;
                break;
            }
        }
        
        GameObject ball = GameManager.instance.GetBallObject();
        ball.transform.SetParent(null);

        StartCoroutine(BallPassMove(tile.transform));
        GameManager.instance.SetBallPosition(currentPlayerWithBall.GetGridPosition());
        currentSelectedPlayer = null;
        
        ToggleUI(false);
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
    }

    #endregion
    
    // --- Once the AI with ball is in the adjustment tile, Can get the ball back ---

    #region Tackle

    private void Tackle()
    {
        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();
        GameManager.instance.SetBallPosition(playerGridPos);
        SetPlayerWithBall(currentSelectedPlayer);
        HandleStates(ActionData.Actions.Move);
        AIPlayerController.instance.RemoveBall();
        
        ToggleUI(false);
    }

    #endregion
   
    // --- Shoot the ball to goal ---

    #region Shoot

    private void ShootToGoal()
    {
        Vector2Int tileIndex = GameManager.instance.GetPlayerGoalTile().GridPosition;
        GridTile targetTile = GridGenerator.instance.GetTile(tileIndex.x, tileIndex.y);

        Transform targetPos = targetTile.transform;

        StartCoroutine(BallShootToGaoal(targetTile.transform));

        currentSelectedPlayer = null;
        
        ToggleUI(false);
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
        GameManager.instance.ResetRound();
    }

    #endregion
    
    // --- Pass to a player and move at the same time ---
    private void Dash()
    {
        currentSelectedPlayer = null;
    }
}
