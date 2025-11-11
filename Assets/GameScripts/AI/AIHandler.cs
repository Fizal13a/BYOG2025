using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStrategy
{
    public enum DecisionType
    {
        Shoot,
        Pass,
        Move,
        Tackle,
        EndTurn
    }

    public DecisionType decision;
    public float score;
    public Player targetPlayer;
}

public class AIHandler : MonoBehaviour
{
    public List<ActionData> availableActions = new List<ActionData>();
    private ActionData currentAction;

    private TeamManager teamManager;
    
    [Header("Game Data")] 
    public LayerMask playerLayer;

    private Player currentSelectedAI;
    
    public void SetAI(TeamManager team)
    {
        teamManager =  team;
    }

    public void SetAITurn(bool turn)
    {
        DebugLogger.Log("AI TURN - " + turn, "cyan");
        StartCoroutine(PlayAITurn(turn));
    }
    
    IEnumerator PlayAITurn(bool turn)
    {
        while (turn)
        {
            // Check if turn is still active
            if (!turn)
            {
                Debug.Log("AI: Turn ended, stopping execution");
                break;
            }

            // Get best decision for current state
            AIStrategy strategy = EvaluateBestMove();

            if (strategy.decision == AIStrategy.DecisionType.EndTurn)
            {
                Debug.Log("AI: No more viable moves");
                break;
            }

            // Execute the decided action
            yield return StartCoroutine(ExecuteAction(strategy, turn));

            // Check if turn is still active after action
            if (!turn)
            {
                Debug.Log("AI: Turn ended during action execution");
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        // Final check to end turn properly
        if (turn)
        {
            turn = false;
            MatchManager.instance.CheckEndTurn();
        }
    }

    #region Evaluation

    private AIStrategy EvaluateBestMove()
    {
        List<AIStrategy> strategies = new List<AIStrategy>();

        // Evaluate all possible moves
        if (teamManager.CheckBallStatus() && teamManager.GetCurrentPlayerWithBall() != null)
        {
            // AI has ball - prioritize scoring or advancing
            EvaluateShoot(strategies);
            EvaluatePass(strategies);
            EvaluateMove(strategies);
        }
        else
        {
            // AI doesn't have ball - get it or move toward it
            EvaluateTackle(strategies);
            EvaluateMove(strategies);
        }

        // If no valid strategies, end turn
        if (strategies.Count == 0)
        {
            return new AIStrategy { decision = AIStrategy.DecisionType.EndTurn, score = 0 };
        }

        // Return highest scoring strategy
        AIStrategy best = strategies[0];
        foreach (var strategy in strategies)
        {
            if (strategy.score > best.score)
                best = strategy;
        }

        return best;
    }

    private void EvaluateShoot(List<AIStrategy> strategies)
    {
        if (!HasActionPoints(ActionData.Actions.Shoot))
            return;

        Vector2Int playerPos = teamManager.GetCurrentPlayerWithBall().GetGridPosition();
        GridTile aiTile = GridGenerator.instance.GetTile(playerPos);
        
        int distToGoal = ManhattanDistance(aiTile.GridPosition, GridGenerator.instance.GetOpponentGoalTile().GridPosition);

        // Only shoot if within 2 tiles of goal
        if (distToGoal <= 2)
        {
            float score = 1000 - (distToGoal * 100);
            strategies.Add(new AIStrategy 
            { 
                decision = AIStrategy.DecisionType.Shoot, 
                score = score,
                targetPlayer = null
            });
            Debug.Log($"AI Shoot Option: Distance to goal {distToGoal}, Score: {score}");
        }
    }

    private void EvaluatePass(List<AIStrategy> strategies)
    {
        if (!HasActionPoints(ActionData.Actions.Pass))
            return;

        Player passTarget = FindAdjacentTeammate();
        
        if (passTarget == null)
            return;

        float score = 0;

        if (HasAdjacentEnemy(teamManager.GetCurrentPlayerWithBall()))
        {
            score = 800;
            Debug.Log("AI Pass Option: Enemies nearby! Score: 800");
        }
        else
        {
            GridTile targetTile = GridGenerator.instance.GetTile(passTarget.GetGridPosition());
            
            int targetDistToGoal = ManhattanDistance(targetTile.GridPosition, GridGenerator.instance.GetOpponentGoalTile().GridPosition);
            int currentDistToGoal = ManhattanDistance(teamManager.GetCurrentPlayerWithBall().GetGridPosition(), GridGenerator.instance.GetOpponentGoalTile().GridPosition);
            
            if (targetDistToGoal < currentDistToGoal)
            {
                score = 300;
                Debug.Log($"AI Pass Option: Target closer to goal. Score: {score}");
            }
        }

        if (score > 0)
        {
            strategies.Add(new AIStrategy 
            { 
                decision = AIStrategy.DecisionType.Pass, 
                score = score,
                targetPlayer = passTarget
            });
        }
    }

    private void EvaluateMove(List<AIStrategy> strategies)
    {
        if (!HasActionPoints(ActionData.Actions.Move))
            return;

        float score = 0;

        if (teamManager.CheckBallStatus() && teamManager.GetCurrentPlayerWithBall() != null)
        {
            GridTile aiTile = GridGenerator.instance.GetTile(
                teamManager.GetCurrentPlayerWithBall().GetGridPosition());
            
            int distToGoal = ManhattanDistance(aiTile.GridPosition, GridGenerator.instance.GetOpponentGoalTile().GridPosition);

            if (!HasAdjacentEnemy(teamManager.GetCurrentPlayerWithBall()))
            {
                score = 500 - (distToGoal * 50);
                Debug.Log($"AI Move Option: Advancing toward goal. Distance: {distToGoal}, Score: {score}");
            }
            else
            {
                Debug.Log("AI Move Option: Blocked by enemies, low priority");
                score = 100;
            }
        }
        else
        {
            Vector2Int ballPos = MatchManager.instance.GetCurrentBallPosition();
            Player closestAI = GetClosestAIToBall();

            if (closestAI != null)
            {
                int distToBall = ManhattanDistance(closestAI.GetGridPosition(), ballPos);
                score = 400 - (distToBall * 50);
                SetSelectedPlayer(closestAI);
                Debug.Log($"AI Move Option: Moving toward ball. Distance: {distToBall}, Score: {score}");
            }
        }

        if (score > 0)
        {
            strategies.Add(new AIStrategy 
            { 
                decision = AIStrategy.DecisionType.Move, 
                score = score,
                targetPlayer = null
            });
        }
    }

    private void EvaluateTackle(List<AIStrategy> strategies)
    {
        if (!HasActionPoints(ActionData.Actions.Tackle))
            return;

        Vector2Int ballPos = MatchManager.instance.GetCurrentBallPosition();
        GridTile ballTile = GridGenerator.instance.GetTile(ballPos);

        Player tackler = GetAIAdjacentToBall(ballTile);

        if (tackler != null)
        {
            SetSelectedPlayer(tackler);
            
            int distToGoal = ManhattanDistance(ballPos, GridGenerator.instance.GetOpponentGoalTile().GridPosition);
            float score = 700 - (distToGoal * 50);
            
            strategies.Add(new AIStrategy 
            { 
                decision = AIStrategy.DecisionType.Tackle, 
                score = score,
                targetPlayer = tackler
            });
            Debug.Log($"AI Tackle Option: Ball near goal. Distance: {distToGoal}, Score: {score}");
        }
    }

    #endregion

    #region Execution

    IEnumerator ExecuteAction(AIStrategy strategy, bool isAITurn)
    {
        // Safety check: verify turn is still active
        if (!isAITurn)
            yield break;

        switch (strategy.decision)
        {
            case AIStrategy.DecisionType.Shoot:
                yield return StartCoroutine(ExecuteShoot());
                break;
            case AIStrategy.DecisionType.Pass:
                yield return StartCoroutine(ExecutePass(strategy.targetPlayer));
                break;
            case AIStrategy.DecisionType.Move:
                yield return StartCoroutine(ExecuteMove());
                break;
            case AIStrategy.DecisionType.Tackle:
                yield return StartCoroutine(ExecuteTackle());
                break;
        }
        
        MatchManager.instance.CheckEndTurn();

        // After action, check if we should continue
        if (!isAITurn)
        {
            Debug.Log("AI: Turn became inactive after action");
            yield break;
        }
    }
    
    IEnumerator ExecuteMove()
    {
        if (!MatchManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Move)))
        {
            yield break;
        }

        if (teamManager.CheckBallStatus() && teamManager.GetCurrentPlayerWithBall() != null)
        {
            GridTile currentTile =
                GridGenerator.instance.GetTile(teamManager.GetCurrentPlayerWithBall().GetGridPosition());
               
            
            GridTile nextTile = GetNextTileToward(currentTile, GridGenerator.instance.GetOpponentGoalTile());

            if (nextTile != null)
            {
                Debug.Log($"{teamManager.GetCurrentPlayerWithBall().name} moving toward goal");
                yield return StartCoroutine(teamManager.ExecuteMove(nextTile));
            }
        }
        else if (currentSelectedAI != null)
        {
            Vector2Int ballPos = MatchManager.instance.GetCurrentBallPosition();
            GridTile ballTile = GridGenerator.instance.GetTile(ballPos);
            
            GridTile currentTile = GridGenerator.instance.GetTile(
                currentSelectedAI.GetGridPosition());
            
            GridTile nextTile = GetNextTileToward(currentTile, ballTile);

            if (nextTile != null)
            {
                Debug.Log($"{currentSelectedAI.name} moving toward ball");
                yield return StartCoroutine(teamManager.ExecuteMove(nextTile));
            }
        }
    }

    IEnumerator ExecutePass(Player receiver)
    {
        if (!MatchManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Pass)))
        {
            yield break;
        }
        
        yield return StartCoroutine(teamManager.BallPass(receiver.ballHolderPosition));
        yield break;
    }

    IEnumerator ExecuteTackle()
    {
        if (!MatchManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Tackle)))
        {
            yield break;
        }
        
        yield return StartCoroutine(teamManager.Tackle());
        yield return new WaitForSeconds(2f);
    }

    IEnumerator ExecuteShoot()
    {
        if (!MatchManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Shoot)))
        {
            yield break;
        }
        
        yield return StartCoroutine(teamManager.ShootToGoal());
        yield break;
    }

    #endregion

    #region Getters
    
    private int ManhattanDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }

    private bool HasActionPoints(ActionData.Actions action)
    {
        return GetActionCost(action) > 0;
    }

    private int GetActionCost(ActionData.Actions action)
    {
        foreach (var actionData in teamManager.availableActions)
        {
            if (actionData.action == action)
                return actionData.actionCost;
        }
        return 0;
    }
    
    private Player FindAdjacentTeammate()
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = teamManager.GetCurrentPlayerWithBall().GetGridPosition() + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos);
            
            if (tile != null && tile.IsOccupied())
            {
                Player occupant = GetAIAtPosition(checkPos);
                if (occupant != null && occupant != teamManager.GetCurrentPlayerWithBall())
                    return occupant;
            }
        }

        return null;
    }
    
     private bool HasAdjacentEnemy(Player ai)
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = ai.GetGridPosition() + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos);
            
            if (tile != null && tile.IsOccupied())
            {
                Player occupant = GetAIAtPosition(checkPos);
                if (occupant != null && occupant != ai)
                    return true;
            }
        }

        return false;
    }

    private Player GetAIAdjacentToBall(GridTile ballTile)
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = ballTile.GridPosition + dir;
            Player ai = GetAIAtPosition(checkPos);
            if (ai != null)
                return ai;
        }

        return null;
    }

    private Player GetClosestAIToBall()
    {
        Vector2Int ballPos = MatchManager.instance.GetCurrentBallPosition();
        Player closest = null;
        int minDist = int.MaxValue;

        foreach (Player ai in teamManager.GetAllPlayers())
        {
            if (ai == null) continue;
            
            int dist = ManhattanDistance(ai.GetGridPosition(), ballPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = ai;
            }
        }

        return closest;
    }

    private Player GetAIAtPosition(Vector2Int pos)
    {
        foreach (Player ai in teamManager.GetAllPlayers())
        {
            if (ai != null && ai.GetGridPosition() == pos)
                return ai;
        }
        return null;
    }

    private GridTile GetNextTileToward(GridTile from, GridTile to)
    {
        Vector2Int dir = to.GridPosition - from.GridPosition;
        int moveX = dir.x != 0 ? (dir.x > 0 ? 1 : -1) : 0;
        int moveY = dir.y != 0 ? (dir.y > 0 ? 1 : -1) : 0;
        
        Vector2Int direc =  new Vector2Int(0, 0);

        // Try diagonal
        if (moveX != 0 && moveY != 0)
        {
            direc = new Vector2Int(from.GridPosition.x + moveX, from.GridPosition.y + moveY);
            GridTile diag = GridGenerator.instance.GetTile(direc);
            if (diag != null && !diag.IsOccupied())
                return diag;
        }
        
        // Try horizontal
        if (moveX != 0)
        {
            direc = new Vector2Int(from.GridPosition.x + moveX, from.GridPosition.y);
            GridTile horiz = GridGenerator.instance.GetTile(direc);
            if (horiz != null && !horiz.IsOccupied())
                return horiz;
        }
        
        // Try vertical
        if (moveY != 0)
        {
            direc = new Vector2Int(from.GridPosition.x, from.GridPosition.y + moveY);
            GridTile vert = GridGenerator.instance.GetTile(direc);
            if (vert != null && !vert.IsOccupied())
                return vert;
        }

        return null;
    }

    private Vector2Int[] GetAdjacentDirections()
    {
        return new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1)
        };
    }

    #endregion

    private void SetSelectedPlayer(Player player)
    {
        teamManager.SetCurrentPlayer(player);
    }
}
