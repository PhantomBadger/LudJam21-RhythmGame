using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitResponseBehaviour : MonoBehaviour
{
    public AnimationCurve TransitionCurve;
    public float ResponseFloatDistance;
    public float TransitionTimeInSeconds;
    private Vector3 endPos;
    private Vector3 startPos;
    private float timeCounter = 0;

    public void Start()
    {
        startPos = transform.position;
        endPos = transform.position + new Vector3(0, ResponseFloatDistance);
    }

    public void Update()
    {
        float newY = Mathf.Lerp(startPos.y, endPos.y, TransitionCurve.Evaluate((timeCounter / TransitionTimeInSeconds)));
        timeCounter += Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;

        if (timeCounter > TransitionTimeInSeconds)
        {
            Destroy(this.gameObject);
        }
    }
}
