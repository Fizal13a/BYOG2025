using UnityEngine;

public partial class PlayerController : MonoBehaviour
{

    private void HandleStates(ActionData.Actions state)
    {
        currentAction = GetAction(state);
        GridGenerator.instance.ClearHighlightedTiles();

        if(!GameManager.instance.SpendActionPoints(currentAction.actionCost)) return;

        switch (state)
        {
            case ActionData.Actions.Move:
                DebugLogger.Log("Player On Move State", "yellow");
                GridGenerator.instance.HighlightMoveTiles(currentSelectedPlayer);
                canSelect = true;
                break;
            
            case ActionData.Actions.Pass:
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
            
            case ActionData.Actions.Tackle:
                Tackle();
                DebugLogger.Log("Player On Tackle State", "yellow");
                break;
            
            case ActionData.Actions.Shoot:
                ShootToGoal();
                DebugLogger.Log("Player On Shoot State", "yellow");
                break;
            
            case ActionData.Actions.Dash:
                DebugLogger.Log("Player On Dash State", "yellow");
                break;
        }
    }

    private ActionData GetAction(ActionData.Actions action)
    {
        foreach (var act in availableActions)
        {
            if (act.action == action)
            {
                return act;
            }
        }
        
        return null;
    }
}
