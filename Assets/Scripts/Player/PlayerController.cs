using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public LayerMask tileLayer;
    
    [Header("Turn Data")]
    private Player currentSelectedPlayer;
    private Player targetSelectedPlayer;
    public  Player currentPlayerWithBall;
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

    public void AddPlayer(Player player, Vector2Int gridPos, int index)
    {
        if(index<3)
        {
            players[index] = player;
            player.SetUpPlayer(this, gridPos);
        }
       
    }

    public void SetUpTurn(bool turn)
    {
        isPlayerTurn = turn;
    }

    #endregion
    
    private void Update()
    {
        if(!isPlayerTurn) return;

        if (canSelect)
        {
            HandleActionSelection();
            return;
        }

        // --- Raycast ---
        HandlePlayerSelection();
    }
    
    private void HandlePlayerSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, playerLayer))
            {
                Player player = hit.collider.GetComponent<Player>();
                if (player != null)
                {
                    GridGenerator.instance.ClearHighlightedTiles();

                    currentSelectedPlayer = player;
                    ToggleUI(true);
                }
            }
        }
    }
}
