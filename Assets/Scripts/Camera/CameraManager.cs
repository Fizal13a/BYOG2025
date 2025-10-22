using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera reference")]
    [SerializeField] CinemachineCamera cine_camera;

    [Header("Ball refernce")]
    Transform ball;
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
        ball = GameManager.instance.ball.transform;

        cine_camera.LookAt = ball;
        cine_camera.Follow = ball;
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
        cine_camera.LookAt = null;
        cine_camera.Follow = null;

        cine_camera.transform.localRotation = Quaternion.Slerp(cine_camera.transform.localRotation, Quaternion.identity, Time.deltaTime);
    }

    #endregion
}
