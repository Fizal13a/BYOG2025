using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchSettings", menuName = "Match/MatchSettings")]
public class MatchSettings : ScriptableObject
{
    [Header("Team")]
    public int maxTeamPlayers;
    public List<Vector2Int> teamPositions;
    public List<Vector2Int> opponentPositions;
    public Vector2Int teamGoalPosition;
    public Vector2Int opponentGoalPosition;
    
    [Header("Match")]
    public float halfTime;
}
