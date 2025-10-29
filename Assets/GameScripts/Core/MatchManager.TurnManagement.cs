using System.Collections;
using UnityEngine;

public partial class MatchManager : MonoBehaviour
{
    public enum PlayState
    {
        PlayerTurn, OpponentTurn, Waiting
    }
    
   [Header("Runtime Info")]
    public PlayState currentState;

    [Header("Action Point Settings")]
    [SerializeField] private int baseActionPoints = 3;
    [SerializeField] private int maxCarryOverPoints = 10;

    private TurnHandler playerTurnHandler;
    private TurnHandler opponentTurnHandler;
    
    private Coroutine currentAITurnCoroutine;
    private bool turnTransitioning = false; // Prevent double turn ends

    private void SetUpTurnStates()
    {
        playerTurnHandler = new TurnHandler("Player");
        opponentTurnHandler = new TurnHandler("Opponent");
    }

    // --- Turn Start ---
    private void StartTurn(TurnHandler team)
    {
        turnTransitioning = false; // Reset flag
        currentState = team == playerTurnHandler ? PlayState.PlayerTurn : PlayState.OpponentTurn;
        SetTurnDisplayText($"{team.TeamName} Turn");

        team.BeginTurn(baseActionPoints, maxCarryOverPoints);
        UpdateActionPointUI(team.CurrentAP);

        Debug.Log($"{team.TeamName} Turn Started with {team.CurrentAP} AP");

        if (team == playerTurnHandler)
        {
            // Player Turn
            if (currentAITurnCoroutine != null)
            {
                StopCoroutine(currentAITurnCoroutine);
                currentAITurnCoroutine = null;
            }
            
            HandCardManager.instance.ClearCards();
            HandCardManager.instance.DrawCards();
            playerTeam.SetUpTurn(true);
            opponentTeam.SetUpTurn(false);
        }
        else
        {
            // Opponent Turn
            HandCardManager.instance.ClearCards();
            playerTeam.SetUpTurn(false);
            opponentTeam.SetUpTurn(true);
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

        TurnHandler activeTeam = IsPlayerTurn() ? playerTurnHandler : opponentTurnHandler;
        TurnHandler nextTeam = IsPlayerTurn() ? opponentTurnHandler : playerTurnHandler;

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
        currentState = PlayState.Waiting;
        
        if (currentAITurnCoroutine != null)
        {
            StopCoroutine(currentAITurnCoroutine);
            currentAITurnCoroutine = null;
        }
    }

    // --- Spend Action Points ---
    public void CheckEndTurn()
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTurnHandler : opponentTurnHandler;
        
        if (activeTeam.CurrentAP <= 0)
        {
            Debug.Log($"{activeTeam.TeamName} out of action points!");
            EndTurn();
            return;
        }
        
        if (IsPlayerTurn())
        {
            HandCardManager.instance.CheckCards();
            //playerController.SetConditions();
        }
    }
    
    // --- Check for Action Points ---
    public bool CheckActionPoints(int cost)
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTurnHandler : opponentTurnHandler;

        if (!activeTeam.CanAfford(cost))
        {
            Debug.Log($"Not enough Action Points! Need {cost}, Have {activeTeam.CurrentAP}");
            return false;
        }
        
        activeTeam.UseActionPoints(cost);
        UpdateActionPointUI(activeTeam.CurrentAP);
        
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
        playerTurnHandler.SetCurrentActionPoints(0);
        opponentTurnHandler.SetCurrentActionPoints(0);
    }

    private bool IsPlayerTurn() => currentState == PlayState.PlayerTurn;
    private bool IsAITurn() => currentState == PlayState.OpponentTurn;

    private void UpdateActionPointUI(int points)
    {
        SetActionPointsDisplayText(points);
    }
}
