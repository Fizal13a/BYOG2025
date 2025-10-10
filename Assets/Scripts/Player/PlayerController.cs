using System;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public enum PlayerStates
    {
        Move, Pass, Tackle, Shoot, Dash
    }
    
    [Header("Game Referances")]
    public Camera mainCamera;
    
    [Header("Players Data")]
    private Player[] players;
    public LayerMask playerLayer;
    
    [Header("Turn Data")]
    private Player currentSelectedPlayer;
    private Player targetSelectedPlayer;
    private PlayerStates currentState;
    
    private bool isPlayerTurn = false;

    #region Initialization

    public void SetUpPlayers()
    {
        players = new Player[3];
        SetUpUI();
    }

    #endregion

    #region Setters

    public void AddPlayer(Player player)
    {
        players[0] = player;
    }

    public void SetUpTurn(bool turn)
    {
        isPlayerTurn = turn;
    }

    #endregion
    
    private void Update()
    {
        if(!isPlayerTurn) return;

        // --- Raycast ---
        HandleSelection();
    }
    
    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, playerLayer))
            {
                Player player = hit.collider.GetComponent<Player>();
                if (player != null)
                {
                    currentSelectedPlayer = player;
                    ToggleUI(true);
                }
            }
        }
    }
}
