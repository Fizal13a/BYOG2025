using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    
    [Header("Referances")]
    public GameObject ballPrefab;
    
    [Header("Match Data")] 
    public static MatchEvents matchEvents;
    private Team.TeamType scoredTeam;
    private int playerTeamScore = 0;
    private int opponentTeamScore = 0;
    
    [Header("Ball")]
    private GameObject ballObject;
    private Vector2Int currentBallPosition;

    [Header("Team Data")] 
    public GameObject teamPrefab;
    public GameObject aiTeamPrefab;

    private TeamManager playerTeam;
    private TeamManager opponentTeam;
    
    private TeamManager currentTeamOnPlay;
    
    #region Initialize

    private void Awake()
    {
        if (instance == null) instance = this;
        
        matchEvents = new MatchEvents();
        
        matchEvents.AddEvent<Team.TeamType>(MatchEvents.MatchEventType.OnTeamScored, SetScoredTeam);
        matchEvents.AddEvent(MatchEvents.MatchEventType.OnRoundReset, ResetRound);
        matchEvents.AddEvent(MatchEvents.MatchEventType.OnRoundReset, StopAllTurns);
        matchEvents.AddEvent<Team.TeamType>(MatchEvents.MatchEventType.OnTurnStart,SetCurrentTeamOnPlay);
        DebugLogger.Log("Events added");
        
        SetUpTurnStates();
    }

    private void Start()
    {
        InitializeMatch();
    }

    // --- Initialize Match ---
    public void InitializeMatch()
    {
        //Generate Grid
        GridGenerator.instance.GenerateGrid();
        
        //Spawn teams
        SpawnTeams();
        
        // Random Turn
        int randomTurn = Random.Range(0, 10);
        bool isPlayerTurn = randomTurn < 5;
        
        // Start turn with delay
        StartCoroutine(StartGameDelay(isPlayerTurn));
    }
    
    IEnumerator StartGameDelay(bool isPlayerTurn)
    {
        yield return new WaitForSeconds(2f);
        
        if (isPlayerTurn)
        {
            playerTeam.SetPlayerWithBall();
            StartTurn(playerTurnHandler);
        }
        else
        {
            opponentTeam.SetPlayerWithBall();
            StartTurn(opponentTurnHandler);
        }
    }
    
    #endregion

    #region Getters

    public Vector2Int GetCurrentBallPosition()
    {
        return currentBallPosition;
    }

    public GameObject GetBallObject()
    {
        return ballObject;
    }

    #endregion

    #region Setters

    public void SetBallPosition(Vector2Int position)
    {
        currentBallPosition = position;
    }

    public void SetBallObject(GameObject ball)
    {
        ballObject = ball;
    }

    public void SetScoredTeam(Team.TeamType team)
    {
        scoredTeam = team;
        matchEvents.TriggerEvent(MatchEvents.MatchEventType.OnRoundReset);
    }

    public void SetCurrentTeamOnPlay(Team.TeamType team)
    {
        currentTeamOnPlay = (team == Team.TeamType.Player) ? playerTeam : opponentTeam;
        DebugLogger.Log("CurrentTeamOnPlay" + currentTeamOnPlay.ToString(), "yellow");
    }
    
    #endregion

    #region Spawners

    private void SpawnTeams()
    {
        GameObject oppTeamPrefab = (GameManager.instance.GetCurrentMode() == MatchData.MatchMode.PlayWithFriend) ? teamPrefab : aiTeamPrefab;
        
        //Spawn Player Team
        GameObject plTeam = Instantiate(teamPrefab, transform);
        plTeam.name = "Player Team";
        Team pTeam = new Team();
        pTeam.teamName = "Player Team";
        pTeam.teamType = Team.TeamType.Player;
        TeamManager pTeamManager = plTeam.GetComponent<TeamManager>();
        playerTeam = pTeamManager;
        
        //Spawn Opponent Team
        GameObject oppTeam = Instantiate(oppTeamPrefab, transform);
        oppTeam.name = "Opponent Team";
        Team oTeam = new Team();
        oTeam.teamName = "Opponent Team";
        oTeam.teamType = Team.TeamType.Opponent;
        TeamManager oTeamManager = oppTeam.GetComponent<TeamManager>();
        opponentTeam = oTeamManager;

        //Initialize Teams 
        pTeamManager.InitializeTeam(pTeam);
        oTeamManager.InitializeTeam(oTeam);
    }

    public void Spawnball(Vector2Int position)
    {
        Vector3 spawnPosition = new Vector3(position.x, 0.5f, position.y);
        ballObject = Instantiate(ballPrefab, transform);
        ballObject.transform.position = spawnPosition;
        
        SetBallPosition(position);
    }

    #endregion

    private void DebugFuntion(Team.TeamType  team)
    {
        DebugLogger.Log("Debug function", "green");
    }
}