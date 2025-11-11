using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TeamManager : MonoBehaviour
{
    public MatchSettings matchSettings;

    [Header("Events")] public static TeamEvents events;

    [Header("Team")] private Team team;

    [Header("Players")] public GameObject playerPrefab;
    public GameObject opponentPrefab;
    private List<Player> players = new List<Player>();

    [Header("Match")] [SerializeField] private LayerMask tileLayer;
    private bool isCurrectTurn = false;
    private Player currentSelectedPlayer;
    private Player currentPlayerWithBall;
    private Player currentPassTarget;

    [Header("Actions")] public List<ActionData> availableActions = new List<ActionData>();
    private ActionData currentAction;

    [Header("Condition bool")] private bool hasBall = false;
    private bool canSelect = false;
    private bool canPass = false;
    private bool canMove = false;
    private bool canTackle = false;
    private bool canShoot = false;
    
    [Header("AI")]
    private AIHandler aiHandler;
    private bool hasAI = false;

    #region Initialization

    private void Awake()
    {
        events = new TeamEvents();
    }

    public void InitializeTeam(Team team)
    {
        this.team = team;

        if (GameManager.instance.GetCurrentMode() == MatchData.MatchMode.Local && this.team.teamType == Team.TeamType.Opponent)
        {
            AIHandler ai = GetComponent<AIHandler>();
            if (ai != null)
            {
                aiHandler = ai;
                aiHandler.SetAI(this);
                hasAI = true;
            }
        }

        //events
        events.AddEvent<Team.TeamType>(TeamEvents.TeamEventType.CheckConditions, SetConditions);
        events.AddEvent(TeamEvents.TeamEventType.ResetPosition, ResetPositions);
        MatchManager.matchEvents.AddEvent<Team.TeamType>(MatchEvents.MatchEventType.OnTurnStart, OnTurnChange);

        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        players.Clear();

        for (int i = 0; i < matchSettings.maxTeamPlayers; i++)
        {
            if (team.teamType == Team.TeamType.Player)
            {
                GameObject player = Instantiate(playerPrefab, transform);
                player.name = "Player" + i;
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.SetUpPlayer(this, matchSettings.teamPositions[i]);
                    players.Add(playerScript);
                }
            }
            else
            {
                GameObject opponent = Instantiate(opponentPrefab, transform);
                opponent.name = "Opponent" + i;
                Player playerScript = opponent.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.SetUpPlayer(this, matchSettings.opponentPositions[i]);
                    players.Add(playerScript);
                }
            }
        }
    }

    #endregion

    #region Setter

    private void SetBallStatus(bool status)
    {
        hasBall = status;
    }

    public void SetCurrentPlayer(Player player)
    {
        currentSelectedPlayer = player;
    }

    private void OnTurnChange(Team.TeamType currTeam)
    {
        SetUpTurn(team.teamType == currTeam);
    }

    public void SetUpTurn(bool turn)
    {
        isCurrectTurn = turn;
        if (hasAI && aiHandler != null)
        {
            aiHandler.SetAITurn(isCurrectTurn);
        }
    }

    public void SetPlayerWithBall(Player player = null)
    {
        if (player == null) // Initial turn set ball
        {
            player = players[0];
            MatchManager.instance.Spawnball(players[0].GetGridPosition());
        }

        GameObject ball = MatchManager.instance.GetBallObject();
        DebugLogger.Log(ball.gameObject.name + ", " + player.gameObject.name, "yellow");
        ball.transform.SetParent(player.ballHolderPosition);
        ball.transform.localPosition = Vector3.zero;
        currentPlayerWithBall = player;

        currentPassTarget = null;
    }

    public void RemovePlayerWithBall()
    {
        currentPlayerWithBall = null;
    }

    public void SetConditions(Team.TeamType teamType)
    {
        if (team.teamType != teamType || currentSelectedPlayer == null) return;

        canPass = canMove = canShoot = canTackle = true;

        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();

        // Disable tackle if player with ball exists nearby
        if (currentPlayerWithBall != null || !IsBallAdjacent(playerGridPos)) canTackle = false;

        // Disable pass if player with ball not exists
        if (currentPlayerWithBall == null)
            canPass = false;

        else if (currentSelectedPlayer != currentPlayerWithBall) canPass = false;

        // Disable shoot if player doesn't have ball or near goal
        if (currentPlayerWithBall == null || !IsNearGoal(playerGridPos)) canShoot = false;

        Debug.Log($"{canPass} {canTackle} {canShoot}");
    }

    private void ResetPositions()
    {
        //TODO reset all players position
        for (int i = 0; i < players.Count; i++)
        {
            players[i].SetUpPlayer(this, players[i].GetOrigionalGridPosition());
        }
    }

    #endregion

    #region Getters

    public List<Player> GetAllPlayers()
    {
        return players;
    }

    public bool CheckBallStatus()
    {
        return hasBall;
    }

    public Player GetCurrentPlayerWithBall()
    {
        return currentPlayerWithBall;
    }

    public GridTile GetSelectedTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, tileLayer))
        {
            // Get the object we hit
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log("Clicked on: " + hit.collider.gameObject.name);

            if (clickedObject != null)
            {
                GridTile tile = clickedObject.GetComponent<GridTile>();
                if (tile != null && tile.isHighlighted)
                {
                    return tile;
                }
            }
        }
        else
        {
            Debug.Log("Didn't hit anything");
        }

        return null;
    }

    #endregion

    #region Checkers

    private bool IsBallAdjacent(Vector2Int playerGridPos)
    {
        Vector2Int[] directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int pos = new Vector2Int(playerGridPos.x + dir.x, playerGridPos.y + dir.y);
            GridTile tile = GridGenerator.instance.GetTile(pos);
            if (tile == null) continue;

            if (tile.GridPosition == MatchManager.instance.GetCurrentBallPosition()) return true;
        }

        return false;
    }

    private bool IsNearGoal(Vector2Int playerGridPos)
    {
        Vector2Int[] directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1), new(2, 0),
            new(-2, 0), new(0, 2), new(0, -2), new(2, 2), new(-2, 2), new(2, -2), new(-2, -2)
        };

        Vector2Int goalPos = GridGenerator.instance.matchSettings.teamGoalPosition;

        foreach (var dir in directions)
        {
            Vector2Int pos = new Vector2Int(playerGridPos.x + dir.x, playerGridPos.y + dir.y);
            GridTile tile = GridGenerator.instance.GetTile(pos);
            if (tile == null) continue;

            if (tile.GridPosition == goalPos) return true;
        }

        return false;
    }

    #endregion

    #region Actions Handling

    public void HandleStates(ActionData.Actions state, Player currPlayer)
    {
        StartCoroutine(HandleState(state, currPlayer));
    }

    private IEnumerator HandleState(ActionData.Actions state, Player currPlayer)
    {
        currentSelectedPlayer = currPlayer;
        currentAction = GetAction(state);

        // Clear all highlighted tiles
        GridGenerator.instance.ClearHighlightedTiles();
        // Checks for action points
        if (!MatchManager.instance.CheckActionPoints(currentAction.actionCost)) yield break;

        // Actions state handling
        switch (state)
        {
            case ActionData.Actions.Move:
                DebugLogger.Log("Player On Move State", "yellow");
                yield return StartCoroutine(CheckForTargetTileSelection());
                break;

            case ActionData.Actions.Pass:
                DebugLogger.Log("Player On Pass State", "yellow");
                yield return StartCoroutine(CheckForTargetPlayerSelection());
                break;

            case ActionData.Actions.Tackle:
                yield return StartCoroutine(Tackle());
                DebugLogger.Log("Player On Tackle State", "yellow");
                break;

            case ActionData.Actions.Shoot:
                yield return StartCoroutine(ShootToGoal());
                DebugLogger.Log("Player On Shoot State", "yellow");
                break;

            case ActionData.Actions.Dash:
                DebugLogger.Log("Player On Dash State", "yellow");
                break;
        }

        GridGenerator.instance.ClearHighlightedTiles();
        MatchManager.instance.CheckEndTurn();
    }

    private ActionData GetAction(ActionData.Actions action)
    {
        foreach (var act in availableActions)
        {
            DebugLogger.Log($"Action {act.action.ToString()}", "yellow");
            if (act.action == action)
            {
                DebugLogger.Log($"Player On Action {act.action.ToString()}", "yellow");
                return act;
            }
        }

        DebugLogger.Log($"Player action is null", "yellow");
        return null;
    }

    #endregion

    #region Action Execution

    #region MOVE

    private IEnumerator CheckForTargetTileSelection()
    {
        GridGenerator.instance.HighlightMoveTiles(currentSelectedPlayer);
        GridTile moveTargTile = null;
        canSelect = true;

        while (canSelect)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GridTile selectedTile = GetSelectedTile();
                if (selectedTile != null)
                {
                    moveTargTile = selectedTile;
                    canSelect = false;
                }
            }

            yield return null;
        }

        yield return StartCoroutine(ExecuteMove(moveTargTile));
    }

    public IEnumerator ExecuteMove(GridTile targetTile)
    {
        // Update tile occupancy
        GridTile currentTile = GridGenerator.instance.GetTile(currentSelectedPlayer.GetGridPosition());
        currentTile.SetOccupied(false);
        targetTile.SetOccupied(true);

        // Move player to target position
        Vector3 start = currentSelectedPlayer.transform.position;
        Vector3 end = new Vector3(targetTile.WorldPosition.x, start.y, targetTile.WorldPosition.z);

        float moveSpeed = currentSelectedPlayer.GetMoveSpeed();
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            currentSelectedPlayer.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Ensure final position is exact
        currentSelectedPlayer.transform.position = end;
        currentSelectedPlayer.SetGridPosition(targetTile.GridPosition);

        // Handle ball pickup
        if (targetTile.GridPosition == MatchManager.instance.GetCurrentBallPosition())
        {
            SetPlayerWithBall(currentSelectedPlayer);
        }

        // Update ball position if this player has it
        if (currentPlayerWithBall == currentSelectedPlayer)
        {
            MatchManager.instance.SetBallPosition(targetTile.GridPosition);
        }

        //GameManager.instance.CheckEndTurn();
        currentSelectedPlayer = null;

        yield return null;
    }

    #endregion

    #region PASS

    private IEnumerator CheckForTargetPlayerSelection()
    {
        foreach (var player in players)
        {
            if (player == currentPlayerWithBall) continue;

            Vector2Int positionIndex = player.GetGridPosition();
            GridGenerator.instance.HighlightPassTiles(positionIndex);
        }

        canSelect = true;

        GridTile tile = null;

        while (canSelect)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GridTile selectedTile = GetSelectedTile();
                if (selectedTile != null)
                {
                    tile = selectedTile;
                    canSelect = false;
                }
            }

            yield return null;
        }

        foreach (var player in players)
        {
            if (player.GetGridPosition() == tile.GridPosition)
            {
                currentPassTarget = player;
                currentPlayerWithBall = player;
                break;
            }
        }

        yield return StartCoroutine(BallPass(currentPassTarget.ballHolderPosition));
    }

    public IEnumerator BallPass(Transform targetTile)
    {
        if (targetTile == null) yield break;

        GameObject ball = MatchManager.instance.GetBallObject();
        ball.transform.SetParent(null);
        MatchManager.instance.SetBallPosition(currentPlayerWithBall.GetGridPosition());
        currentSelectedPlayer = null;

        yield return StartCoroutine(BallController.instance.MoveBall(targetTile));

        SetPlayerWithBall(currentPlayerWithBall);
    }

    #endregion
    
    #region TACKLE

    public IEnumerator Tackle()
    {
        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();
        MatchManager.instance.SetBallPosition(playerGridPos);
        SetPlayerWithBall(currentSelectedPlayer);
        //AIPlayerController.instance.RemoveBall();
        yield return null;
    }

    #endregion
    
    #region SHOOT

    public IEnumerator ShootToGoal()
    {
        Vector2Int tileIndex = GridGenerator.instance.matchSettings.teamGoalPosition;
        GridTile targetTile = GridGenerator.instance.GetTile(tileIndex);

        Transform targetPos = targetTile.transform;

        yield return StartCoroutine(BallShootToGoal(targetPos));

        currentSelectedPlayer = null;
    }

    IEnumerator BallShootToGoal(Transform targetTile)
    {
        if (targetTile == null) yield break;

        yield return StartCoroutine(BallController.instance.BallShoot(targetTile));
        MatchManager.matchEvents.TriggerEvent(MatchEvents.MatchEventType.OnTeamScored, team.teamType);
    }

    #endregion

    #endregion
}