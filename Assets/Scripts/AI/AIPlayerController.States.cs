using UnityEngine;

public partial class AIPlayerController : MonoBehaviour
{
    private void HandleStates(AIStates state)
    {
        switch (state)
        {
            case AIStates.Move:
                DebugLogger.Log("AI On Move State", "yellow");
                GridTile targetTile;
                if (currentAIWithBall != null)
                {
                    targetTile = GetNextTileTowardGoal(currentAIWithBall);
                    MoveToTile(currentAIWithBall, targetTile);
                }
                else
                {
                    targetTile = GetNextTileTowardBall(currentSelectedAI);
                    MoveToTile(currentSelectedAI, targetTile);
                }
                break;
            
            case AIStates.Pass:
                DebugLogger.Log("AI On Pass State", "yellow");
                break;
            
            case AIStates.Tackle:
                DebugLogger.Log("AI On Tackle State", "yellow");
                break;
            
            case AIStates.Shoot:
                DebugLogger.Log("AI On Shoot State", "yellow");
                break;
            
            case AIStates.Dash:
                DebugLogger.Log("AI On Dash State", "yellow");
                break;
        }
    }
}
