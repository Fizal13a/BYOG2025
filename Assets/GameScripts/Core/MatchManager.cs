using System;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    
    [Header("Match Data")] 
    public static MatchEvents matchEvents;
    
    [Header("Ball")]
    private GameObject ballObject;
    private Vector2Int currentBallPosition;

    [Header("Team Data")] 
    public GameObject teamPrefab;

    private TeamManager playerTeam;
    private TeamManager opponentTeam;
    

    #region Initialize

    private void Awake()
    {
        if (instance == null) instance = this;
        
        matchEvents = new MatchEvents();
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
        
        //Spawn Player Team
        GameObject plTeam = Instantiate(teamPrefab, transform);
        plTeam.name = "Player Team";
        Team pTeam = new Team();
        pTeam.teamName = "Player Team";
        pTeam.teamType = Team.TeamType.Player;
        TeamManager pTeamManager = plTeam.GetComponent<TeamManager>();
        pTeamManager.InitializeTeam(pTeam);
        playerTeam = pTeamManager;
        
        //Spawn Opponent Team
        GameObject oppTeam = Instantiate(teamPrefab, transform);
        oppTeam.name = "Opponent Team";
        Team oTeam = new Team();
        oTeam.teamName = "Opponent Team";
        oTeam.teamType = Team.TeamType.Opponent;
        TeamManager oTeamManager = oppTeam.GetComponent<TeamManager>();
        oTeamManager.InitializeTeam(oTeam);
        opponentTeam = oTeamManager;
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

    #endregion
}