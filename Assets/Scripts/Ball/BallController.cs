using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController instance;

    [Header("Target")]
    public Transform targetTile;

    [Header("Curve Settings")]
    public float moveDuration = 1.5f;
    public float curveSideAmount = 1.5f;
    public float curveHeight = 1.2f;

    private Coroutine ballMoveCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void CurveMoveBall(Transform tile)
    {
        targetTile = tile;

        if (ballMoveCoroutine != null)
            StopCoroutine(ballMoveCoroutine);

        ballMoveCoroutine = StartCoroutine(MoveAlongCurve());
    }

    IEnumerator MoveAlongCurve()
    {
        if (targetTile == null)
            yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetTile.position;

        float elapsed = 0f;

        Vector3 forwardDir = (endPos - startPos).normalized;
        Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir); 

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            float smoothT = Mathf.Pow(t, 5f);

            Vector3 pos = Vector3.Lerp(startPos, endPos, smoothT);

            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * curveHeight;

            float sideOffset = Mathf.Sin(smoothT * Mathf.PI) * curveSideAmount;

            pos += sideDir * sideOffset;
            pos.y += heightOffset + 0.3f;

            transform.position = pos;

            yield return null;
        }

        transform.position = new Vector3(endPos.x, endPos.y + 0.3f, endPos.z);
        ballMoveCoroutine = null;
    }
}
