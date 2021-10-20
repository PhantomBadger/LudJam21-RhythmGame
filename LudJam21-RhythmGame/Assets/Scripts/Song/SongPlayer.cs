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

            LeftChannelInfo = new NoteChannelInfo(LeftChannelTransform);
            DownChannelInfo = new NoteChannelInfo(DownChannelTransform);
            UpChannelInfo = new NoteChannelInfo(UpChannelTransform);
            RightChannelInfo = new NoteChannelInfo(RightChannelTransform);

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
                LeftChannelInfo,
                DownChannelInfo,
                UpChannelInfo,
                RightChannelInfo,
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
            for (int i = 0; i < NoteRows.Count; i++)
            {
                if (NoteRows[i].LeftNoteObject != null)
                {
                    NoteRows[i].LeftNoteObject.transform.position = NoteRows[i].LeftNoteStartPos + (LeftChannelInfo.Direction * distanceOffset);
                }

                if (NoteRows[i].DownNoteObject != null)
                {
                    NoteRows[i].DownNoteObject.transform.position = NoteRows[i].DownNoteStartPos + (DownChannelInfo.Direction * distanceOffset);
                }

                if (NoteRows[i].UpNoteObject != null)
                {
                    NoteRows[i].UpNoteObject.transform.position = NoteRows[i].UpNoteStartPos + (UpChannelInfo.Direction * distanceOffset);
                }

                if (NoteRows[i].RightNoteObject != null)
                {
                    NoteRows[i].RightNoteObject.transform.position = NoteRows[i].RightNoteStartPos + (RightChannelInfo.Direction * distanceOffset);
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
