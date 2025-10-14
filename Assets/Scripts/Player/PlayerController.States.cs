using UnityEngine;

public partial class PlayerController : MonoBehaviour
{

    private PlayerStates currentAction;
    private void HandleStates(PlayerStates state)
    {
        currentAction = state;
        GridGenerator.instance.ClearHighlightedTiles();

        switch (state)
        {
            case PlayerStates.Move:
                DebugLogger.Log("Player On Move State", "yellow");
                GridGenerator.instance.HighlightMoveTiles(currentSelectedPlayer);
                canSelect = true;
                break;
            
            case PlayerStates.Pass:
                foreach(var player in players)
                {
                    if (player == currentPlayerWithBall)
                        continue;

                    Vector2Int positionIndex = player.GetGridPosition();
                    GridGenerator.instance.HighlightPassTiles(positionIndex.x, positionIndex.y);
                }
                DebugLogger.Log("Player On Pass State", "yellow");
                canSelect = true;
                break;
            
            case PlayerStates.Tackle:
                Tackle();
                DebugLogger.Log("Player On Tackle State", "yellow");
                break;
            
            case PlayerStates.Shoot:
                ShootToGoal();
                DebugLogger.Log("Player On Shoot State", "yellow");
                break;
            
            case PlayerStates.Dash:
                DebugLogger.Log("Player On Dash State", "yellow");
                break;
        }
    }
}
