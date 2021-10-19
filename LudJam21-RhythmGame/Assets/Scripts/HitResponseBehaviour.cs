using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitResponseBehaviour : MonoBehaviour
{
    public AnimationCurve transitionCurve;
    private Vector3 endPos;
    private Vector3 startPos;
    private float timeCounter = 0;
    private const float transitionTimeInSeconds = 0.5f;

    public void Start()
    {
        startPos = transform.position;
        endPos = transform.position + new Vector3(0, 0.75f);
    }

    public void Update()
    {
        float newY = Mathf.Lerp(startPos.y, endPos.y, transitionCurve.Evaluate((timeCounter / transitionTimeInSeconds)));
        timeCounter += Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;

        if (timeCounter > transitionTimeInSeconds)
        {
            Destroy(this.gameObject);
        }
    }
}
