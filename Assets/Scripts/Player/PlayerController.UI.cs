using UnityEngine;
using UnityEngine.UI;

public partial class PlayerController : MonoBehaviour
{
    [Header("UI")]
    public GameObject playerUIPanel;
    #region Buttons
    public Button moveButton;
    public Button passButton;
    public Button tackleButton;
    public Button shootButton;
    public Button dashButton;
    #endregion
  

    #region Initialization

    private void SetUpUI()
    {
        InitializeButtonEvents();
    }

    private void InitializeButtonEvents()
    {
        moveButton.onClick.RemoveAllListeners();
        passButton.onClick.RemoveAllListeners();
        tackleButton.onClick.RemoveAllListeners();
        shootButton.onClick.RemoveAllListeners();
        dashButton.onClick.RemoveAllListeners();
        
        moveButton.onClick.AddListener( (() => {HandleStates(PlayerStates.Move);}));
        passButton.onClick.AddListener( (() => {HandleStates(PlayerStates.Pass);}));
        tackleButton.onClick.AddListener( (() => {HandleStates(PlayerStates.Tackle);}));
        shootButton.onClick.AddListener( (() => {HandleStates(PlayerStates.Shoot);}));
        dashButton.onClick.AddListener( (() => {HandleStates(PlayerStates.Dash);}));
    }
    
    #endregion

    private void ToggleUI(bool state)
    {
        moveButton.interactable = true;
        passButton.interactable = true;
        tackleButton.interactable = true;
        shootButton.interactable = true;
        dashButton.interactable = false;
        
        playerUIPanel.SetActive(state);
        
        Vector2Int playerGridPos = currentSelectedPlayer.GetGridPosition();
        Transform playerTransform = currentSelectedPlayer.transform;

        Vector2Int[] tackledirections = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // right
            new Vector2Int(-1, 0),  // left
            new Vector2Int(0, 1),   // up
            new Vector2Int(0, -1),  // down
            new Vector2Int(1, 1),   // top right
            new Vector2Int(-1, 1),  // top left
            new Vector2Int(1, -1),  // bottom right
            new Vector2Int(-1, -1), // bottom left
        };
        
        Vector2Int[] goaldirections = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // right
            new Vector2Int(-1, 0),  // left
            new Vector2Int(0, 1),   // up
            new Vector2Int(0, -1),  // down
            new Vector2Int(1, 1),   // top right
            new Vector2Int(-1, 1),  // top left
            new Vector2Int(1, -1),  // bottom right
            new Vector2Int(-1, -1), // bottom left
            new Vector2Int(2, 0),   // right
            new Vector2Int(-2, 0),  // left
            new Vector2Int(0, 2),   // up
            new Vector2Int(0, -2),  // down
            new Vector2Int(2, 2),   // top right
            new Vector2Int(-2, 2),  // top left
            new Vector2Int(2, -2),  // bottom right
            new Vector2Int(-2, -2), // bottom left
        };

        foreach (var dir in tackledirections)
        {
            if (currentPlayerWithBall != null)
            {
                tackleButton.interactable = false;
                break;
            }
            
            Vector2Int checkPos = playerGridPos + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos.x, checkPos.y);

            if(tile == null) continue;
            
            Vector2Int tilePos = tile.GridPosition;

            if (tile != null && tilePos == GameManager.instance.GetCurrentBallPosition())
            {
                tackleButton.interactable = false;
            }
        }
        foreach (var dir in goaldirections)
        {
            if (currentPlayerWithBall == null)
            {
                shootButton.interactable = false;
                break;
            }
            
            Vector2Int checkPos = playerGridPos + dir;
            GridTile tile = GridGenerator.instance.GetTile(checkPos.x, checkPos.y);

            if(tile == null) continue;
            
            Vector2Int tilePos = tile.GridPosition;

            if (tile != null && tilePos == GameManager.instance.GetPlayerGoalTile().GridPosition)
            {
                shootButton.interactable = false;
            }
        }
    }
}
