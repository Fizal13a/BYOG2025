using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Score UI")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI AIScoreText;

    private int playerScore = 0;
    private int enemyScore = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddPlayerScore(int value = 1)
    {
        playerScore += value;
        UpdateScoreUI();
    }

    public void AddAIScore(int value = 1)
    {
        enemyScore += value;
        UpdateScoreUI();
    }
    public void ResetScores()
    {
        playerScore = 0;
        enemyScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        playerScoreText.text = playerScore.ToString();
        AIScoreText.text = enemyScore.ToString();
    }

    public int GetPlayerScore() => playerScore;
    public int GetEnemyScore() => enemyScore;
}
