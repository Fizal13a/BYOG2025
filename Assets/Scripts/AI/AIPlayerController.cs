using UnityEngine;

public partial class AIPlayerController : MonoBehaviour
{
    public enum AIStates
    {
        Move, Pass, Tackle, Shoot, Dash
    }

    [Header("Game Data")] public LayerMask playerLayer;
    
    [Header("AIs Data")]
    private AIPlayer[] ais;
    private bool hasBall = false;
    private AIPlayer currentAIWithBall = null;
    private AIPlayer currentSelectedAI = null;
    
    private bool isAITurn = false;

    public void SetUpAIs()
    {
        ais = new AIPlayer[3];
    }

    public void AddAI(AIPlayer player)
    {
        ais[0] = player;
    }

    public void HasBall(bool status)
    {
        hasBall = status;
    }

    public void SetPlayereWithBall(AIPlayer player)
    {
        currentAIWithBall = player;
    }

    public void SetSelectedPlayer(AIPlayer player)
    {
        currentAIWithBall = player;
    }

    public void SetUpTurn(bool turn)
    {
        isAITurn = turn;
        if(isAITurn) DetermineAIState();
    }
    
    private void DetermineAIState()
    {
        // If AI has ball
        if (currentAIWithBall != null)
        {
            bool enemyNearby = IsEnemyWithinRange(currentAIWithBall.transform.position, 2);

            // If near goal → shoot
            if (IsNearGoal(currentAIWithBall.transform.position, 2))
            {
                HandleStates(AIStates.Shoot);
                return;
            }

            // If enemies close → pass
            if (enemyNearby)
            {
                HandleStates(AIStates.Pass);
                return;
            }

            // Otherwise → move toward goal
            HandleStates(AIStates.Move);
            return;
        }

        // If AI doesn’t have ball
        AIPlayer nearestToBall = GetNearestAIToBall();
        if (nearestToBall == null) return;

        currentSelectedAI = nearestToBall;

        // If ball is adjacent (1 tile in any direction)
        if (IsBallNearby(nearestToBall.transform.position))
        {
            HandleStates(AIStates.Tackle);
            return;
        }

        // Otherwise, move toward ball or player with ball
        HandleStates(AIStates.Move);
    }
    
    private bool IsEnemyWithinRange(Vector3 position, int range)
    {
        Collider[] hits = Physics.OverlapSphere(position, range * 1, playerLayer);
        return hits.Length > 0;
    }

    private bool IsNearGoal(Vector3 position, int range)
    {
        GridTile goalTile = GridGenerator.instance.GetTile(6, 4);
        return Vector3.Distance(position, goalTile.transform.position) <= range * 1;
    }

    private bool IsBallNearby(Vector3 position)
    {
        return Vector3.Distance(position, BallController.instance.transform.position) <= 1;
    }

    private AIPlayer GetNearestAIToBall()
    {
        AIPlayer nearest = null;
        float minDist = float.MaxValue;
        foreach (var ai in ais)
        {
            float dist = Vector3.Distance(ai.transform.position, BallController.instance.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = ai;
            }
        }
        return nearest;
    }
}
