using TMPro;
using UnityEngine;

public partial class MatchManager : MonoBehaviour
{
    [Header("UIs")]
    public TextMeshProUGUI turnDisplayText;
    public TextMeshProUGUI actionPointDisplayText;
    public TextMeshProUGUI playerTeamScoreDisplayText;
    public TextMeshProUGUI opponentTeamScoreDisplayText;

    public void SetTurnDisplayText(string status)
    {
        turnDisplayText.text = status;
    }
    
    public void SetActionPointsDisplayText(int status)
    {
        actionPointDisplayText.text = status.ToString();
    }

    public void SetPlayerScore()
    {
        playerTeamScore++;
        playerTeamScoreDisplayText.text = playerTeamScore.ToString();
    }

    public void SetOpponentScore()
    {
        playerTeamScore++;
        playerTeamScoreDisplayText.text = playerTeamScore.ToString();
    }
}
