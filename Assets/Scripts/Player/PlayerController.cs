using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    
    public List<ActionData> availableActions = new List<ActionData>();
    private ActionData currentAction;
    
    [Header("Game Referances")]
    public Camera mainCamera;
    
    [Header("Players Data")]
    private Player[] players;
    public LayerMask playerLayer;
    public LayerMask tileLayer;
    
    [Header("Turn Data")]
    public Player currentSelectedPlayer;
    public  Player currentPlayerWithBall;
    public Player currentPassTargetPlayer;

    private bool isPlayerTurn = false;
    
    [Header("Conditions")]
    public bool canMove = true;
    public bool canPass = true;
    public bool canTackle = true;
    public bool canShoot =  true;

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

    public void RemovePlayerWithBall()
    {
        currentPlayerWithBall = null;
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
        //HandlePlayerSelection();
    }

    public void SetConditions()
    {
        canPass = true; canTackle = true;
        canMove = true; canShoot = true;

        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();

        // Disable tackle button if player with ball exists nearby
        if (currentPlayerWithBall != null || !IsBallAdjacent(playerGridPos))
            canTackle = false;
        
        // Disable pass button if player with ball not exists
        if (currentPlayerWithBall == null)
            canPass =  false;
        
        else if(currentSelectedPlayer != currentPlayerWithBall)
            canPass =  false;

        // Disable shoot button if player doesn't have ball or near goal
        if (currentPlayerWithBall == null || !IsNearGoal(playerGridPos))
            canShoot =  false;
        
        Debug.Log($"{canPass} {canTackle} {canShoot}");
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
                    currentSelectedPlayer = player;
                    ToggleUI(true);
                }
            }
        }
    }

}
