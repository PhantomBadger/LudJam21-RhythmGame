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
        public PrefabNoteObjectFactory NoteObjectFactory;
        public AudioSource TargetAudioSource;
        public GameObject NoteFloor;
        public Vector3 NoteFloorDir;

        [Header("Song Settings")]
        [Range(1, 300)]
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
        private bool isRewinding = false;
        private float totalSongDistance;
        private Vector3 noteFloorStartPos;

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
            if (isSongLoaded && !isPaused)
            {
                RepositionNoteRows();
            }
        }

        public float GetNoteDistanceOverTime(float timeDuration)
        {
            float percentageOfSong = timeDuration / TargetAudioSource.clip.length;
            float distance = totalSongDistance * percentageOfSong;

            return distance;
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

            NoteFloor.transform.position = noteFloorStartPos + (NoteFloorDir.normalized * distanceOffset);
        }

        /// <summary>
        /// Starts playing the song - this resets the song also!
        /// </summary>
        public void PlaySong()
        {
            isPaused = false;
            isRewinding = false;
            TargetAudioSource.Play();
            TargetAudioSource.pitch = Math.Abs(TargetAudioSource.pitch);
            RepositionNoteRows();
            OnSongPlay.Invoke();
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
                RepositionNoteRows();
                OnSongPlay.Invoke();
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
                OnSongPaused.Invoke();
            }
        }

        public void ForwardSong()
        {
            TargetAudioSource.pitch = Math.Abs(TargetAudioSource.pitch / 2);
            if (TargetAudioSource.time <= 0.1)
            {
                // Restart it if we've rewound to the start
                TargetAudioSource.Play();
            }
            isRewinding = false;
        }

        public void RewindSong()
        {
            TargetAudioSource.pitch = Math.Abs(TargetAudioSource.pitch * 2) * -1;
            isRewinding = true;
        }
    }
}
