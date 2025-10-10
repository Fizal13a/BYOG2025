using UnityEngine;

public class AIPlayerController : MonoBehaviour
{
    [Header("AIs Data")]
    private AIPlayer[] ais;
    
    private bool isAITurn = false;

    public void SetUpAIs()
    {
        ais = new AIPlayer[3];
    }

    public void AddAI(AIPlayer player)
    {
        ais[0] = player;
    }

    public void SetUpTurn(bool turn)
    {
        isAITurn = turn;
    }
}
