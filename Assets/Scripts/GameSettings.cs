using UnityEngine;

[CreateAssetMenu(fileName = "GameManagerSettings", menuName = "Scriptable Objects/GameManagerSettings")]
public class GameSettings : ScriptableObject
{
    public enum GameState
    {
        MatchStart,
        PlayerTurn,
        Waiting,
        AITurn,
        ResetMatch,
        MatchEnd
    }
    
    [Header("Game Settings")]
    public float turnDuration = 5f;
    
    [Header("Grid Settings")]
    public int gridHeight = 10;
    public int gridWidth = 10;
    public float spacing = 1f;
    public GameObject tilePrefab;
}
