using Assets.Scripts;
using Assets.Scripts.Song;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static Assets.Scripts.NoteMissEventArgs;

namespace Assets.Scripts
{
    public class NoteHitDetector : MonoBehaviour
    {
        private class CandidateNoteHitTest
        {
            public GameObject NoteObject { get; set; }
            public NoteChannelInfo NoteChannelInfo { get; set; }
        }

        public SongPlayer SongPlayer;

        [Range(1f, 50f)]
        public float AboveHitThreshold = 1f;
        [Range(1f, 50f)]
        public float BelowHitThreshold = 1f;
        public float MissCooldownInSeconds = 0.25f;

        public KeyCode LeftChannelKey;
        public KeyCode DownChannelKey;
        public KeyCode UpChannelKey;
        public KeyCode RightChannelKey;

        public UnityEvent<NoteHitEventArgs> OnNoteHit;
        public UnityEvent<NoteMissEventArgs> OnNoteMiss;

        private bool isDetectionActive;
        private float missCooldownCounter;
        private bool isMissCooldownActive;

        private List<NoteRow> initialNotesToReset;

        /// <summary>
        /// Called on Start
        /// </summary>
        public void Start()
        {
            if (SongPlayer == null) { throw new ArgumentNullException(nameof(SongPlayer)); }

            isDetectionActive = false;
            missCooldownCounter = 0;
            isMissCooldownActive = false;
            initialNotesToReset = new List<NoteRow>();
        }

        /// <summary>
        /// Called each Frame
        /// </summary>
        public void Update()
        {
            // If the detection is active
            if (isDetectionActive)
            {
                bool leftHitInputToBeProcessed = Input.GetKeyDown(LeftChannelKey);
                bool downHitInputToBeProcessed = Input.GetKeyDown(DownChannelKey);
                bool upHitInputToBeProcessed = Input.GetKeyDown(UpChannelKey);
                bool rightHitInputToBeProcessed = Input.GetKeyDown(RightChannelKey);

                // Process each of the possible note rows
                for (int i = 0; i < SongPlayer.NoteRows.Count; i++)
                {
                    NoteRow noteRow = SongPlayer.NoteRows[i];

                    CandidateNoteHitTest testNoteHitTest = GetNoteTestInfo(noteRow);
                    if (testNoteHitTest == null || testNoteHitTest.NoteObject == null)
                    {
                        // No valid notes
                        continue;
                    }

                    Vector3 distToGoal = testNoteHitTest.NoteChannelInfo.GoalPos - testNoteHitTest.NoteObject.transform.position;
                    float dot = Vector3.Dot(distToGoal, testNoteHitTest.NoteChannelInfo.Direction);
                    float dist = Vector3.Distance(testNoteHitTest.NoteChannelInfo.GoalPos, testNoteHitTest.NoteObject.transform.position);

                    if (dot < 0)
                    {
                        // The Note is moving away from the goal
                        if (dist > AboveHitThreshold)
                        {
                            // If it is still hittable
                            // We have missed the note 100%!
                            // Set the hit processed flags so we don't attempt to check it again
                            if (noteRow.LeftNoteObject != null && !noteRow.LeftNoteHasBeenHitProcessed)
                            {
                                noteRow.LeftNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(TypeOfMissedNote.NoAttemptedNote, SongPlayer.LeftChannelInfo);
                            }
                            if (noteRow.DownNoteObject != null && !noteRow.DownNoteHasBeenHitProcessed)
                            {
                                noteRow.DownNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(TypeOfMissedNote.NoAttemptedNote, SongPlayer.DownChannelInfo);
                            }
                            if (noteRow.UpNoteObject != null && !noteRow.UpNoteHasBeenHitProcessed)
                            {
                                noteRow.UpNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(TypeOfMissedNote.NoAttemptedNote, SongPlayer.UpChannelInfo);
                            }
                            if (noteRow.RightNoteObject != null && !noteRow.RightNoteHasBeenHitProcessed)
                            {
                                noteRow.RightNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(TypeOfMissedNote.NoAttemptedNote, SongPlayer.RightChannelInfo);
                            }
                        }
                    }
                    
                    if (dist < BelowHitThreshold)
                    {
                        // We are able to test if we hit it or not
                        if (noteRow.LeftNoteObject != null && leftHitInputToBeProcessed)
                        {
                            noteRow.LeftNoteHasBeenHitProcessed = true;
                            leftHitInputToBeProcessed = false;
                            InvokeNoteHitEvent(noteRow.LeftNoteObject, SongPlayer.LeftChannelInfo);
                        }

                        if (noteRow.DownNoteObject != null && downHitInputToBeProcessed)
                        {
                            noteRow.DownNoteHasBeenHitProcessed = true;
                            downHitInputToBeProcessed = false;
                            InvokeNoteHitEvent(noteRow.DownNoteObject, SongPlayer.DownChannelInfo);
                        }

                        if (noteRow.UpNoteObject != null && upHitInputToBeProcessed)
                        {
                            noteRow.UpNoteHasBeenHitProcessed = true;
                            upHitInputToBeProcessed = false;
                            InvokeNoteHitEvent(noteRow.UpNoteObject, SongPlayer.UpChannelInfo);
                        }

                        if (noteRow.RightNoteObject != null && rightHitInputToBeProcessed)
                        {
                            noteRow.RightNoteHasBeenHitProcessed = true;
                            rightHitInputToBeProcessed = false;
                            InvokeNoteHitEvent(noteRow.RightNoteObject, SongPlayer.RightChannelInfo);
                        }
                    }
                }

                // Check the remaining flags
                if (leftHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(TypeOfMissedNote.IncorrectlyAttemptedNote, SongPlayer.LeftChannelInfo);
                }
                else if (downHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(TypeOfMissedNote.IncorrectlyAttemptedNote, SongPlayer.DownChannelInfo);
                }
                else if (upHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(TypeOfMissedNote.IncorrectlyAttemptedNote, SongPlayer.UpChannelInfo);
                }
                else if (rightHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(TypeOfMissedNote.IncorrectlyAttemptedNote, SongPlayer.RightChannelInfo);
                }
            }

            // Process the miss cooldown
            if (isMissCooldownActive)
            {
                if ((missCooldownCounter += Time.deltaTime) > MissCooldownInSeconds)
                {
                    isMissCooldownActive = false;
                    missCooldownCounter = 0;
                }
            }
        }

        /// <summary>
        /// Gets info about the note to perform the distance check on from the given row
        /// </summary>
        private CandidateNoteHitTest GetNoteTestInfo(NoteRow noteRow)
        {
            if (noteRow == null)
            {
                return null;
            }

            if (noteRow.LeftNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.LeftNoteObject,
                    NoteChannelInfo = SongPlayer.LeftChannelInfo,
                };
            }

            if (noteRow.DownNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.DownNoteObject,
                    NoteChannelInfo = SongPlayer.DownChannelInfo,
                };
            }

            if (noteRow.UpNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.UpNoteObject,
                    NoteChannelInfo = SongPlayer.UpChannelInfo,
                };
            }

            if (noteRow.RightNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.RightNoteObject,
                    NoteChannelInfo = SongPlayer.RightChannelInfo,
                };
            }

            return null;
        }

        /// <summary>
        /// Invokes the Miss Event
        /// </summary>
        private void InvokeNoteMissEvent(TypeOfMissedNote typeOfMissedNote, NoteChannelInfo attemptedChannel)
        {
            // Debug.Break();
            NoteMissEventArgs args = new NoteMissEventArgs()
            {
                MissType = typeOfMissedNote,
                AttemptedChannel = attemptedChannel,
            };

            if (isMissCooldownActive)
            {
                // If we're already cooling down from a miss then don't invoke more
                return;
            }

            isMissCooldownActive = true;

            OnNoteMiss.Invoke(args);
        }

        /// <summary>
        /// Invoke the Hit Event
        /// </summary>
        private void InvokeNoteHitEvent(GameObject noteObject, NoteChannelInfo attemptedChannel)
        {
            NoteBlock blockBase = noteObject.GetComponent<NoteBlock>();
            if (blockBase != null)
            {
                NoteHitEventArgs args = new NoteHitEventArgs()
                {
                    HitNote = blockBase,
                    AttemptedChannel = attemptedChannel,
                };

                OnNoteHit.Invoke(args);
            }
            else
            {
                Debug.LogError($"Unable to Trigger OnNoteHit event, hit Note Block Game Object doesn't contain the NoteBlockBase component!");
            }
        }

        /// <summary>
        /// Enable or Disable the Hit Detection
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableHitDetection(bool enabled)
        {
            isDetectionActive = enabled;
        }

        /// <summary>
        /// Gets the last note to pass the 'goal' in the specified channel
        /// </summary>
        public NoteBlock GetLastCompletedNote(NoteChannelInfo channelToCheck, Vector3 goalPosOffset, List<GameObject> notesToIgnore, float minNoteDist)
        {
            NoteBlock closestNote = null;
            float curSmallestDist = float.MaxValue;

            for (int i = 0; i < SongPlayer.NoteRows.Count; i++)
            {
                NoteRow noteRow = SongPlayer.NoteRows[i];

                // Identify the note object to check
                GameObject noteToCheck = null;
                switch (channelToCheck.NoteChannel)
                {
                    case NoteChannel.Left:
                        noteToCheck = noteRow.LeftNoteObject;
                        break;
                    case NoteChannel.Down:
                        noteToCheck = noteRow.DownNoteObject;
                        break;
                    case NoteChannel.Up:
                        noteToCheck = noteRow.UpNoteObject;
                        break;
                    case NoteChannel.Right:
                        noteToCheck = noteRow.RightNoteObject;
                        break;
                }

                // If the note isnt here then that row has nothing in that spot ignore it
                if (noteToCheck == null)
                {
                    continue;
                }

                // Skip it if it's a note we want to ignore
                if (notesToIgnore != null && notesToIgnore.Contains(noteToCheck))
                {
                    continue;
                }

                Vector3 distToGoal = (channelToCheck.GoalPos + goalPosOffset) - noteToCheck.transform.position;
                float dot = Vector3.Dot(distToGoal, channelToCheck.Direction);
                float dist = Vector3.Distance((channelToCheck.GoalPos + goalPosOffset), noteToCheck.transform.position);

                // If the dot product is negative then the Note is past the goal, ie, we care about it!
                if (dot < 0)
                {
                    if (dist < curSmallestDist)
                    {
                        if (dist < minNoteDist)
                        {
                            continue;
                        }
                        curSmallestDist = dist;
                        closestNote = noteToCheck.GetComponent<NoteBlock>();
                    }
                }
            }

            return closestNote;
        }

        public void StartCollectNotesToReset()
        {
            initialNotesToReset = new List<NoteRow>();

            // Get all Note Rows below the goal pos
            for (int i = 0; i < SongPlayer.NoteRows.Count; i++)
            {
                NoteRow noteRow = SongPlayer.NoteRows[i];
                CandidateNoteHitTest noteHit = GetNoteTestInfo(noteRow);
                if (noteHit == null || noteHit.NoteChannelInfo == null || noteHit.NoteObject == null)
                {
                    continue;
                }

                Vector3 distToGoal = noteHit.NoteChannelInfo.GoalPos - noteHit.NoteObject.transform.position;
                float dot = Vector3.Dot(distToGoal, noteHit.NoteChannelInfo.Direction);

                if (dot < 0)
                {
                    initialNotesToReset.Add(noteRow);
                }
            }
        }

        public void EndCollectNotesToReset(List<NoteBlock> notesToIgnore)
        {
            List<NoteRow> finalNotesBelowGoal = new List<NoteRow>();
            List<GameObject> noteObjectsToIgnore = new List<GameObject>(notesToIgnore.Select((NoteBlock nbb) => nbb.gameObject));

            // Get all Note Rows below the goal pos
            for (int i = 0; i < SongPlayer.NoteRows.Count; i++)
            {
                NoteRow noteRow = SongPlayer.NoteRows[i];
                CandidateNoteHitTest noteHit = GetNoteTestInfo(noteRow);
                if (noteHit == null || noteHit.NoteChannelInfo == null || noteHit.NoteObject == null)
                {
                    continue;
                }

                Vector3 distToGoal = noteHit.NoteChannelInfo.GoalPos - noteHit.NoteObject.transform.position;
                float dot = Vector3.Dot(distToGoal, noteHit.NoteChannelInfo.Direction);

                if (dot < 0)
                {
                    finalNotesBelowGoal.Add(noteRow);
                }
            }

            // Remove any notes in the initial list that are in this list (ie, it's still below the goal when we finish rewinding)
            List<NoteRow> noteRowsToReset = new List<NoteRow>(initialNotesToReset.Except(finalNotesBelowGoal));
            for (int i = 0; i < noteRowsToReset.Count; i++)
            {
                if (noteObjectsToIgnore.Contains(noteRowsToReset[i].LeftNoteObject) ||
                    noteObjectsToIgnore.Contains(noteRowsToReset[i].DownNoteObject) ||
                    noteObjectsToIgnore.Contains(noteRowsToReset[i].UpNoteObject) ||
                    noteObjectsToIgnore.Contains(noteRowsToReset[i].RightNoteObject))
                {
                    continue;
                }

                noteRowsToReset[i].LeftNoteHasBeenHitProcessed = false;
                noteRowsToReset[i].DownNoteHasBeenHitProcessed = false;
                noteRowsToReset[i].UpNoteHasBeenHitProcessed = false;
                noteRowsToReset[i].RightNoteHasBeenHitProcessed = false;
            }
        }
    }
}
