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

        [Range(0.1f, 3f)]
        public float HitThreshold = 0.1f;

        public KeyCode LeftChannelKey;
        public KeyCode DownChannelKey;
        public KeyCode UpChannelKey;
        public KeyCode RightChannelKey;

        public UnityEvent<NoteHitEventArgs> OnNoteHit;
        public UnityEvent<NoteMissEventArgs> OnNoteMiss;

        private bool isDetectionActive;

        /// <summary>
        /// Called on Start
        /// </summary>
        public void Start()
        {
            isDetectionActive = false;
        }

        /// <summary>
        /// Called each Frame
        /// </summary>
        public void Update()
        {
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
                        if (dist > HitThreshold)
                        {
                            // If it is still hittable
                            if ((noteRow.LeftNoteObject != null && !noteRow.LeftNoteHasBeenHitProcessed) ||
                                (noteRow.DownNoteObject != null && !noteRow.DownNoteHasBeenHitProcessed) ||
                                (noteRow.UpNoteObject != null && !noteRow.UpNoteHasBeenHitProcessed) ||
                                (noteRow.RightNoteObject != null) && !noteRow.RightNoteHasBeenHitProcessed)
                            {
                                // We have missed the note 100%!
                                // Set the hit processed flags so we don't attempt to check it again
                                InvokeNoteMissEvent(testNoteHitTest.NoteChannelInfo);
                                noteRow.LeftNoteHasBeenHitProcessed = true;
                                noteRow.DownNoteHasBeenHitProcessed = true;
                                noteRow.RightNoteHasBeenHitProcessed = true;
                                noteRow.UpNoteHasBeenHitProcessed = true;
                            }
                        }
                    }

                    
                    if (dist < HitThreshold)
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
            NoteMissEventArgs args = new NoteMissEventArgs()
            {
                AttemptedChannel = attemptedChannel,
            };

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
