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
    private TurnHandler opponentTeam; // Could be AI or Player 2

    private void SetUpTurnStates()
    {
        // Assign teams (for now, Player vs AI)
        playerTeam = new TurnHandler("Player");
        opponentTeam = new TurnHandler("Opponent");
    }

    // --- Turn Start ---
    private void StartTurn(TurnHandler team)
    {
        currentState = team == playerTeam ? GameSettings.GameState.PlayerTurn : GameSettings.GameState.AITurn;
        SetTurnDisplayText($"{team.TeamName} Turn");

        team.BeginTurn(baseActionPoints, maxCarryOverPoints);
        UpdateActionPointUI(team.CurrentAP);

        Debug.Log($"{team.TeamName} Turn Started with {team.CurrentAP} AP");

        // Enable/Disable input accordingly
        playerController.SetUpTurn(team == playerTeam);
        aiPlayerController.SetUpTurn(team == opponentTeam && IsAITurn());

        // If AI turn, start coroutine
        if (IsAITurn())
            StartCoroutine(HandleAITurn());
    }

    // --- Turn End ---
    public void EndTurn()
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTeam : opponentTeam;
        TurnHandler nextTeam = IsPlayerTurn() ? opponentTeam : playerTeam;

        Debug.Log($"{activeTeam.TeamName} Turn Ended with {activeTeam.CurrentAP} leftover AP");

        StartTurn(nextTeam);
    }

    public void StopAllTurns()
    {
        currentState = GameSettings.GameState.Waiting;
    }

    // --- Spend Action Points ---
    public bool SpendActionPoints(int cost)
    {
        TurnHandler activeTeam = IsPlayerTurn() ? playerTeam : opponentTeam;

        if (!activeTeam.CanAfford(cost))
        {
            Debug.Log("Not enough Action Points!");
            return false;
        }

        activeTeam.UseActionPoints(cost);
        UpdateActionPointUI(activeTeam.CurrentAP);

        if (activeTeam.CurrentAP <= 0)
            EndTurn();

        return true;
    }

    // --- AI Logic (temporary stub) ---
    private IEnumerator HandleAITurn()
    {
        TurnHandler aiTeam = opponentTeam;

        yield return new WaitForSeconds(1f);

        while (aiTeam.CurrentAP > 0)
        {
            int actionCost = Random.Range(1, 3);
            aiTeam.UseActionPoints(actionCost);
            Debug.Log($"AI performed action costing {actionCost}. Remaining AP: {aiTeam.CurrentAP}");

            UpdateActionPointUI(aiTeam.CurrentAP);

            yield return new WaitForSeconds(1f);
        }

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
        // Hook this to your UI
        SetActionPointsText(points);
    }
}
