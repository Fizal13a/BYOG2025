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
        playerUIPanel.SetActive(state);
    }
}
