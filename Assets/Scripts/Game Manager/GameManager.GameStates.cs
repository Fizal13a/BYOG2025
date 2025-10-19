using System.Collections;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    [Header("Runtime Info")]
    public GameSettings.GameState currentState;

    [Header("Action Point Settings")]
    [SerializeField] private int baseActionPoints = 3;
    [SerializeField] private int maxCarryOverPoints = 10;

    private TurnHandler playerTeam;
    private TurnHandler opponentTeam;
    
    private Coroutine currentAITurnCoroutine;
    private bool turnTransitioning = false; // Prevent double turn ends

    private void SetUpTurnStates()
    {
        playerTeam = new TurnHandler("Player");
        opponentTeam = new TurnHandler("Opponent");
    }

    // --- Turn Start ---
    private void StartTurn(TurnHandler team)
    {
        turnTransitioning = false; // Reset flag
        currentState = team == playerTeam ? GameSettings.GameState.PlayerTurn : GameSettings.GameState.AITurn;
        SetTurnDisplayText($"{team.TeamName} Turn");

        team.BeginTurn(baseActionPoints, maxCarryOverPoints);
        UpdateActionPointUI(team.CurrentAP);

        Debug.Log($"{team.TeamName} Turn Started with {team.CurrentAP} AP");

        if (team == playerTeam)
        {
            // Player Turn
            if (currentAITurnCoroutine != null)
            {
                StopCoroutine(currentAITurnCoroutine);
                currentAITurnCoroutine = null;
            }
            
            playerController.SetUpTurn(true);
            aiPlayerController.SetUpTurn(false);
        }
        else
        {
            // AI Turn
            playerController.SetUpTurn(false);
            aiPlayerController.SetUpTurn(true);
        }
    }

    // --- Turn End ---
    public void EndTurn()
    {
        // Prevent multiple simultaneous turn ends
        if (turnTransitioning)
        {
            Debug.LogWarning("Turn already transitioning, ignoring extra EndTurn call");
            return;
        }

        turnTransitioning = true;

        TurnHandler activeTeam = IsPlayerTurn() ? playerTeam : opponentTeam;
        TurnHandler nextTeam = IsPlayerTurn() ? opponentTeam : playerTeam;

        Debug.Log($"{activeTeam.TeamName} Turn Ended with {activeTeam.CurrentAP} leftover AP");

        // Stop AI coroutine if switching from AI turn
        if (IsAITurn() && currentAITurnCoroutine != null)
        {
            StopCoroutine(currentAITurnCoroutine);
            currentAITurnCoroutine = null;
        }

        StartTurn(nextTeam);
    }

    public void StopAllTurns()
    {
        currentState = GameSettings.GameState.Waiting;
        
        if (currentAITurnCoroutine != null)
        {
            StopCoroutine(currentAITurnCoroutine);
            currentAITurnCoroutine = null;
        }
    }

    // --- Spend Action Points ---
    public void CheckEndTurn()
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTeam : opponentTeam;
        
        if (activeTeam.CurrentAP <= 0)
        {
            Debug.Log($"{activeTeam.TeamName} out of action points!");
            EndTurn();
        }
    }
    
    // --- Check for Action Points ---
    public bool CheckActionPoints(int cost)
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTeam : opponentTeam;

        if (!activeTeam.CanAfford(cost))
        {
            Debug.Log($"Not enough Action Points! Need {cost}, Have {activeTeam.CurrentAP}");
            return false;
        }
        
        activeTeam.UseActionPoints(cost);
        UpdateActionPointUI(activeTeam.CurrentAP);
        
        Debug.Log($"Action executed. Remaining AP: {activeTeam.CurrentAP}");

        // Check immediately if this was the last action point
        if (activeTeam.CurrentAP <= 0)
        {
            StartCoroutine(DelayedEndTurn());
        }
        
        return true;
    }

    // Small delay to let current action finish before ending turn
    IEnumerator DelayedEndTurn()
    {
        yield return new WaitForSeconds(0.1f);
        EndTurn();
    }

    public void ResetActionPoints()
    {
        playerTeam.SetCurrentActionPoints(0);
        opponentTeam.SetCurrentActionPoints(0);
    }

    private bool IsPlayerTurn() => currentState == GameSettings.GameState.PlayerTurn;
    private bool IsAITurn() => currentState == GameSettings.GameState.AITurn;

    private void UpdateActionPointUI(int points)
    {
        SetActionPointsText(points);
    }
}