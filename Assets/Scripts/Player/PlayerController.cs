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
    private Player currentSelectedPlayer;
    public  Player currentPlayerWithBall;
    public Player currentPassTargetPlayer;

    [Header("Camera Info")]
    [SerializeField] CinemachineCamera cine_camera;

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
        HandlePlayerSelection();

        // --- Camera Positon
        SetCameraTarget();
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

    #region Cinemachine camera Target
    private void SetCameraTarget()
    {
        if(currentSelectedPlayer!=null)
        {
            cine_camera.LookAt = currentSelectedPlayer.transform;
            cine_camera.Follow = currentSelectedPlayer.transform;
        }
        else
        {
            //ResetCamPos();
        }

    }

    private void ResetCamPos()
    {
        cine_camera.LookAt = null;
        cine_camera.Follow = null;

        cine_camera.transform.position = new Vector3(0, 0, -25f);
        cine_camera.transform.rotation = Quaternion.identity;
    }

    #endregion
}
