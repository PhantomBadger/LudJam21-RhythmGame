using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    public AnimationCurve StartTiltCurve;
    public AnimationCurve EndTiltCurve;
    public float TiltOffset = 0.5f;
    public float TiltTimeInSeconds = 3f;
    private Vector3 startPos;
    private Vector3 tiltPos;
    private float upTiltTimeCounter = 0;
    private float downTiltTimeCounter = float.MaxValue;
    private bool tiltEnabled = false;

    public void Start()
    {
        startPos = transform.position;
        tiltPos = startPos - new Vector3(0, TiltOffset);
    }

    public void Update()
    {
        if (tiltEnabled)
        {
            if (upTiltTimeCounter < TiltTimeInSeconds)
            {
                upTiltTimeCounter += Time.deltaTime;
                float t = StartTiltCurve.Evaluate(upTiltTimeCounter / TiltTimeInSeconds);
                transform.position = Vector3.Lerp(startPos, tiltPos, t);
            }
            else
            {
                transform.position = tiltPos;
            }
        }
        else
        {
            if (downTiltTimeCounter < TiltTimeInSeconds)
            {
                downTiltTimeCounter += Time.deltaTime;
                float t = EndTiltCurve.Evaluate(downTiltTimeCounter / TiltTimeInSeconds);
                transform.position = Vector3.Lerp(tiltPos, startPos, t);
            }
            else
            {
                transform.position = startPos;
            }
        }
    }

    public void EnableCameraDownTilt(bool enabled)
    {
        if (enabled)
        {
            upTiltTimeCounter = 0;
            tiltEnabled = true;
        }
        else
        {
            downTiltTimeCounter = 0;
            tiltEnabled = false;
        }
    }
}
