using Assets.Scripts;
using Assets.Scripts.Song;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Assets.Scripts.NoteMissEventArgs;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        OnNoteFloor,
        OnNoteFloorRewinding,
        Falling,
        OnNote,
        OnNoteRewinding,
        TransitionToNote,
        TransitionToMiss,
    }

    private enum PlayerFallingSubState
    {
        Falling,
        FallingToOtherChannel,
    }

    [Header("General")]
    public AudioSource TargetAudioSource;
    public NoteHitDetector NoteHitDetector;
    public SongPlayer SongPlayer;
    public Animator PlayerAnimator;

    [Header("Transition Settings")]
    public float TransitionTimeInSeconds;
    public float HeightOffsetForMidPoint;
    public float PauseBeforeResumeTimeInSeconds;

    [Header("Miss Transition Settings")]
    public float MissStutterTimeInSeconds;

    [Header("FallToOtherChannel Settings")]
    public AnimationCurve OtherChannelAnimationCurve;
    public float OtherChannelTransitionTimeInSeconds;

    [Header("Events")]
    public UnityEvent OnFallStarted;
    public UnityEvent OnFallEnded;
    public UnityEvent<NoteHitEventArgs> OnConfirmedHit;
    public UnityEvent<NoteMissEventArgs> OnConfirmedMiss;

    [HideInInspector]
    public int FallCounter;

    // The targets when transitioning to a note
    private NoteBlock targetNoteBlock;
    private NoteBlock lastSuccessfulNoteBlock;
    private NoteChannelInfo targetChannelInfo;

    // Target when transitioning to a miss
    private Vector3 missEndPoint;
    private NoteChannelInfo missChannelInfo;
    private float audioSourceTimeOnMiss;
    private TypeOfMissedNote typeOfMiss;
    private float missStutterTimeCounter = 0;

    // Target when falling
    private NoteBlock targetFallNote;
    private NoteChannelInfo fallChannelInfo;
    private NoteChannelInfo fallOtherChannelInfo;
    private float otherChannelTransitionCounter = 0;
    private bool isFallingToOtherChannel = false;

    // Rewinding
    private float pauseBeforeResumeCounter = 0;

    // Common info for all transitions
    private Vector3 transitionStartPos;
    private float transitionCounter = 0;
    private PlayerState playerState;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        if (NoteHitDetector == null) { throw new ArgumentNullException(nameof(NoteHitDetector)); }

        Debug.Log("Transitioning to OnSideStage to Start");
        playerState = PlayerState.OnNoteFloor;
        targetChannelInfo = SongPlayer.LeftChannelInfo;
        transform.position = SongPlayer.LeftChannelTransform.position;
        transitionStartPos = transform.position;

        FallCounter = 0;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public void Update()
    {
        switch (playerState)
        {
            case PlayerState.OnNoteFloor:
                {
                    Vector3 newPos = transform.position;
                    newPos.y = SongPlayer.NoteFloor.transform.position.y;
                    transform.position = newPos;
                    PlayerAnimator.SetTrigger("PlayerIdleTrigger");
                    break;
                }
            case PlayerState.OnNoteFloorRewinding:
                {
                    PlayerAnimator.SetTrigger("PlayerLandTrigger");
                    Vector3 posToGoal = fallChannelInfo.GoalPos - transform.position;
                    float distToGoal = Vector3.Distance(transform.position, fallChannelInfo.GoalPos);
                    float rewindDot = Vector3.Dot(posToGoal, fallChannelInfo.Direction);

                    Vector3 newPos = transform.position;
                    newPos.y = SongPlayer.NoteFloor.transform.position.y;
                    transform.position = newPos;

                    if (rewindDot >= 0 || distToGoal < 0.1f)
                    {
                        if (pauseBeforeResumeCounter < PauseBeforeResumeTimeInSeconds)
                        {
                            pauseBeforeResumeCounter += Time.deltaTime;
                            SongPlayer.PauseSong();
                            SetOnNoteAnimationTrigger();
                        }
                        else
                        {
                            SongPlayer.RewindGracePeriodThenUnpause();

                            Debug.Log("Transitioning to OnNoteFloor from OnNoteFloorRewinding");
                            playerState = PlayerState.OnNoteFloor;
                            List<NoteBlock> notesToIgnore = new List<NoteBlock>();
                            NoteHitDetector.EndCollectNotesToReset(notesToIgnore);
                            OnFallEnded?.Invoke();
                            SetOnNoteAnimationTrigger();
                        }
                    }
                    break;
                }
            case PlayerState.OnNote:
                {
                    SetOnNoteAnimationTrigger();

                    if (targetNoteBlock != null)
                    {
                        transform.position = targetNoteBlock.GetLandedPosition();
                    }
                    break;
                }
            case PlayerState.TransitionToNote:
                {
                    if (transitionCounter < TransitionTimeInSeconds)
                    {
                        // Calculate the up-to-date end point and transition to it
                        Vector3 targetNoteEndPos = targetNoteBlock.GetLandedPosition();
                        transitionCounter += Time.deltaTime;
                        Vector3 topMidPos = GetParabolicMidPoint(transitionStartPos, targetNoteEndPos, HeightOffsetForMidPoint);
                        float normalisedTransitionCounter = (transitionCounter / TransitionTimeInSeconds);

                        if (normalisedTransitionCounter <= 0.5f)
                        {
                            PlayerAnimator.SetTrigger("PlayerUpTrigger");
                        }
                        else
                        {
                            PlayerAnimator.SetTrigger("PlayerDownTrigger");
                        }

                        if (targetNoteEndPos.x >= transitionStartPos.x)
                        {
                            Vector3 scale = transform.localScale;
                            scale.x = Math.Abs(transform.localScale.x);
                            transform.localScale = scale;
                        }
                        else
                        {
                            Vector3 scale = transform.localScale;
                            scale.x = Math.Abs(transform.localScale.x) * -1;
                            transform.localScale = scale;
                        }

                        transform.position = ParabolicLerp(transitionStartPos, topMidPos, targetNoteEndPos, normalisedTransitionCounter);
                    }
                    else
                    {
                        Vector3 scale = transform.localScale;
                        scale.x = Math.Abs(transform.localScale.x);
                        transform.localScale = scale;

                        // We have landed!
                        Debug.Log("Transitioning to OnNote from TransitionToNote");
                        playerState = PlayerState.OnNote;
                        lastSuccessfulNoteBlock = targetNoteBlock;
                        SetOnNoteAnimationTrigger();
                    }
                    break;
                }
            case PlayerState.TransitionToMiss:
                {
                    float currentAudioSourceTime = TargetAudioSource.time;
                    float timeDiff = currentAudioSourceTime - audioSourceTimeOnMiss;
                    float distanceForThatTime = SongPlayer.GetNoteDistanceOverTime(timeDiff);

                    Vector3 adjustedMissEndPoint = missEndPoint + (missChannelInfo.Direction * distanceForThatTime);

                    if (transitionCounter < TransitionTimeInSeconds)
                    {
                        // Use the pre-defined miss endpoint and transition to it
                        transitionCounter += Time.deltaTime;
                        Vector3 topMidPos = GetParabolicMidPoint(transitionStartPos, adjustedMissEndPoint, HeightOffsetForMidPoint);
                        float normalisedTransitionCounter = (transitionCounter / TransitionTimeInSeconds);

                        if (normalisedTransitionCounter <= 0.5f)
                        {
                            PlayerAnimator.SetTrigger("PlayerUpTrigger");
                        }
                        else
                        {
                            PlayerAnimator.SetTrigger("PlayerDownTrigger");
                        }

                        if (adjustedMissEndPoint.x >= transitionStartPos.x)
                        {
                            Vector3 scale = transform.localScale;
                            scale.x = Math.Abs(transform.localScale.x);
                            transform.localScale = scale;
                        }
                        else
                        {
                            Vector3 scale = transform.localScale;
                            scale.x = Math.Abs(transform.localScale.x) * -1;
                            transform.localScale = scale;
                        }

                        transform.position = ParabolicLerp(transitionStartPos, topMidPos, adjustedMissEndPoint, normalisedTransitionCounter);
                    }
                    else
                    {
                        if (missStutterTimeCounter < MissStutterTimeInSeconds)
                        {
                            missStutterTimeCounter += Time.deltaTime;
                            PlayerAnimator.SetTrigger("PlayerFailTrigger");
                        }
                        else
                        {
                            // We are falling!
                            Debug.Log("Transitioning to Falling from TransitionToMiss");
                            playerState = PlayerState.Falling;
                            FallCounter++;

                            Vector3 scale = transform.localScale;
                            scale.x = Math.Abs(transform.localScale.x);
                            transform.localScale = scale;

                            List<GameObject> notesToIgnore = new List<GameObject>();
                            if (lastSuccessfulNoteBlock != null)
                            {
                                notesToIgnore.Add(lastSuccessfulNoteBlock.gameObject);
                            }
                            targetFallNote = NoteHitDetector.GetLastCompletedNote(missChannelInfo, (adjustedMissEndPoint - missChannelInfo.GoalPos), notesToIgnore, 10f);
                            fallChannelInfo = missChannelInfo;

                            NoteHitDetector.StartCollectNotesToReset();
                            OnFallStarted?.Invoke();
                        }
                    }

                    break;
                }
            case PlayerState.Falling:
                {
                    PlayerAnimator.SetTrigger("PlayerFallTrigger");

                    string fallDebugLog = "Falling Enter!";
                    // If we are also falling to another channel, handle our horizontal movement
                    if (isFallingToOtherChannel)
                    {
                        fallDebugLog += "\n\tFalling to Other Channel!";
                        if (otherChannelTransitionCounter < OtherChannelTransitionTimeInSeconds)
                        {
                            otherChannelTransitionCounter += Time.deltaTime;
                            fallDebugLog += $"\n\t\tCurrently Transitioning to Other Channel - CurCounter '{otherChannelTransitionCounter}' | Max: '{OtherChannelTransitionTimeInSeconds}'!";

                            Vector3 startPos = new Vector3(fallChannelInfo.GoalPos.x, transform.position.y, transform.position.z);
                            Vector3 endPos = new Vector3(fallOtherChannelInfo.GoalPos.x, transform.position.y, transform.position.z);
                            float t = OtherChannelAnimationCurve.Evaluate(otherChannelTransitionCounter / OtherChannelTransitionTimeInSeconds);

                            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
                            transform.position = newPos;
                            fallDebugLog += $"\n\t\tNew Position '{transform.position.ToString()}' Lerped between '{startPos.ToString()}' and '{endPos.ToString()}' with t {t}";
                        }
                        else
                        {
                            fallDebugLog += $"\n\t\tFinished Transitioning, Snapping to Endpoint";
                            otherChannelTransitionCounter = 0;
                            isFallingToOtherChannel = false;

                            Vector3 newPos = transform.position;
                            newPos.x = fallOtherChannelInfo.GoalPos.x;
                            transform.position = newPos;

                            fallChannelInfo = fallOtherChannelInfo;
                            fallOtherChannelInfo = null;
                        }
                    }

                    // If there's no target note for us to fall to, we're falling to the floor
                    if (targetFallNote == null)
                    {
                        fallDebugLog += "\n\tFalling to Floor!";
                        float distToFloor = Vector3.Distance(transform.position, SongPlayer.NoteFloor.transform.position);
                        Vector3 posToFloor = SongPlayer.NoteFloor.transform.position - transform.position;
                        float floorDot = Vector3.Dot(posToFloor, fallChannelInfo.Direction);

                        if (floorDot <= 0 || distToFloor < 0.1f)
                        {
                            Debug.Log("Transitioning to OnNoteFloorRewinding from Falling");
                            playerState = PlayerState.OnNoteFloorRewinding;

                            Vector3 newPos = transform.position;
                            newPos.x = fallChannelInfo.GoalPos.x;
                            newPos.y = SongPlayer.NoteFloor.transform.position.y;
                            transform.position = newPos;
                            targetChannelInfo = fallChannelInfo;
                            pauseBeforeResumeCounter = 0;
                        }
                    }
                    else
                    {
                        fallDebugLog += "\n\tFalling to Target Note!";
                        // Otherwise we're moving to a specific note. Stay in this state until we're close enough
                        // or past it
                        float distToTarget = Vector3.Distance(transform.position, targetFallNote.GetLandedPosition());
                        Vector3 posToTarget = targetFallNote.GetLandedPosition() - transform.position;
                        float fallDot = Vector3.Dot(posToTarget, fallChannelInfo.Direction);

                        if (fallDot <= 0 || distToTarget < 0.1f)
                        {
                            Debug.Log("Transitioning to OnNote from Falling");

                            switch (targetFallNote.NoteType)
                            {
                                // For a normal block, we just snap to it and continue
                                case NoteType.Normal:
                                    {
                                        transform.position = targetFallNote.GetLandedPosition();

                                        // Update the state machine and the note info
                                        playerState = PlayerState.OnNoteRewinding;
                                        targetNoteBlock = targetFallNote;
                                        targetChannelInfo = fallChannelInfo;
                                        pauseBeforeResumeCounter = 0;

                                        SetOnNoteAnimationTrigger();
                                        break;
                                    }
                                case NoteType.FallRight:
                                    {
                                        // Move to the channel to the right
                                        NoteChannelInfo rightChannel = GetRightChannel(fallChannelInfo);

                                        //Vector3 newPos = transform.position;
                                        //newPos.x = rightChannel.GoalPos.x;
                                        //transform.position = newPos;

                                        List<GameObject> notesToIgnore = new List<GameObject>();
                                        targetFallNote = NoteHitDetector.GetLastCompletedNote(rightChannel, (transform.position - rightChannel.GoalPos), notesToIgnore, 0.1f);
                                        fallOtherChannelInfo = rightChannel;

                                        isFallingToOtherChannel = true;
                                        otherChannelTransitionCounter = 0;
                                        break;
                                    }
                                case NoteType.FallLeft:
                                    {
                                        // Move to the channel to the left
                                        NoteChannelInfo leftChannel = GetLeftChannel(fallChannelInfo);

                                        //Vector3 newPos = transform.position;
                                        //newPos.x = leftChannel.GoalPos.x;
                                        //transform.position = newPos;

                                        List<GameObject> notesToIgnore = new List<GameObject>();
                                        targetFallNote = NoteHitDetector.GetLastCompletedNote(leftChannel, (transform.position - leftChannel.GoalPos), notesToIgnore, 0.1f);
                                        fallOtherChannelInfo = leftChannel;

                                        isFallingToOtherChannel = true;
                                        otherChannelTransitionCounter = 0;
                                        break;
                                    }
                                case NoteType.Fragile:
                                    {
                                        // Fall straight through the block
                                        List<GameObject> notesToIgnore = new List<GameObject>()
                                        {
                                            targetFallNote.gameObject
                                        };
                                        targetFallNote = NoteHitDetector.GetLastCompletedNote(fallChannelInfo, (transform.position - fallChannelInfo.GoalPos), notesToIgnore, 0.1f);
                                        break;
                                    }
                                case NoteType.Unknown:
                                default:
                                    {
                                        throw new NotImplementedException($"Unable to identify fall landing behaviour for note type of '{targetFallNote.NoteType}'");
                                    }
                            }
                        }
                    }

                    Debug.Log(fallDebugLog);
                    break;
                }
            case PlayerState.OnNoteRewinding:
                {
                    PlayerAnimator.SetTrigger("PlayerLandTrigger");

                    Vector3 posToGoal = fallChannelInfo.GoalPos - transform.position;
                    float distToGoal = Vector3.Distance(transform.position, fallChannelInfo.GoalPos);
                    float rewindDot = Vector3.Dot(posToGoal, fallChannelInfo.Direction);

                    transform.position = targetNoteBlock.GetLandedPosition();

                    if (rewindDot >= 0 || distToGoal < 0.1f)
                    {
                        if (PauseBeforeResumeTimeInSeconds > float.Epsilon && 
                            pauseBeforeResumeCounter < PauseBeforeResumeTimeInSeconds)
                        {
                            pauseBeforeResumeCounter += Time.deltaTime;
                            SongPlayer.PauseSong();
                            SetOnNoteAnimationTrigger();
                        }
                        else
                        {
                            SongPlayer.RewindGracePeriodThenUnpause(new List<GameObject>() { targetNoteBlock.gameObject });

                            Debug.Log("Transitioning to OnNote from OnNoteRewinding");
                            playerState = PlayerState.OnNote;
                            lastSuccessfulNoteBlock = targetNoteBlock;
                            List<NoteBlock> notesToIgnore = new List<NoteBlock>();
                            if (targetNoteBlock != null)
                            {
                                notesToIgnore.Add(targetNoteBlock);
                            }
                            NoteHitDetector.EndCollectNotesToReset(notesToIgnore);
                            OnFallEnded?.Invoke();

                            SetOnNoteAnimationTrigger();
                        }
                    }

                    break;
                }
            default:
                throw new NotImplementedException($"Unsupported Player State of {playerState}");
        }
    }

    /// <summary>
    /// Gets the channel to the left of the one provided
    /// </summary>
    private NoteChannelInfo GetLeftChannel(NoteChannelInfo currentChannel)
    {
        switch (currentChannel.NoteChannel)
        {
            case NoteChannel.Left:
                {
                    // Cant go any more left
                    return currentChannel;
                }
            case NoteChannel.Down:
                {
                    return SongPlayer.LeftChannelInfo;
                }
            case NoteChannel.Up:
                {
                    return SongPlayer.DownChannelInfo;
                }
            case NoteChannel.Right:
                {
                    return SongPlayer.UpChannelInfo;
                }
            default:
                throw new NotImplementedException($"Unable to identify Note Channel of {currentChannel.NoteChannel}");
        }
    }

    private void SetOnNoteAnimationTrigger()
    {
        if (targetNoteBlock != null)
        {
            if (targetNoteBlock.NoteType == NoteType.FallLeft)
            {
                PlayerAnimator.SetTrigger("PlayerIdleLeftTrigger");
            }
            else if (targetNoteBlock.NoteType == NoteType.FallRight)
            {
                PlayerAnimator.SetTrigger("PlayerIdleRightTrigger");
            }
            else if (targetNoteBlock.NoteType == NoteType.Fragile)
            {
                PlayerAnimator.SetTrigger("PlayerIdleFragileTrigger");
            }
            else
            {
                PlayerAnimator.SetTrigger("PlayerIdleTrigger");
            }
        }
        else
        {
            PlayerAnimator.SetTrigger("PlayerIdleTrigger");
        }
    }

    /// <summary>
    /// Gets the channel to the right of the one provided
    /// </summary>
    private NoteChannelInfo GetRightChannel(NoteChannelInfo currentChannel)
    {
        switch (currentChannel.NoteChannel)
        {
            case NoteChannel.Left:
                {
                    // Cant go any more left
                    return SongPlayer.DownChannelInfo;
                }
            case NoteChannel.Down:
                {
                    return SongPlayer.UpChannelInfo;
                }
            case NoteChannel.Up:
                {
                    return SongPlayer.RightChannelInfo;
                }
            case NoteChannel.Right:
                {
                    return currentChannel;
                }
            default:
                throw new NotImplementedException($"Unable to identify Note Channel of {currentChannel.NoteChannel}");
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

        if (playerState == PlayerState.Falling || playerState == PlayerState.TransitionToMiss)
        {
            // We're currently falling, can't hit anything!
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
        lastSuccessfulNoteBlock = targetNoteBlock;

        OnConfirmedHit?.Invoke(args);
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
        missStutterTimeCounter = 0;

        // Identify the end point
        typeOfMiss = args.MissType;
        switch (args.MissType)
        {
            case NoteMissEventArgs.TypeOfMissedNote.IncorrectlyAttemptedNote:
                missEndPoint = args.AttemptedChannel.GoalPos;
                missChannelInfo = args.AttemptedChannel;
                break;
            case NoteMissEventArgs.TypeOfMissedNote.NoAttemptedNote:
                missEndPoint = new Vector3(targetChannelInfo.GoalPos.x, transform.position.y, transform.position.z);
                missChannelInfo = targetChannelInfo;
                break;
            default:
                throw new NotImplementedException($"Unknown Miss Type of {args.MissType}");
        }

        Debug.Log($"Transitioning to TransitionToMiss from {playerState} due to OnNoteMissEvent");
        playerState = PlayerState.TransitionToMiss;
        audioSourceTimeOnMiss = TargetAudioSource.time;

        OnConfirmedMiss?.Invoke(args);
    }
}
