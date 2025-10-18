using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public partial class PlayerController : MonoBehaviour
{
    #region Movement

    private void MoveToTile(GridTile targetTile)
    {
        DebugLogger.Log($"Moving to tile {targetTile.gameObject.name}");

        #region Play Animation
        Animator animator = currentSelectedPlayer.GetComponentInChildren<Animator>();

        AnimationManager.Instance.MoveAnim(animator, true);

        #endregion

        StartCoroutine(MoveToTileRoutine(animator,targetTile));
        
        ToggleUI(false);
    }
    
    private IEnumerator MoveToTileRoutine(Animator animator, GridTile targetTile)
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

        #region Play Animation

        AnimationManager.Instance.MoveAnim(animator, false);

        #endregion

        currentSelectedPlayer = null;
    }

    #endregion
    
    // --- Once the target player is selected, pass the ball to that player ---

    #region Pass

    private void PassToPlayer(GridTile tile)
    {
        #region Play Animation

        Animator animator = currentPlayerWithBall.GetComponentInChildren<Animator>();
        AnimationManager.Instance.PassAnim(animator);

        #endregion


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
        Vector3 midPoint = (startPos + endPos) / 2f;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Clamp(distance * 0.4f, 0.3f, 1.2f); // Duration scales with distance
        float elapsed = 0f;

        // Calculate arc with some randomness for arcade feel
        float arcHeight = distance * 0.3f + Random.Range(-0.1f, 0.15f);

        // Get rotation direction
        Vector3 direction = (endPos - startPos).normalized;
        Quaternion rotationAxis = Quaternion.FromToRotation(Vector3.forward, direction);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out-in curve for more natural feel (faster start, slower end)
            float easeT = t < 0.5f 
                ? 2f * t * t 
                : -1f + (4f - 2f * t) * t;

            // Parabolic arc (more natural than sine)
            float arcAmount = Mathf.Sin(t * Mathf.PI) * arcHeight;

            // Interpolate position with arc
            Vector3 pos = Vector3.Lerp(startPos, endPos, easeT);
            pos.y += arcAmount;

            ball.transform.position = pos;

            // Rotate ball for tumble effect
            ball.transform.Rotate(direction * 800f * Time.deltaTime, Space.World);

            yield return null;
        }

        // Snap to final position
        ball.transform.position = new Vector3(endPos.x, endPos.y + 0.05f, endPos.z);
        SetPlayerWithBall(currentPlayerWithBall);
    }

    #endregion
    
    // --- Once the AI with ball is in the adjustment tile, Can get the ball back ---

    #region Tackle

    private void Tackle()
    {
        #region Play Animation

        Animator animator = currentSelectedPlayer.GetComponentInChildren<Animator>();
        AnimationManager.Instance.TackleAnim(animator);

        #endregion

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

        #region Play Animation

        Animator animator = currentPlayerWithBall.GetComponentInChildren<Animator>();
        AnimationManager.Instance.ShootAnim(animator);

        #endregion

        Vector2Int tileIndex = GameManager.instance.GetPlayerGoalTile().GridPosition;
        GridTile targetTile = GridGenerator.instance.GetTile(tileIndex.x, tileIndex.y);

        Transform targetPos = targetTile.transform;

        StartCoroutine(BallShootToGoal(targetTile.transform));

        currentSelectedPlayer = null;
        
        ToggleUI(false);
    }
    
    IEnumerator BallShootToGoal(Transform targetTile)
    {
        if (targetTile == null)
            yield break;

        GameObject ball = GameManager.instance.GetBallObject();
    
        Vector3 startPos = ball.transform.position;
        Vector3 endPos = targetTile.position;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Clamp(distance * 0.25f, 0.2f, 0.8f); // Faster than passes
        float elapsed = 0f;

        // Higher arc for goal shots (more dramatic)
        float arcHeight = distance * 0.2f + Random.Range(0.02f, 0.1f);

        Vector3 direction = (endPos - startPos).normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out curve (fast start, quick deceleration) for powerful feel
            float easeT = 1f - Mathf.Pow(1f - t, 2.5f);

            // Parabolic arc
            float arcAmount = Mathf.Sin(t * Mathf.PI) * arcHeight;

            Vector3 pos = Vector3.Lerp(startPos, endPos, easeT);
            pos.y += arcAmount;

            ball.transform.position = pos;

            // Faster spin for more aggressive feel
            ball.transform.Rotate(direction * 1200f * Time.deltaTime, Space.World);

            yield return null;
        }

        // Snap to final position
        ball.transform.position = endPos;
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
