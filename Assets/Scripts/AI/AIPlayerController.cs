using System;
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
    public AIPlayer targetPlayer;
}

public partial class AIPlayerController : MonoBehaviour
{
    public static AIPlayerController instance;
    
    public List<ActionData> availableActions = new List<ActionData>();
    private ActionData currentAction;

    [Header("Game Data")] 
    public LayerMask playerLayer;
    
    [Header("AIs Data")]
    private AIPlayer[] ais;
    private bool hasBall = false;
    public AIPlayer currentAIWithBall = null;
    public AIPlayer currentSelectedAI = null;
    public AIPlayer currentPassTargetAI = null;
    [SerializeField] private GridTile goalTile;
    private bool isAITurn = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetUpAIs()
    {
        ais = new AIPlayer[3];
        goalTile = GameManager.instance.GetAIGoalTile();
    }

    public void AddAI(AIPlayer player, Vector2Int gridPos, int index)
    {
        if(index < 3)
        {
            ais[index] = player;
            player.SetUpPlayer(this, gridPos);
        }
    }

    public void RemoveBall()
    {
        if (hasBall && currentAIWithBall != null)
        {
            hasBall = false;
            currentAIWithBall = null;
        }
    }

    public void HasBall(bool status)
    {
        hasBall = status;
    }

    public void SetPlayerWithBall(AIPlayer player)
    {
        GameObject ball = GameManager.instance.GetBallObject();
        ball.transform.SetParent(player.ballHolderPosition);
        ball.transform.localPosition = Vector3.zero;
        GameManager.instance.SetBallPosition(player.GetGridPosition());
        currentAIWithBall = player;
        hasBall = true;
        currentPassTargetAI = null;
    }

    public void SetSelectedPlayer(AIPlayer player)
    {
        currentSelectedAI = player;
    }

    public void SetUpTurn(bool turn)
    {
        isAITurn = turn;
        if (isAITurn)
        {
            StartCoroutine(DelayAITurnStart());
        }
    }

    IEnumerator DelayAITurnStart()
    {
        yield return new WaitForSeconds(1);
        if (isAITurn) // Double check turn hasn't ended
        {
            StartCoroutine(PlayAITurn());
        }
    }

    IEnumerator PlayAITurn()
    {
        while (isAITurn)
        {
            // Check if turn is still active
            if (!isAITurn)
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
            yield return StartCoroutine(ExecuteAction(strategy));

            // Check if turn is still active after action
            if (!isAITurn)
            {
                Debug.Log("AI: Turn ended during action execution");
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        // Final check to end turn properly
        if (isAITurn)
        {
            isAITurn = false;
            GameManager.instance.CheckEndTurn();
        }
    }

    private AIStrategy EvaluateBestMove()
    {
        List<AIStrategy> strategies = new List<AIStrategy>();

        // Evaluate all possible moves
        if (hasBall && currentAIWithBall != null)
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

        GridTile aiTile = GridGenerator.instance.GetTile(
            currentAIWithBall.GetGridPosition().x, 
            currentAIWithBall.GetGridPosition().y);
        
        int distToGoal = ManhattanDistance(aiTile.GridPosition, goalTile.GridPosition);

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

        AIPlayer passTarget = FindAdjacentTeammate();
        
        if (passTarget == null)
            return;

        float score = 0;

        if (HasAdjacentEnemy(currentAIWithBall))
        {
            score = 800;
            Debug.Log("AI Pass Option: Enemies nearby! Score: 800");
        }
        else
        {
            GridTile targetTile = GridGenerator.instance.GetTile(
                passTarget.GetGridPosition().x, 
                passTarget.GetGridPosition().y);
            
            int targetDistToGoal = ManhattanDistance(targetTile.GridPosition, goalTile.GridPosition);
            int currentDistToGoal = ManhattanDistance(currentAIWithBall.GetGridPosition(), goalTile.GridPosition);

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

        if (hasBall && currentAIWithBall != null)
        {
            GridTile aiTile = GridGenerator.instance.GetTile(
                currentAIWithBall.GetGridPosition().x, 
                currentAIWithBall.GetGridPosition().y);
            
            int distToGoal = ManhattanDistance(aiTile.GridPosition, goalTile.GridPosition);

            if (!HasAdjacentEnemy(currentAIWithBall))
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
            Vector2Int ballPos = GameManager.instance.GetCurrentBallPosition();
            AIPlayer closestAI = GetClosestAIToBall();

            if (closestAI != null)
            {
                int distToBall = ManhattanDistance(closestAI.GetGridPosition(), ballPos);
                score = 400 - (distToBall * 50);
                currentSelectedAI = closestAI;
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

        Vector2Int ballPos = GameManager.instance.GetCurrentBallPosition();
        GridTile ballTile = GridGenerator.instance.GetTile(ballPos.x, ballPos.y);

        AIPlayer tackler = GetAIAdjacentToBall(ballTile);

        if (tackler != null)
        {
            currentSelectedAI = tackler;
            
            int distToGoal = ManhattanDistance(ballPos, goalTile.GridPosition);
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

    IEnumerator ExecuteAction(AIStrategy strategy)
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
        
        GameManager.instance.CheckEndTurn();

        // After action, check if we should continue
        if (!isAITurn)
        {
            Debug.Log("AI: Turn became inactive after action");
            yield break;
        }
    }

    IEnumerator ExecuteMove()
    {
        if (!GameManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Move)))
        {
            yield break;
        }

        if (hasBall && currentAIWithBall != null)
        {
            GridTile currentTile = GridGenerator.instance.GetTile(
                currentAIWithBall.GetGridPosition().x, 
                currentAIWithBall.GetGridPosition().y);
            
            GridTile nextTile = GetNextTileToward(currentTile, goalTile);

            if (nextTile != null)
            {
                Debug.Log($"{currentAIWithBall.name} moving toward goal");
                MoveToTile(currentAIWithBall, nextTile);
                GameManager.instance.SetBallPosition(nextTile.GridPosition);
            }
        }
        else if (currentSelectedAI != null)
        {
            Vector2Int ballPos = GameManager.instance.GetCurrentBallPosition();
            GridTile ballTile = GridGenerator.instance.GetTile(ballPos.x, ballPos.y);
            
            GridTile currentTile = GridGenerator.instance.GetTile(
                currentSelectedAI.GetGridPosition().x, 
                currentSelectedAI.GetGridPosition().y);
            
            GridTile nextTile = GetNextTileToward(currentTile, ballTile);

            if (nextTile != null)
            {
                Debug.Log($"{currentSelectedAI.name} moving toward ball");
                MoveToTile(currentSelectedAI, nextTile);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    IEnumerator ExecutePass(AIPlayer receiver)
    {
        if (!GameManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Pass)))
        {
            yield break;
        }

        if (currentAIWithBall == null || receiver == null)
            yield break;

        Debug.Log($"{currentAIWithBall.name} passing to {receiver.name}");

        //#region Play Animation

        //Animator animator = currentAIWithBall.GetComponentInChildren<Animator>();
        //AnimationManager.Instance.PassAnim(animator);

        //#endregion

        GridTile targetTile = GridGenerator.instance.GetTile(
            receiver.GetGridPosition().x, 
            receiver.GetGridPosition().y);

        currentPassTargetAI = receiver;
        yield return StartCoroutine(MoveBallInArc(
            BallController.instance.gameObject.transform, 
            targetTile.transform.position));

        SetPlayerWithBall(receiver);
        
        yield return new WaitForSeconds(2f);
    }

    IEnumerator ExecuteTackle()
    {
        if (!GameManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Tackle)))
        {
            yield break;
        }

        if (currentSelectedAI == null)
            yield break;

        Debug.Log($"{currentSelectedAI.name} tackling for the ball");


        //#region Play Animation

        //Animator animator = currentSelectedAI.GetComponentInChildren<Animator>();
        //AnimationManager.Instance.TackleAnim(animator);

        //#endregion

        SetPlayerWithBall(currentSelectedAI);
        PlayerController.instance.RemovePlayerWithBall();
        GameManager.instance.SetBallPosition(currentSelectedAI.GetGridPosition());

        yield return new WaitForSeconds(2f);
    }

    IEnumerator ExecuteShoot()
    {
        if (!GameManager.instance.CheckActionPoints(GetActionCost(ActionData.Actions.Shoot)))
        {
            yield break;
        }

        if (currentAIWithBall == null)
            yield break;

        Debug.Log($"{currentAIWithBall.name} shooting at goal!");

        //#region Play Animation
        //Animator animator = currentAIWithBall.GetComponentInChildren<Animator>();
        //AnimationManager.Instance.ShootAnim(animator);
        //#endregion

        GameObject ball = GameManager.instance.GetBallObject();
        Transform goalTileTrans = GameManager.instance.GetAIGoalTile().transform;

        yield return StartCoroutine(MoveBallToShoot(
            ball.transform, 
            new Vector3(goalTileTrans.position.x, goalTileTrans.position.y, goalTileTrans.position.z)));

        UIManager.instance.AddAIScore(1);
        GameManager.instance.ResetRound();
    }

    public IEnumerator MoveBallInArc(Transform ball, Vector3 targetPosition, float arcHeight = 2f, float duration = 1f)
    {
        Vector3 startPos = ball.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, targetPosition, smoothT);
            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;
            pos.y += heightOffset + 0.3f;

            ball.position = pos;
            yield return null;
        }

        ball.position = targetPosition;
    }

    public IEnumerator MoveBallToShoot(Transform ball, Vector3 targetPosition, float arcHeight = 2f, float duration = 1f)
    {
        Vector3 startPos = ball.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, targetPosition, smoothT);
            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;
            pos.y += heightOffset + 0.3f;

            ball.position = pos;
            yield return null;
        }

        ball.position = targetPosition;
    }

    // === Helper Methods ===

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
        foreach (var actionData in availableActions)
        {
            if (actionData.action == action)
                return actionData.actionCost;
        }
        return 0;
    }

    private AIPlayer FindAdjacentTeammate()
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = currentAIWithBall.GetGridPosition() + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos.x, checkPos.y);
            
            if (tile != null && tile.IsOccupied())
            {
                AIPlayer occupant = GetAIAtPosition(checkPos);
                if (occupant != null && occupant != currentAIWithBall)
                    return occupant;
            }
        }

        return null;
    }

    private bool HasAdjacentEnemy(AIPlayer ai)
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = ai.GetGridPosition() + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos.x, checkPos.y);
            
            if (tile != null && tile.IsOccupied())
            {
                AIPlayer occupant = GetAIAtPosition(checkPos);
                if (occupant != null && occupant != ai)
                    return true;
            }
        }

        return false;
    }

    private AIPlayer GetAIAdjacentToBall(GridTile ballTile)
    {
        Vector2Int[] directions = GetAdjacentDirections();

        foreach (var dir in directions)
        {
            Vector2Int checkPos = ballTile.GridPosition + dir;
            AIPlayer ai = GetAIAtPosition(checkPos);
            if (ai != null)
                return ai;
        }

        return null;
    }

    private AIPlayer GetClosestAIToBall()
    {
        Vector2Int ballPos = GameManager.instance.GetCurrentBallPosition();
        AIPlayer closest = null;
        int minDist = int.MaxValue;

        foreach (AIPlayer ai in ais)
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

    private AIPlayer GetAIAtPosition(Vector2Int pos)
    {
        foreach (AIPlayer ai in ais)
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

        // Try diagonal
        if (moveX != 0 && moveY != 0)
        {
            GridTile diag = GridGenerator.instance.GetTile(from.GridPosition.x + moveX, from.GridPosition.y + moveY);
            if (diag != null && !diag.IsOccupied())
                return diag;
        }

        // Try horizontal
        if (moveX != 0)
        {
            GridTile horiz = GridGenerator.instance.GetTile(from.GridPosition.x + moveX, from.GridPosition.y);
            if (horiz != null && !horiz.IsOccupied())
                return horiz;
        }

        // Try vertical
        if (moveY != 0)
        {
            GridTile vert = GridGenerator.instance.GetTile(from.GridPosition.x, from.GridPosition.y + moveY);
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
}