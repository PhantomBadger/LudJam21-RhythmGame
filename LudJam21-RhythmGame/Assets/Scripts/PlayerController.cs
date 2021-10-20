using Assets.Scripts;
using Assets.Scripts.Song;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        OnSideStage,
        Falling,
        OnNote,
        TransitionToNote,
        TransitionToMiss,
    }

    public Transform NotOnNotePosition;
    public NoteHitDetector NoteHitDetector;
    public float TransitionTimeInSeconds;
    public float HeightOffsetForMidPoint;

    public UnityEvent OnFallStarted;
    public UnityEvent OnFallEnded;

    // The targets when transitioning to a note
    private NoteBlockBase targetNoteBlock;
    private NoteChannelInfo targetChannelInfo;

    // Target when transitioning to a miss
    private Vector3 missEndPoint;
    private NoteChannelInfo missChannelInfo;

    // Target when falling
    private NoteBlockBase targetFallNote;
    private NoteChannelInfo fallChannelInfo;

    // Common info for all transitions
    private Vector3 transitionStartPos;
    private float transitionCounter = 0;
    private PlayerState playerState;


    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        if (NotOnNotePosition == null) { throw new ArgumentNullException(nameof(NotOnNotePosition)); }
        if (NoteHitDetector == null) { throw new ArgumentNullException(nameof(NoteHitDetector)); }

        transform.position = NotOnNotePosition.position;
        transitionStartPos = transform.position;
        Debug.Log("Transitioning to OnSideStage to Start");
        playerState = PlayerState.OnSideStage;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public void Update()
    {
        switch (playerState)
        {
            case PlayerState.OnSideStage:
                transform.position = NotOnNotePosition.position;
                break;
            case PlayerState.OnNote:
                if (targetNoteBlock != null)
                {
                    transform.position = targetNoteBlock.GetLandedPosition();
                }
                break;
            case PlayerState.TransitionToNote:
                if (transitionCounter < TransitionTimeInSeconds)
                {
                    // Calculate the up-to-date end point and transition to it
                    Vector3 targetNoteEndPos = targetNoteBlock.GetLandedPosition();
                    transitionCounter += Time.deltaTime;
                    Vector3 topMidPos = GetParabolicMidPoint(transitionStartPos, targetNoteEndPos, HeightOffsetForMidPoint);
                    float normalisedTransitionCounter = (transitionCounter / TransitionTimeInSeconds);
                    transform.position = ParabolicLerp(transitionStartPos, topMidPos, targetNoteEndPos, normalisedTransitionCounter);
                }
                else
                {
                    // We have landed!
                    Debug.Log("Transitioning to OnNote from TransitionToNote");
                    playerState = PlayerState.OnNote;
                    targetNoteBlock.OnLanded();
                }
                break;
            case PlayerState.TransitionToMiss:
                if (transitionCounter < TransitionTimeInSeconds)
                {
                    // Use the pre-defined miss endpoint and transition to it
                    transitionCounter += Time.deltaTime;
                    Vector3 topMidPos = GetParabolicMidPoint(transitionStartPos, missEndPoint, HeightOffsetForMidPoint);
                    float normalisedTransitionCounter = (transitionCounter / TransitionTimeInSeconds);
                    transform.position = ParabolicLerp(transitionStartPos, topMidPos, missEndPoint, normalisedTransitionCounter);
                }
                else
                {
                    // We are falling!
                    Debug.Log("Transitioning to Falling from TransitionToMiss");
                    playerState = PlayerState.Falling;

                    List<GameObject> notesToIgnore = new List<GameObject>();
                    if (targetNoteBlock != null)
                    {
                        notesToIgnore.Add(targetNoteBlock.gameObject);
                    }
                    targetFallNote = NoteHitDetector.GetLastCompletedNote(missChannelInfo, notesToIgnore, 0.1f);
                    fallChannelInfo = missChannelInfo;

                    NoteHitDetector.StartCollectNotesToReset();
                    OnFallStarted?.Invoke();
                }
                break;
            case PlayerState.Falling:
                if (targetFallNote == null)
                {
                    transform.position = NotOnNotePosition.position;
                    Debug.Log("Transitioning to OnSideStage from Falling");
                    playerState = PlayerState.OnSideStage;

                    NoteHitDetector.EndCollectNotesToReset(new List<NoteBlockBase>());
                    OnFallEnded?.Invoke();
                    return;
                }

                float distToTarget = Vector3.Distance(transform.position, targetFallNote.GetLandedPosition());
                Vector3 posToTarget = targetFallNote.GetLandedPosition() - transform.position;
                float dot = Vector3.Dot(posToTarget, fallChannelInfo.Direction);

                if (dot <= 0 || distToTarget < 0.1f)
                {
                    transform.position = targetFallNote.GetLandedPosition();
                    Debug.Log("Transitioning to OnNote from Falling");
                    playerState = PlayerState.OnNote;
                    targetNoteBlock = targetFallNote;

                    NoteHitDetector.EndCollectNotesToReset(new List<NoteBlockBase>() { targetNoteBlock });
                    OnFallEnded?.Invoke();
                }
                break;
            default:
                throw new NotImplementedException($"Unsupported Player State of {playerState}");
        }
    }

    /// <summary>
    /// Gets a mid point for a parabolic curve given a start, end, and a height offset
    /// </summary>
    private Vector3 GetParabolicMidPoint(Vector3 startPos, Vector3 endPos, float heightOffset)
    {
        return startPos + ((endPos - startPos) / 2f) + Vector3.up * heightOffset;
    }

    /// <summary>
    /// Lerps along a parabolic curve formed by the start, mid, and end points
    /// </summary>
    private Vector3 ParabolicLerp(Vector3 startPos, Vector3 midPos, Vector3 endPos, float normalizedTime)
    {
        Vector3 m1 = Vector3.Lerp(startPos, midPos, normalizedTime);
        Vector3 m2 = Vector3.Lerp(midPos, endPos, normalizedTime);
        return Vector3.Lerp(m1, m2, normalizedTime);
    }

    /// <summary>
    /// Called when a Note is correctly hit
    /// </summary>
    public void OnNoteHitEvent(NoteHitEventArgs args)
    {
        if (args == null || args.HitNote == null)
        {
            return;
        }

        // Initialise any transition
        transitionCounter = 0;
        transitionStartPos = transform.position;

        // Set the target note
        targetNoteBlock = args.HitNote;
        targetChannelInfo = args.AttemptedChannel;

        Debug.Log($"Transitioning to TransitionToNote from {playerState} due to OnNoteHitEvent");
        playerState = PlayerState.TransitionToNote;
    }

    /// <summary>
    /// Called when a Note is incorrectly hit
    /// </summary>
    public void OnNoteMissEvent(NoteMissEventArgs args)
    {
        if (args == null || args.AttemptedChannel == null || targetChannelInfo == null)
        {
            // TODO - Account for failures at the start
            return;
        }

        if (playerState == PlayerState.Falling || playerState == PlayerState.TransitionToMiss)
        {
            // We're currently falling, misses don't count!
            return;
        }

        // Initialise any transition
        transitionCounter = 0;
        targetNoteBlock = null;
        transitionStartPos = transform.position;

        // Identify the end point
        switch (args.MissType)
        {
            case NoteMissEventArgs.TypeOfMissedNote.IncorrectlyAttemptedNote:
                missEndPoint = args.AttemptedChannel.GoalPos;
                missChannelInfo = args.AttemptedChannel;
                break;
            case NoteMissEventArgs.TypeOfMissedNote.NoAttemptedNote:
                missEndPoint = targetChannelInfo.GoalPos;
                missChannelInfo = targetChannelInfo;
                break;
            default:
                throw new NotImplementedException($"Unknown Miss Type of {args.MissType}");
        }

        Debug.Log($"Transitioning to TransitionToMiss from {playerState} due to OnNoteMissEvent");
        playerState = PlayerState.TransitionToMiss;
    }
}
