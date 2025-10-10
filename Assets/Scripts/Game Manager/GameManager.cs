using UnityEngine;

public partial class GameManager : MonoBehaviour
{
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
    
    [Header("In Game Referances")]
    public PlayerController playerController;
    public AIPlayerController aiPlayerController;

    // Internal storage
    private GameObject[] players;
    private GameObject[] aiPlayers;
    private GameObject ball;

    #region Initialization

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
                Vector3 spawnPos = tile.transform.position + Vector3.up * 0.5f;
                players[i] = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                players[i].name = $"Player_{i}";
                players[i].transform.SetParent(playersParent);
                Player player  = players[i].GetComponent<Player>();
                playerController.AddPlayer(player);
            }
            
            GridTile tileScript = GridGenerator.instance.GetTile(gridPos.x, gridPos.y);
            tileScript.SetIsOccupied(true);
        }

        // --- Spawn AI ---
        for (int i = 0; i < aiSpawnTiles.Length; i++)
        {
            Vector2Int gridPos = aiSpawnTiles[i];
            GameObject tile = GridGenerator.instance.GetTileObject(gridPos.x, gridPos.y);

            if (tile)
            {
                Vector3 spawnPos = tile.transform.position + Vector3.up * 0.5f;
                aiPlayers[i] = Instantiate(aiPrefab, spawnPos, Quaternion.identity);
                aiPlayers[i].name = $"AI_{i}";
                aiPlayers[i].transform.SetParent(aisParent);
                AIPlayer ai  = players[i].GetComponent<AIPlayer>();
                aiPlayerController.AddAI(ai);
            }
            
            GridTile tileScript = GridGenerator.instance.GetTile(gridPos.x, gridPos.y);
            tileScript.SetIsOccupied(true);
        }

        // --- Spawn Ball ---
        GameObject ballTile = GridGenerator.instance.GetTileObject(ballSpawnTile.x, ballSpawnTile.y);
        if (ballTile)
        {
            Vector3 spawnPos = ballTile.transform.position + Vector3.up * 0.3f;
            ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
            ball.name = "Ball";
        }
        
        GridTile ballTileScript = GridGenerator.instance.GetTile(ballSpawnTile.x, ballSpawnTile.y);
        ballTileScript.SetIsOccupied(true);
        
        // --- Start State --- 
        StartPlayerTurn();
    }

    #endregion

   
}
