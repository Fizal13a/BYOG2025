using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallController : MonoBehaviour
{
    public static BallController instance;
    
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

    public IEnumerator MoveBall(Transform targetTile)
    {
        if (targetTile == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetTile.position;
    
        // Precalculate movement parameters
        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Clamp(distance * 0.4f, 0.3f, 1.2f);
        float arcHeight = distance * 0.3f + Random.Range(-0.1f, 0.15f);
        float invDuration = 1f / duration; // Cache division
    
        // Cache rotation values
        Vector3 direction = (endPos - startPos).normalized;
        float rotationSpeed = 800f * Time.fixedDeltaTime; // More consistent rotation
    
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * invDuration; // Use cached inverse
        
            // Ease-out cubic for smoother deceleration
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
        
            // Parabolic arc
            float arcAmount = Mathf.Sin(t * Mathf.PI) * arcHeight;
        
            // Update position
            transform.position = Vector3.LerpUnclamped(startPos, endPos, easeT) + Vector3.up * arcAmount;
        
            // Rotate ball
            transform.Rotate(direction, rotationSpeed, Space.World);
        
            yield return null;
        }

        // Snap to final position with slight offset
        transform.position = endPos + Vector3.up * 0.05f;
    }
    
    public IEnumerator BallShoot(Transform targetTile)
    {
        if (targetTile == null) yield break;

        GameObject ball = MatchManager.instance.GetBallObject();

        Vector3 startPos = ball.transform.position;
        Vector3 endPos = targetTile.position;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Clamp(distance * 0.25f, 0.2f, 0.8f); // Faster than passes
        float elapsed = 0f;

        // Higher arc for goal shots (more dramatic)
        float arcHeight = distance * 0.2f + Random.Range(0.02f, 0.1f);

        Vector3 direction = (endPos - startPos).normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out curve (fast start, quick deceleration) for powerful feel
            float easeT = 1f - Mathf.Pow(1f - t, 2.5f);

            // Parabolic arc
            float arcAmount = Mathf.Sin(t * Mathf.PI) * arcHeight;

            Vector3 pos = Vector3.Lerp(startPos, endPos, easeT);
            pos.y += arcAmount;

            ball.transform.position = pos;

            // Faster spin for more aggressive feel
            ball.transform.Rotate(direction * 1200f * Time.deltaTime, Space.World);

            yield return null;
        }

        // Snap to final position
        ball.transform.position = endPos;
    }
}
