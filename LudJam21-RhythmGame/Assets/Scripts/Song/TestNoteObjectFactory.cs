using Assets.Scripts.Song.API;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementation of <see cref="INoteGameObjectFactory"/> which spawns test instances based on only the channel
/// </summary>
public class TestNoteObjectFactory : MonoBehaviour, INoteGameObjectFactory
{
    public GameObject NoteParent;
    public GameObject LeftNotePrefab;
    public GameObject UpNotePrefab;
    public GameObject DownNotePrefab;
    public GameObject RightNotePrefab;

    /// <summary>
    /// Instantistes the appropriate prefab based on the provided note info
    /// </summary>
    public GameObject GetNote(Vector3 notePosition, NoteInfo noteInfo)
    {
        if (!noteInfo.IsPresent)
        {
            return null;
        }

        switch (noteInfo.NoteChannel)
        {
            case NoteChannel.Left:
                {
                    GameObject noteObject = Instantiate(LeftNotePrefab, notePosition, Quaternion.identity);
                    noteObject.transform.parent = NoteParent.transform;
                    return noteObject;
                }
            case NoteChannel.Up:
                {
                    GameObject noteObject = Instantiate(UpNotePrefab, notePosition, Quaternion.identity);
                    noteObject.transform.parent = NoteParent.transform;
                    return noteObject;
                }
            case NoteChannel.Down:
                {
                    GameObject noteObject = Instantiate(DownNotePrefab, notePosition, Quaternion.identity);
                    noteObject.transform.parent = NoteParent.transform;
                    return noteObject;
                }
            case NoteChannel.Right:
                {
                    GameObject noteObject = Instantiate(RightNotePrefab, notePosition, Quaternion.identity);
                    noteObject.transform.parent = NoteParent.transform;
                    return noteObject;
                }
            default:
                {
                    throw new InvalidOperationException($"Unsupported Note Channel of '{noteInfo.NoteChannel}'");
                }
        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        if (LeftNotePrefab == null)
        {
            throw new ArgumentNullException(nameof(LeftNotePrefab));
        }

        if (UpNotePrefab == null)
        {
            throw new ArgumentNullException(nameof(UpNotePrefab));
        }

        if (DownNotePrefab == null)
        {
            throw new ArgumentNullException(nameof(DownNotePrefab));
        }

        if (RightNotePrefab == null)
        {
            throw new ArgumentNullException(nameof(RightNotePrefab));
        }
    }
}
