using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera reference")]
    [SerializeField] CinemachineCamera cine_camera;

    [Header("Centerpoint refernce")]
    [SerializeField] Transform centerPoint;
    void Start()
    {
        SetInitialPos();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraTarget();
    }

    #region Initial Cinemachine camera Target

    private void SetInitialPos()
    {

        cine_camera.LookAt = centerPoint;
        cine_camera.Follow = centerPoint;
    }

    #endregion


    #region Updating Cinemachine camera Target
    private void UpdateCameraTarget()
    {
        if (GameManager.instance.currentState == GameSettings.GameState.PlayerTurn && PlayerController.instance.currentSelectedPlayer != null)
        {
            Transform currentSelectedPlayer = PlayerController.instance.currentSelectedPlayer.transform;

            cine_camera.LookAt = currentSelectedPlayer;
            cine_camera.Follow = currentSelectedPlayer;
        }
        else
        {
            ResetCamPos();
        }

    }

    private void ResetCamPos()
    {
        cine_camera.LookAt = centerPoint;
        cine_camera.Follow = centerPoint;
    }

    #endregion
}
