using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public partial class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("Settings")]
    public GameSettings gameSettings;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject aiPrefab;
    public GameObject ballPrefab;

    [Header("Spawn Positions (Grid Coordinates)")]
    public Transform playersParent;
    public Transform aisParent;
    public Vector2Int[] playerSpawnTiles;
    public Vector2Int[] aiSpawnTiles;
    public Vector2Int ballSpawnTile;
    public Vector2Int aiTeamGoalPos;
    public Vector2Int playerTeamGoalPos;
    
    [Header("In Game Referances")]
    public PlayerController playerController;
    public AIPlayerController aiPlayerController;

    // Internal storage
    private GameObject[] players;
    private GameObject[] aiPlayers;
    private GameObject ball;
    
    [Header("Runtime Datas")]
    private Player currentPlayerWithBall;
    private AIPlayer currentAIWithBall;
    private Vector2Int currentBallPosition;

    private GridTile playerGoalTile;
    private GridTile aiGoalTile;

    private string scoredTeam = null;

    #region Initialization

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // --- All initializing goes here --- 
        InitializeMatch();
    }

    void InitializeMatch()
    {
        // --- Spawn Grid ---
        GridGenerator.instance.GenerateGrid();
        
        // --- Initialize Players and AIs
        playerController.SetUpPlayers(); 
        aiPlayerController.SetUpAIs();
        
        players = new GameObject[playerSpawnTiles.Length];
        aiPlayers = new GameObject[aiSpawnTiles.Length];

        // --- Spawn Players ---
        for (int i = 0; i < playerSpawnTiles.Length; i++)
        {
            Vector2Int gridPos = playerSpawnTiles[i];
            GameObject tile = GridGenerator.instance.GetTileObject(gridPos.x, gridPos.y);

            if (tile)
            {
                Vector3 spawnPos = tile.transform.position + Vector3.up * 0.1f;
                players[i] = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                players[i].name = $"Player_{i}";
                players[i].transform.SetParent(playersParent);
                Player player  = players[i].GetComponent<Player>();
                playerController.AddPlayer(player,gridPos,i);
            }
            
            GridTile tileScript = GridGenerator.instance.GetTile(gridPos.x, gridPos.y);
            tileScript.SetOccupied(true);
        }

        // --- Spawn AI ---
        for (int i = 0; i < aiSpawnTiles.Length; i++)
        {
            Vector2Int gridPos = aiSpawnTiles[i];
            GameObject tile = GridGenerator.instance.GetTileObject(gridPos.x, gridPos.y);

            if (tile)
            {
                Vector3 spawnPos = tile.transform.position + Vector3.up * 0.1f;
                aiPlayers[i] = Instantiate(aiPrefab, spawnPos, Quaternion.Euler(0,180f,0));
                aiPlayers[i].name = $"AI_{i}";
                aiPlayers[i].transform.SetParent(aisParent);
                AIPlayer ai  = aiPlayers[i].GetComponent<AIPlayer>();
                aiPlayerController.AddAI(ai, gridPos, i);
            }
            
            GridTile tileScript = GridGenerator.instance.GetTile(gridPos.x, gridPos.y);
            tileScript.SetOccupied(true);
        }

        // --- Spawn Ball ---
        GameObject ballTile = GridGenerator.instance.GetTileObject(ballSpawnTile.x, ballSpawnTile.y);
        if (ballTile)
        {
            Vector3 spawnPos = ballTile.transform.position + Vector3.up * 0.3f;
            ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
            ball.name = "Ball";
            SetBallPosition(ballSpawnTile);
        }

        StartCoroutine(StartGameDelay());
    }

    IEnumerator StartGameDelay()
    {
        yield return new WaitForSeconds(2f);
        int randomTurn = Random.Range(0, 10);
        if (randomTurn > 5)
        {
            // --- Start State --- 
            AIPlayer ai = aiPlayers[0].GetComponent<AIPlayer>();
            aiPlayerController.SetPlayerWithBall(ai);
            StartAITurn();
        }
        else
        {
            // --- Start State --- 
            Player player = players[0].GetComponent<Player>();
            playerController.SetPlayerWithBall(player);
            StartPlayerTurn();
        }
    }

    #endregion

    #region Getters

    public Player GetCurrentPlayerWithBall()
    {
        return currentPlayerWithBall;
    }

    public AIPlayer GetCurrentAIWithBall()
    {
        return currentAIWithBall;
    }

    public Vector2Int GetCurrentBallPosition()
    {
        return currentBallPosition;
    }

    public GameObject GetBallObject()
    {
        return ball;
    }

    public GridTile GetPlayerGoalTile()
    {
        return playerGoalTile;
    }

    public GridTile GetAIGoalTile()
    {
        return aiGoalTile;
    }

    #endregion

    #region Setters

    public void SetPlayerWithBall(Player player)
    {
        currentPlayerWithBall = player;
    }

    public void SetAIWithBall(AIPlayer ai)
    {
        currentAIWithBall = ai;
    }

    public void SetBallPosition(Vector2Int position)
    {
        currentBallPosition = position;
    }

    public void SetPlayerGoalTile(GridTile tile)
    {
        playerGoalTile = tile;
    }

    public void SetAIGoalTile(GridTile tile)
    {
        aiGoalTile = tile;
    }
    
    public void SetScored(string team)
    {
        scoredTeam = team;
    }

    #endregion

    #region Reset

    public void ResetRound()
    {
        StartCoroutine(DelayForReset());
        StopAllTurns();
    }

    IEnumerator DelayForReset()
    {
        yield return new WaitForSeconds(3f);
        ResetMatch();
    }

    public void ResetMatch()
    {
        GridGenerator.instance.ResetOccupiedTiles();
        
        for (int i = 0; i < playerSpawnTiles.Length; i++)
        {
            float yPos = players[i].transform.position.y;
            players[i].transform.position = new Vector3(playerSpawnTiles[i].x, yPos, playerSpawnTiles[i].y);
            Player pl = players[i].GetComponent<Player>();
            pl.SetGridPosition(playerSpawnTiles[i]);
            GridTile playerTile = GridGenerator.instance.GetTile(playerSpawnTiles[i].x, playerSpawnTiles[i].y);
            playerTile.SetOccupied(true);
            
            aiPlayers[i].transform.position = new Vector3(aiSpawnTiles[i].x, yPos, aiSpawnTiles[i].y);
            AIPlayer aiP = aiPlayers[i].GetComponent<AIPlayer>();
            aiP.SetGridPosition(aiSpawnTiles[i]);
            GridTile aiTile = GridGenerator.instance.GetTile(aiSpawnTiles[i].x, aiSpawnTiles[i].y);
            aiTile.SetOccupied(true);
        }

        if (scoredTeam == "Player")
        {
            SetBallPosition(playerSpawnTiles[0]);
            Player player = players[0].GetComponent<Player>();
            playerController.SetPlayerWithBall(player);
            StartCoroutine(DelayForTurn(StartPlayerTurn));
        }
        else
        {
            SetBallPosition(aiSpawnTiles[0]);
            AIPlayer ai = aiPlayers[0].GetComponent<AIPlayer>();
            aiPlayerController.SetPlayerWithBall(ai);
            StartCoroutine(DelayForTurn(StartAITurn));
        }
    }

    IEnumerator DelayForTurn(Action turn)
    {
        yield return new WaitForSeconds(3f);
        turn();
    }
    #endregion

   
}
