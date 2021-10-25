using Assets.Scripts.Song;
using Assets.Scripts.Song.API;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Assets.Scripts.Song
{
    public class SongPlayer : MonoBehaviour
    {
        public SongMetadata SongMetadata { get; set; }
        public NoteChannelInfo LeftChannelInfo { get; set; }
        public NoteChannelInfo DownChannelInfo { get; set; }
        public NoteChannelInfo UpChannelInfo { get; set; }
        public NoteChannelInfo RightChannelInfo { get; set; }
        public List<NoteRow> NoteRows { get; set; }

        public float StartTimeOffsetInSeconds = 0;
        public string SongFilePath;
        public PrefabNoteObjectFactory NoteObjectFactory;
        public AudioSource TargetAudioSource;
        public GameObject NoteFloor;
        public Vector3 NoteFloorDir;

        [Header("Song Settings")]
        [Range(1, 300)]
        public float DistanceInSecond = 10;
        public float ForwardPitch = 1f;
        public float RewindPitch = -2f;
        public float ForwardVolume = 0.6f;
        public float RewindVolume = 0.3f;
        public Transform LeftChannelTransform;
        public Transform DownChannelTransform;
        public Transform UpChannelTransform;
        public Transform RightChannelTransform;

        [Header("Rewind Grace Period")]
        public float RewindGracePeriodInSeconds = 1.5f;

        [Header("Song Events")]
        public UnityEvent OnSongLoaded;
        public UnityEvent OnSongPaused;
        public UnityEvent OnSongPlay;
        public UnityEvent OnRewind;

        private bool isSongLoaded = false;
        private bool isPaused = true;
        private bool isRewinding = false;
        private float totalSongDistance;
        private Vector3 noteFloorStartPos;
        private bool isInRewindGracePeriod;
        private bool isInGracePeriodSetup;
        private float rewindGracePeriodCounter;
        private float gracePeriodDistance;
        private List<NoteRow> gracePeriodNotes;

        public bool IsSongLoaded
        {
            get
            {
                return isSongLoaded;
            }
        }

        public bool IsSongPaused
        {
            get
            {
                return isPaused;
            }
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        public void Start()
        {
            if (!File.Exists(SongFilePath))
            {
                throw new InvalidDataException($"Provided Song File {SongFilePath} doesn't exist!");
            }

            if (NoteObjectFactory == null) { throw new ArgumentNullException(nameof(NoteObjectFactory)); }
            if (TargetAudioSource == null) { throw new ArgumentNullException(nameof(TargetAudioSource)); }

            if (LeftChannelTransform == null) { throw new ArgumentNullException(nameof(LeftChannelTransform)); }
            if (DownChannelTransform == null) { throw new ArgumentNullException(nameof(DownChannelTransform)); }
            if (UpChannelTransform == null) { throw new ArgumentNullException(nameof(UpChannelTransform)); }
            if (RightChannelTransform == null) { throw new ArgumentNullException(nameof(RightChannelTransform)); }

            LeftChannelInfo = new NoteChannelInfo(LeftChannelTransform, NoteChannel.Left);
            DownChannelInfo = new NoteChannelInfo(DownChannelTransform, NoteChannel.Down);
            UpChannelInfo = new NoteChannelInfo(UpChannelTransform, NoteChannel.Up);
            RightChannelInfo = new NoteChannelInfo(RightChannelTransform, NoteChannel.Right);

            noteFloorStartPos = NoteFloor.transform.position;

            StartCoroutine(InitialiseSong());
        }

        /// <summary>
        /// Initialises the song over multiple frames by loading the 
        /// audio clip and then generating and placing the appropriate notes
        /// </summary>
        private IEnumerator InitialiseSong()
        {
            // Parse the song and load it into the target
            SongParser songParser = new SongParser();
            SongMetadata = songParser.ParseSongFile(SongFilePath);
            yield return StartCoroutine(LoadAudioClip(SongMetadata));

            NoteRows = GenerateSongNotes(SongMetadata);

            totalSongDistance = TargetAudioSource.clip.length * DistanceInSecond;
            gracePeriodNotes = new List<NoteRow>();

            OnSongLoaded.Invoke();
        }

        /// <summary>
        /// Loads the song specified in <see cref="SongMetadata.MusicFilePath"/>
        /// </summary>
        private IEnumerator LoadAudioClip(SongMetadata songMetadata)
        {
            // Load the Song
            string musicFilePath = songMetadata.MusicFilePath;
            if (!Path.IsPathRooted(musicFilePath))
            {
                FileInfo songFileInfo = new FileInfo(SongFilePath);
                musicFilePath = Path.Combine(Path.GetDirectoryName(songFileInfo.FullName), musicFilePath);
            }
            string uri = $"file://{musicFilePath}";
            UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
            while (!webRequest.isDone)
            {
                yield return webRequest.SendWebRequest();
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Web Request for '{uri}' Failed with error: '{webRequest.error}'");
            }

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            TargetAudioSource.clip = audioClip;
            TargetAudioSource.Pause();

            isSongLoaded = true;
        }

        /// <summary>
        /// Generates the song notes
        /// </summary>
        /// <param name="songMetadata"></param>
        private List<NoteRow> GenerateSongNotes(SongMetadata songMetadata)
        {
            SongNoteGenerator songNoteGenerator = new SongNoteGenerator(
                NoteObjectFactory,
                LeftChannelInfo,
                DownChannelInfo,
                UpChannelInfo,
                RightChannelInfo,
                DistanceInSecond);
            return songNoteGenerator.GenerateNotes(songMetadata);
        }

        /// <summary>
        /// FixedUpdate is called each physics tick
        /// </summary>
        public void FixedUpdate()
        {
            if (isSongLoaded)
            {
                // If we're currently in the grace period after the rewind we will handle specific note 
                // positions manually
                if (isInRewindGracePeriod)
                {
                    // If we are in the set up phase we're still moving some of the notes backwards
                    if (isInGracePeriodSetup)
                    {
                        if (rewindGracePeriodCounter < RewindGracePeriodInSeconds)
                        {
                            // Continue moving backwards
                            rewindGracePeriodCounter += (Time.deltaTime * Math.Abs(RewindPitch));
                            rewindGracePeriodCounter = Math.Min(rewindGracePeriodCounter, RewindGracePeriodInSeconds);

                            float gracePeriodDistanceOffset = GetNoteDistanceOverTime(rewindGracePeriodCounter);
                            float currentDistance = GetNoteDistanceOverTime(TargetAudioSource.time);

                            for (int i = 0; i < gracePeriodNotes.Count; i++)
                            {
                                RepositionNoteRow(gracePeriodNotes[i], currentDistance - gracePeriodDistanceOffset);
                            }
                        }
                        else
                        {
                            isInGracePeriodSetup = false;
                            rewindGracePeriodCounter = RewindGracePeriodInSeconds;
                        }
                    }
                    // We then move them back towards the player and start the song again once they align
                    else
                    {
                        // Move forwards
                        if (rewindGracePeriodCounter > 0)
                        {
                            rewindGracePeriodCounter -= (Time.deltaTime * Math.Abs(ForwardPitch));
                            rewindGracePeriodCounter = Math.Max(rewindGracePeriodCounter, 0);

                            float gracePeriodDistanceOffset = GetNoteDistanceOverTime(rewindGracePeriodCounter);
                            float currentDistance = GetNoteDistanceOverTime(TargetAudioSource.time);

                            for (int i = 0; i < gracePeriodNotes.Count; i++)
                            {
                                RepositionNoteRow(gracePeriodNotes[i], currentDistance - gracePeriodDistanceOffset);
                            }
                        }
                        else
                        {
                            isInRewindGracePeriod = false;
                            UnpauseSong();
                            ForwardSong();
                        }
                    }
                }
                else if (!isPaused)
                {
                    RepositionAllNoteRowsToSong();
                }
            }
        }

        public float GetNoteDistanceOverTime(float timeDuration)
        {
            float percentageOfSong = timeDuration / TargetAudioSource.clip.length;
            float distance = totalSongDistance * percentageOfSong;

            return distance;
        }

        private void RepositionAllNoteRowsToSong()
        {
            float percentageThroughSong = TargetAudioSource.time / TargetAudioSource.clip.length;
            float distanceOffset = totalSongDistance * percentageThroughSong;

            // Move each note towards the goal
            for (int i = 0; i < NoteRows.Count; i++)
            {
                RepositionNoteRow(NoteRows[i], distanceOffset);
            }

            NoteFloor.transform.position = noteFloorStartPos + (NoteFloorDir.normalized * distanceOffset);
        }

        private void RepositionNoteRow(NoteRow noteRow, float distanceOffset)
        {
            if (noteRow.LeftNoteObject != null)
            {
                noteRow.LeftNoteObject.transform.position = noteRow.LeftNoteStartPos + (LeftChannelInfo.Direction * distanceOffset);
            }

            if (noteRow.DownNoteObject != null)
            {
                noteRow.DownNoteObject.transform.position = noteRow.DownNoteStartPos + (DownChannelInfo.Direction * distanceOffset);
            }

            if (noteRow.UpNoteObject != null)
            {
                noteRow.UpNoteObject.transform.position = noteRow.UpNoteStartPos + (UpChannelInfo.Direction * distanceOffset);
            }

            if (noteRow.RightNoteObject != null)
            {
                noteRow.RightNoteObject.transform.position = noteRow.RightNoteStartPos + (RightChannelInfo.Direction * distanceOffset);
            }
        }

        /// <summary>
        /// Starts playing the song - this resets the song also!
        /// </summary>
        public void PlaySong()
        {
            isPaused = false;
            isRewinding = false;
            TargetAudioSource.Play();
            TargetAudioSource.time = StartTimeOffsetInSeconds;
            TargetAudioSource.pitch = ForwardPitch;
            TargetAudioSource.volume = ForwardVolume;
            RepositionAllNoteRowsToSong();
            OnSongPlay.Invoke();
        }

        public void RewindGracePeriodThenUnpause(List<GameObject> objectsToExcludeInGracePeriod = null)
        {
            if (objectsToExcludeInGracePeriod == null)
            {
                objectsToExcludeInGracePeriod = new List<GameObject>();
            }
            PauseSong();
            isInRewindGracePeriod = true;
            isInGracePeriodSetup = true;
            rewindGracePeriodCounter = 0;
            TargetAudioSource.pitch = 0;

            gracePeriodDistance = GetNoteDistanceOverTime(RewindGracePeriodInSeconds);

            gracePeriodNotes = new List<NoteRow>();
            for (int i = 0; i < NoteRows.Count; i++)
            {
                NoteRow noteRow = NoteRows[i];
                CandidateNoteHitTest noteHit = GetNoteTestInfo(noteRow);
                if (noteHit != null && noteHit.NoteObject != null && noteHit.NoteChannelInfo != null)
                {
                    if (objectsToExcludeInGracePeriod.Contains(noteHit.NoteObject))
                    {
                        continue;
                    }

                    if (noteHit.NoteObject.transform.position.y > noteHit.NoteChannelInfo.GoalPos.y)
                    {
                        gracePeriodNotes.Add(noteRow);
                    }
                }
            }
        }

        /// <summary>
        /// Resumes playing the song
        /// </summary>
        public void UnpauseSong()
        {
            if (isPaused)
            {
                isPaused = false;
                TargetAudioSource.UnPause();
                RepositionAllNoteRowsToSong();
                OnSongPlay?.Invoke();
            }
        }

        /// <summary>
        /// Pauses the song
        /// </summary>
        public void PauseSong()
        {
            if (!isPaused)
            {
                isPaused = true;
                TargetAudioSource.Pause();
                OnSongPaused?.Invoke();
            }
        }

        public void ForwardSong()
        {
            TargetAudioSource.pitch = ForwardPitch;
            TargetAudioSource.volume = ForwardVolume;
            if (TargetAudioSource.time <= 0.1)
            {
                // Restart it if we've rewound to the start
                TargetAudioSource.Play();
            }
            isRewinding = false;
        }

        public void RewindSong()
        {
            TargetAudioSource.pitch = RewindPitch;
            TargetAudioSource.volume = RewindVolume;
            isRewinding = true;
            OnRewind?.Invoke();
        }

        /// <summary>
        /// Gets info about the note to perform the distance check on from the given row
        /// </summary>
        public CandidateNoteHitTest GetNoteTestInfo(NoteRow noteRow)
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
                    NoteChannelInfo = LeftChannelInfo,
                };
            }

            if (noteRow.DownNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.DownNoteObject,
                    NoteChannelInfo = DownChannelInfo,
                };
            }

            if (noteRow.UpNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.UpNoteObject,
                    NoteChannelInfo = UpChannelInfo,
                };
            }

            if (noteRow.RightNoteObject != null)
            {
                return new CandidateNoteHitTest()
                {
                    NoteObject = noteRow.RightNoteObject,
                    NoteChannelInfo = RightChannelInfo,
                };
            }

            return null;
        }

    }
}
