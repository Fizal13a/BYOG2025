using System;
using System.Collections;
using UnityEngine;

public partial class AIPlayerController : MonoBehaviour
{
    private void MoveToTile(AIPlayer currentplayer, GridTile tile)
    {
        GridTile currentTile = GridGenerator.instance.GetTile(currentplayer.GetGridPosition().x, currentplayer.GetGridPosition().y);
        if (currentTile != null)
        {
            currentTile.SetOccupied(false);
            currentTile.occupyingEntity = null;
        }

        //#region Play Animation

        //Animator animator = currentplayer.GetComponentInChildren<Animator>();

        //AnimationManager.Instance.MoveAnim(animator,true);

        //#endregion

        // Move to new tile
        currentplayer.SetGridPosition(tile.GridPosition);
        tile.SetOccupied(true);
        tile.occupyingEntity = currentplayer.gameObject;
        //StartCoroutine(MoveToTileRoutine(animator, currentplayer, tile));
        StartCoroutine(MoveToTileRoutine(currentplayer, tile));
        DebugLogger.Log(currentplayer, "yellow");
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
       
        //#region Play Animation

        //AnimationManager.Instance.MoveAnim(animator, false);

        //#endregion


        currentSelectedAI = null;
    }
}
