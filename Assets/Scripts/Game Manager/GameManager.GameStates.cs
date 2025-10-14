using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class GameManager : MonoBehaviour
{
    [Header("Runtime Info")]
    public GameSettings.GameState currentState;
    private Coroutine turnRoutine;

    private Coroutine stateDelayRoutine;
    
    // --- Player Turn --- 
    private void StartPlayerTurn()
    {
        currentState = GameSettings.GameState.PlayerTurn;
        SetTurnDisplayText(currentState.ToString());
        Debug.Log("Player Turn Started");

        playerController.SetUpTurn(true);
        aiPlayerController.SetUpTurn(false);

        // Automatically switch to AI after delay
        if (turnRoutine != null) StopCoroutine(turnRoutine);
        turnRoutine = StartCoroutine(WaitAndStartNextTurn(GameSettings.GameState.AITurn));
    }

    // --- AI Turn --- 
    private void StartAITurn()
    {
        Debug.Log("AI Turn Started");
        currentState = GameSettings.GameState.AITurn;
        SetTurnDisplayText(currentState.ToString());

        aiPlayerController.SetUpTurn(true);
        playerController.SetUpTurn(false);

        if (turnRoutine != null) StopCoroutine(turnRoutine);
        turnRoutine = StartCoroutine(WaitAndStartNextTurn(GameSettings.GameState.PlayerTurn));
        
        GridGenerator.instance.ClearHighlightedTiles();
    }

    // --- Wait State --- 
    IEnumerator WaitAndStartNextTurn(GameSettings.GameState nextState)
    {
        DebugLogger.Log("WaitAndStartNextTurn", "orange");
        float currentTime = gameSettings.turnDuration;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime; // decrease per second, not per frame
            SetTurnTimerText(currentTime);
            yield return null;
        }

        // Ensure timer text shows 0 at end
        SetTurnTimerText(0);

        // Switch to next state
        if (nextState == GameSettings.GameState.PlayerTurn)
            StartPlayerTurn();
        else if (nextState == GameSettings.GameState.AITurn)
            StartAITurn();
    }

    
    // --- Skip Turn --- 
    public void EndTurnEarly()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            if (currentState == GameSettings.GameState.PlayerTurn)
                StartAITurn();
            else if (currentState == GameSettings.GameState.AITurn)
                StartPlayerTurn();
        }
    }
    
    public void StopAllTurns()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
        }
    }
}
