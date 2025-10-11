using System.Collections;
using UnityEngine;

public partial class AIPlayerController : MonoBehaviour
{
    private void MoveToTile(AIPlayer currentplayer, GridTile tile)
    {
        StartCoroutine(MoveToTileRoutine(currentplayer, tile));
    }
    
    private IEnumerator MoveToTileRoutine(AIPlayer currentplayer, GridTile targetTile)
    {
        Vector3 start = currentplayer.transform.position;
        Vector3 end = targetTile.WorldPosition;
        end.y = start.y;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * currentplayer.GetMoveSpeed();
            currentplayer.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        currentplayer.SetGridPosition(targetTile.GridPosition);
        DebugLogger.Log($"Target Tile {targetTile.GridPosition}, Ball Position {GameManager.instance.GetCurrentBallPosition()}", "red");
        if (targetTile.GridPosition == GameManager.instance.GetCurrentBallPosition())
        {
            //SetPlayerWithBall(currentplayer);
        }
        currentSelectedAI = null;
        //GameManager.instance.EndTurnEarly();
    }
}
