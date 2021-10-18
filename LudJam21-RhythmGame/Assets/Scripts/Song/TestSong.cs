using Assets.Scripts.Song;
using Assets.Scripts.Song.FileData;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TestSong : MonoBehaviour
{
    public string SongFilePath;
    public TestNoteObjectFactory NoteObjectFactory;
    public AudioSource SongAudioSource;

    [Range(1, 30)]
    public float DistanceInSecond = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(LoadSong());
        }
    }

    private IEnumerator LoadSong()
    {
        SongParser songParser = new SongParser();
        SongMetadata songMetaData = songParser.ParseSongFile(SongFilePath);

        // Load the Song
        string uri = $"file://{songMetaData.MusicFilePath}";
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
        while (!webRequest.isDone)
        {
            yield return webRequest.SendWebRequest();
        }

        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
        SongAudioSource.clip = audioClip;
        SongAudioSource.Pause();

        SongNoteGenerator songNoteGenerator = new SongNoteGenerator(NoteObjectFactory, 0, -3f, -1f, 1f, 3f, DistanceInSecond);
        List<NoteRow> noteRows = songNoteGenerator.GenerateNotes(songMetaData);

        yield return StartCoroutine(DoNoteMovementTest(noteRows));
    }

    private IEnumerator DoNoteMovementTest(List<NoteRow> noteRows)
    {
        SongAudioSource.Play();

        while (SongAudioSource.time < SongAudioSource.clip.length)
        {
            for (int i = 0; i < noteRows.Count; i++)
            {
                if (noteRows[i].LeftNoteObject != null)
                {
                    noteRows[i].LeftNoteObject.transform.position -= new Vector3(0, DistanceInSecond * Time.deltaTime);
                }

                if (noteRows[i].DownNoteObject != null)
                {
                    noteRows[i].DownNoteObject.transform.position -= new Vector3(0, DistanceInSecond * Time.deltaTime);
                }

                if (noteRows[i].UpNoteObject != null)
                {
                    noteRows[i].UpNoteObject.transform.position -= new Vector3(0, DistanceInSecond * Time.deltaTime);
                }

                if (noteRows[i].RightNoteObject != null)
                {
                    noteRows[i].RightNoteObject.transform.position -= new Vector3(0, DistanceInSecond * Time.deltaTime);
                }
            }

            yield return null;
        }
    }
}
