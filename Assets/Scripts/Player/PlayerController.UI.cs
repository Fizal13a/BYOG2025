using UnityEngine;
using UnityEngine.UI;

public partial class PlayerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject playerUIPanel;

    #region Buttons
    [SerializeField] private Button moveButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button tackleButton;
    [SerializeField] private Button shootButton;
    [SerializeField] private Button dashButton;
    #endregion

    #region Initialization

    private void SetUpUI()
    {
        //InitializeButtonEvents();
    }

    private void InitializeButtonEvents()
    {
        moveButton.onClick.RemoveAllListeners();
        passButton.onClick.RemoveAllListeners();
        tackleButton.onClick.RemoveAllListeners();
        shootButton.onClick.RemoveAllListeners();
        dashButton.onClick.RemoveAllListeners();

        //moveButton.onClick.AddListener(() => HandleStates(availableActions[0].action));
        //passButton.onClick.AddListener(() => HandleStates(availableActions[1].action));
        //tackleButton.onClick.AddListener(() => HandleStates(availableActions[2].action));
        //shootButton.onClick.AddListener(() => HandleStates(availableActions[3].action));
        //dashButton.onClick.AddListener(() => HandleStates(availableActions[4].action));
    }

    #endregion

    #region UI Control

    private void ToggleUI(bool state)
    {
        if(currentSelectedPlayer == null) return;
        
        // Enable/disable UI
        playerUIPanel.SetActive(state);

        // Default button states
        moveButton.interactable = true;
        passButton.interactable = true;
        tackleButton.interactable = true;
        shootButton.interactable = true;
        dashButton.interactable = false;

        if (currentSelectedPlayer == null) return;

        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();

        // Disable tackle button if player with ball exists nearby
        if (currentPlayerWithBall != null || !IsBallAdjacent(playerGridPos))
            tackleButton.interactable = false;
        
        // Disable pass button if player with ball not exists
        if (currentPlayerWithBall == null)
        {
            passButton.interactable = false;
        }
        else if(currentSelectedPlayer != currentPlayerWithBall)
        {
            passButton.interactable = false;
        }

        // Disable shoot button if player doesn't have ball or near goal
        if (currentPlayerWithBall == null || !IsNearGoal(playerGridPos))
            shootButton.interactable = false;
    }

    private bool IsBallAdjacent(Vector2Int playerGridPos)
    {
        Vector2Int[] directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)
        };

        foreach (var dir in directions)
        {
            GridTile tile = GridGenerator.instance.GetTile(playerGridPos.x + dir.x, playerGridPos.y + dir.y);
            if (tile == null) continue;

            if (tile.GridPosition == GameManager.instance.GetCurrentBallPosition())
                return true;
        }

        return false;
    }

    private bool IsNearGoal(Vector2Int playerGridPos)
    {
        Vector2Int[] directions =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
            new(2, 0), new(-2, 0), new(0, 2), new(0, -2),
            new(2, 2), new(-2, 2), new(2, -2), new(-2, -2)
        };

        Vector2Int goalPos = GameManager.instance.GetPlayerGoalTile().GridPosition;

        foreach (var dir in directions)
        {
            GridTile tile = GridGenerator.instance.GetTile(playerGridPos.x + dir.x, playerGridPos.y + dir.y);
            if (tile == null) continue;

            if (tile.GridPosition == goalPos)
                return true;
        }

        return false;
    }

    #endregion
}
