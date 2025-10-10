using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController instance;
    
    public Transform targetTile;
    public float moveDuration = 1.5f; 
    public float curveLength = 2f;

    Coroutine ballMoveCoroutine;

    public bool moveBall;
    //private void Start()
   // {
        //BallMoveCoroutine = StartCoroutine(MoveAlongCurve());
    //}

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    private void Update()
    {
        if (moveBall && ballMoveCoroutine == null)
        {
            ballMoveCoroutine = StartCoroutine(MoveAlongCurve());
        }
    }

    IEnumerator MoveAlongCurve()
    {
        if (targetTile != null)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = targetTile.position;
            float elapsed = 0f;

            while (elapsed < moveDuration && (transform.position.x != endPos.x && transform.position.z!= endPos.z ))
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);

                float length = 4 * curveLength * t * (1 - t);

                Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, t);
                transform.position = new Vector3(horizontalPos.x + length, horizontalPos.y + 0.3f, horizontalPos.z);

                yield return null;
            }

            transform.position = new Vector3(endPos.x, endPos.y + 0.3f, endPos.z);

            moveBall = false;
            ballMoveCoroutine = null;
        }
    }
}
