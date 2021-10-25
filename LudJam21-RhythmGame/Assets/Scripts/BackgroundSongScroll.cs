using Assets.Scripts.Song;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSongScroll : MonoBehaviour
{
    [Header("Screen Info")]
    public float MaxYValue;
    public float MinYValue;
    public Transform CameraPos;

    [Header("Audio")]
    public AudioSource TargetAudioSource;
    public SongPlayer SongPlayer;

    [Header("Layers")]
    public GameObject Layer1ObjectPrefab;
    public float Layer1ScrollScale = 1.0f;
    public float Layer1YSize = 170;
    public Vector3 Layer1ScrollDir;
    public GameObject Layer2ObjectPrefab;
    public float Layer2ScrollScale = 0.5f;
    public float Layer2YSize = 170;
    public Vector3 Layer2ScrollDir;

    private List<GameObject> layer1Objects;
    private Vector3 layer1StartPos;
    private Vector3 layer1Offset;
    private List<GameObject> layer2Objects;
    private Vector3 layer2StartPos;
    private Vector3 layer2Offset;
    private bool isPostGame = false;
    private float postGameCounter;

    // Start is called before the first frame update
    public void Start()
    {
        layer1Objects = new List<GameObject>();

        Vector3 camStartPos = new Vector3(CameraPos.position.x, CameraPos.position.y, transform.position.z);
        layer1Offset = new Vector3(0, Layer1YSize);
        layer2Offset = new Vector3(0, Layer2YSize);

        layer1Objects.Add(Instantiate(Layer1ObjectPrefab, camStartPos - layer1Offset, Quaternion.identity, this.transform));
        layer1Objects.Add(Instantiate(Layer1ObjectPrefab, camStartPos, Quaternion.identity, this.transform));
        layer1Objects.Add(Instantiate(Layer1ObjectPrefab, camStartPos + layer1Offset, Quaternion.identity, this.transform));
        layer1StartPos = camStartPos;

        layer2Objects = new List<GameObject>();
        layer2Objects.Add(Instantiate(Layer2ObjectPrefab, camStartPos - layer2Offset, Quaternion.identity, this.transform));
        layer2Objects.Add(Instantiate(Layer2ObjectPrefab, camStartPos, Quaternion.identity, this.transform));
        layer2Objects.Add(Instantiate(Layer2ObjectPrefab, camStartPos + layer2Offset, Quaternion.identity, this.transform));
        layer2StartPos = camStartPos;
    }

    // Update is called once per frame
    public void Update()
    {
        if (SongPlayer.IsSongLoaded)
        {
            float distanceOffset = 0;
            if (isPostGame)
            {
                postGameCounter += Time.deltaTime;
                distanceOffset = SongPlayer.GetNoteDistanceOverTime(TargetAudioSource.time + postGameCounter);
            }
            else
            {
                distanceOffset = SongPlayer.GetNoteDistanceOverTime(TargetAudioSource.time);
            }

            Vector3 newLayer1Pos = layer1StartPos + (Layer1ScrollDir * (distanceOffset * Layer1ScrollScale));
            newLayer1Pos.y = Mathf.Repeat(newLayer1Pos.y - MinYValue, MaxYValue - MinYValue) + MinYValue;
            layer1Objects[0].transform.position = newLayer1Pos - layer1Offset;
            layer1Objects[1].transform.position = newLayer1Pos;
            layer1Objects[2].transform.position = newLayer1Pos + layer1Offset;

            Vector3 newlayer2Pos = layer2StartPos + (Layer2ScrollDir * (distanceOffset * Layer2ScrollScale));
            newlayer2Pos.y = Mathf.Repeat(newlayer2Pos.y - MinYValue, MaxYValue - MinYValue) + MinYValue;
            layer2Objects[0].transform.position = newlayer2Pos - layer2Offset;
            layer2Objects[1].transform.position = newlayer2Pos;
            layer2Objects[2].transform.position = newlayer2Pos + layer2Offset;
        }
    }

    public void SetPostGameState(bool isPostGame)
    {
        if (this.isPostGame != isPostGame)
        {
            this.isPostGame = isPostGame;
            postGameCounter = 0;
        }
    }
}
