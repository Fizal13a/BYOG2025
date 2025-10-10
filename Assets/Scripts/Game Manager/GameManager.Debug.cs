using TMPro;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    [Header("Debug")] 
    public TextMeshProUGUI turnDisplayText;

    public TextMeshProUGUI turnTimerText;

    private void SetTurnDisplayText(string turn)
    {
        turnDisplayText.text = turn;
    }

    private void SetTurnTimerText(float timer)
    {
        turnTimerText.text = timer.ToString("F");
    }
}
