using Assets.Scripts;
using Assets.Scripts.Song;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        [Range(0.01f, 3f)]
        public float AboveHitThreshold = 0.1f;
        [Range(0.01f, 3f)]
        public float BelowHitThreshold = 0.1f;
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

        /// <summary>
        /// Called on Start
        /// </summary>
        public void Start()
        {
            if (SongPlayer == null) { throw new ArgumentNullException(nameof(SongPlayer)); }

            isDetectionActive = false;
            missCooldownCounter = 0;
            isMissCooldownActive = false;
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
                                InvokeNoteMissEvent(SongPlayer.LeftChannelInfo);
                            }
                            if (noteRow.DownNoteObject != null && !noteRow.DownNoteHasBeenHitProcessed)
                            {
                                noteRow.DownNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(SongPlayer.DownChannelInfo);
                            }
                            if (noteRow.UpNoteObject != null && !noteRow.UpNoteHasBeenHitProcessed)
                            {
                                noteRow.UpNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(SongPlayer.UpChannelInfo);
                            }
                            if (noteRow.RightNoteObject != null && !noteRow.RightNoteHasBeenHitProcessed)
                            {
                                noteRow.RightNoteHasBeenHitProcessed = true;
                                InvokeNoteMissEvent(SongPlayer.RightChannelInfo);
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
                    InvokeNoteMissEvent(SongPlayer.LeftChannelInfo);
                }
                else if (downHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(SongPlayer.DownChannelInfo);
                }
                else if (upHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(SongPlayer.UpChannelInfo);
                }
                else if (rightHitInputToBeProcessed)
                {
                    InvokeNoteMissEvent(SongPlayer.RightChannelInfo);
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

        private void InvokeNoteMissEvent(NoteChannelInfo attemptedChannel)
        {
            Debug.Break();
            NoteMissEventArgs args = new NoteMissEventArgs()
            {
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

        private void InvokeNoteHitEvent(GameObject noteObject, NoteChannelInfo attemptedChannel)
        {
            NoteBlockBase blockBase = noteObject.GetComponent<NoteBlockBase>();
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

        public void EnableHitDetection(bool enabled)
        {
            isDetectionActive = enabled;
        }

        public void OutputHitMessage(NoteHitEventArgs args)
        {
            Debug.Log("HIT!");
        }

        public void OutputMissMessage(NoteMissEventArgs args)
        {
            Debug.Log("MISS!");
        }
    }
}
