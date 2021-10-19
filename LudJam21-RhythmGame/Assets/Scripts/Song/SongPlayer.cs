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
        public string SongFilePath;
        public TestNoteObjectFactory NoteObjectFactory;
        public AudioSource TargetAudioSource;

        [Header("Song Settings")]
        [Range(1, 30)]
        public float DistanceInSecond = 10;
        public Transform LeftChannelTransform;
        public Transform DownChannelTransform;
        public Transform UpChannelTransform;
        public Transform RightChannelTransform;

        [Header("Song Events")]
        public UnityEvent OnSongLoaded;
        public UnityEvent OnSongPaused;
        public UnityEvent OnSongPlay;

        private NoteChannelInfo leftChannelInfo;
        private NoteChannelInfo downChannelInfo;
        private NoteChannelInfo upChannelInfo;
        private NoteChannelInfo rightChannelInfo;
        private SongMetadata songMetadata;
        private List<NoteRow> noteRows;
        private bool isSongLoaded = false;
        private bool isPaused = true;
        private float totalSongDistance;

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

            leftChannelInfo = new NoteChannelInfo()
            {
                GoalPos = LeftChannelTransform.position,
                Direction = LeftChannelTransform.forward.normalized
            };
            downChannelInfo = new NoteChannelInfo()
            {
                GoalPos = DownChannelTransform.position,
                Direction = DownChannelTransform.forward.normalized
            };
            upChannelInfo = new NoteChannelInfo()
            {
                GoalPos = UpChannelTransform.position,
                Direction = UpChannelTransform.forward.normalized
            };
            rightChannelInfo = new NoteChannelInfo()
            {
                GoalPos = RightChannelTransform.position,
                Direction = RightChannelTransform.forward.normalized
            };

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
            songMetadata = songParser.ParseSongFile(SongFilePath);
            yield return StartCoroutine(LoadAudioClip(songMetadata));

            noteRows = GenerateSongNotes(songMetadata);

            totalSongDistance = TargetAudioSource.clip.length * DistanceInSecond;

            OnSongLoaded.Invoke();
        }

        /// <summary>
        /// Loads the song specified in <see cref="SongMetadata.MusicFilePath"/>
        /// </summary>
        private IEnumerator LoadAudioClip(SongMetadata songMetadata)
        {
            // Load the Song
            string uri = $"file://{songMetadata.MusicFilePath}";
            UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
            while (!webRequest.isDone)
            {
                yield return webRequest.SendWebRequest();
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
                leftChannelInfo,
                downChannelInfo,
                upChannelInfo,
                rightChannelInfo,
                DistanceInSecond);
            return songNoteGenerator.GenerateNotes(songMetadata);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public void Update()
        {
            if (isSongLoaded && !isPaused)
            {
                RepositionNoteRows();
            }
        }

        private void RepositionNoteRows()
        {
            float percentageThroughSong = TargetAudioSource.time / TargetAudioSource.clip.length;
            float distanceOffset = totalSongDistance * percentageThroughSong;

            // Move each note towards the goal
            for (int i = 0; i < noteRows.Count; i++)
            {
                if (noteRows[i].LeftNoteObject != null)
                {
                    noteRows[i].LeftNoteObject.transform.position = noteRows[i].LeftNoteStartPos + (leftChannelInfo.Direction * distanceOffset);
                }

                if (noteRows[i].DownNoteObject != null)
                {
                    noteRows[i].DownNoteObject.transform.position = noteRows[i].DownNoteStartPos + (downChannelInfo.Direction * distanceOffset);
                }

                if (noteRows[i].UpNoteObject != null)
                {
                    noteRows[i].UpNoteObject.transform.position = noteRows[i].UpNoteStartPos + (upChannelInfo.Direction * distanceOffset);
                }

                if (noteRows[i].RightNoteObject != null)
                {
                    noteRows[i].RightNoteObject.transform.position = noteRows[i].RightNoteStartPos + (rightChannelInfo.Direction * distanceOffset);
                }
            }
        }

        /// <summary>
        /// Starts playing the song
        /// </summary>
        public void PlaySong()
        {
            isPaused = false;
            TargetAudioSource.Play();
            RepositionNoteRows();
            OnSongPlay.Invoke();
        }

        /// <summary>
        /// Pauses the song
        /// </summary>
        public void PauseSong()
        {
            isPaused = true;
            TargetAudioSource.Pause();
            OnSongPaused.Invoke();
        }
    }
}
