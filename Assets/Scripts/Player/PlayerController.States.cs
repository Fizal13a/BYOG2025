using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    private void HandleStates(PlayerStates state)
    {
        switch (state)
        {
            case PlayerStates.Move:
                DebugLogger.Log("Player On Move State", "yellow");
                GridGenerator.instance.HighlightMoveTiles(currentSelectedPlayer);
                currentSelectedPlayer.SetSelection(true);
                break;
            
            case PlayerStates.Pass:
                DebugLogger.Log("Player On Pass State", "yellow");
                break;
            
            case PlayerStates.Tackle:
                DebugLogger.Log("Player On Tackle State", "yellow");
                break;
            
            case PlayerStates.Shoot:
                DebugLogger.Log("Player On Shoot State", "yellow");
                break;
            
            case PlayerStates.Dash:
                DebugLogger.Log("Player On Dash State", "yellow");
                break;
        }
        
        ToggleUI(false);
    }
}
