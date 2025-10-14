using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
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
    public  Player currentPlayerWithBall;
    public Player currentPassTargetPlayer;
    private PlayerStates currentState;
    
    private bool isPlayerTurn = false;

    #region Initialization
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    #endregion

    #region Setters

    public void SetUpPlayers()
    {
        players = new Player[3];
        SetUpUI();
    }
    
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
        DebugLogger.Log("Setting player turn " + isPlayerTurn);
    }

    public void SetPlayerWithBall(Player player)
    {
        GameObject ball = GameManager.instance.GetBallObject();
        DebugLogger.Log(ball.gameObject.name + ", " + player.gameObject.name, "yellow");
        ball.transform.SetParent(player.ballHolderPosition);
        ball.transform.localPosition = Vector3.zero;
        currentPlayerWithBall = player;

        currentPassTargetPlayer = null;
    }

    #endregion
    
    private void Update()
    {
        if(!isPlayerTurn) return;

        // --- Raycast to select target for pass or move --- 
        if (canSelect)
        {
            HandleActionSelection();
            return;
        }

        // --- Raycast to select the player ---
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
