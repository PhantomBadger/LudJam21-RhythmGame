using Assets.Scripts.Song.API;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementation of <see cref="INoteGameObjectFactory"/> which spawns test instances based on only the channel
/// </summary>
public class PrefabNoteObjectFactory : MonoBehaviour, INoteGameObjectFactory
{
    public GameObject NoteParent;

    // Normal Notes
    [Header("Normal Notes")]
    public GameObject LeftNotePrefab;
    public GameObject UpNotePrefab;
    public GameObject DownNotePrefab;
    public GameObject RightNotePrefab;

    // Left Fall Notes
    [Header("Left Fall Notes")]
    public GameObject LeftFallLeftNotePrefab;
    public GameObject UpFallLeftNotePrefab;
    public GameObject DownFallLeftNotePrefab;
    public GameObject RightFallLeftNotePrefab;

    // Right Fall Notes
    [Header("Right Fall Notes")]
    public GameObject LeftFallRightNotePrefab;
    public GameObject UpFallRightNotePrefab;
    public GameObject DownFallRightNotePrefab;
    public GameObject RightFallRightNotePrefab;

    // Fragile Notes
    [Header("Fragile Notes")]
    public GameObject LeftFragileNotePrefab;
    public GameObject UpFragileNotePrefab;
    public GameObject DownFragileNotePrefab;
    public GameObject RightFragileNotePrefab;

    /// <summary>
    /// Instantistes the appropriate prefab based on the provided note info
    /// </summary>
    public GameObject GetNote(Vector3 notePosition, NoteInfo noteInfo)
    {
        if (!noteInfo.IsPresent)
        {
            return null;
        }

        GameObject noteObject = null;
        switch (noteInfo.NoteType)
        {
            case NoteType.Normal:
                noteObject = GetCorrectChannelNote(LeftNotePrefab, DownNotePrefab, UpNotePrefab, RightNotePrefab, noteInfo.NoteChannel);
                break;
            case NoteType.FallLeft:
                noteObject = GetCorrectChannelNote(LeftFallLeftNotePrefab, DownFallLeftNotePrefab, UpFallLeftNotePrefab, RightFallLeftNotePrefab, noteInfo.NoteChannel);
                break;
            case NoteType.FallRight:
                noteObject = GetCorrectChannelNote(LeftFallRightNotePrefab, DownFallRightNotePrefab, UpFallRightNotePrefab, RightFallRightNotePrefab, noteInfo.NoteChannel);
                break;
            case NoteType.Fragile:
                noteObject = GetCorrectChannelNote(LeftFragileNotePrefab, DownFragileNotePrefab, UpFragileNotePrefab, RightFragileNotePrefab, noteInfo.NoteChannel);
                break;
            case NoteType.Unknown:
            default:
                throw new NotImplementedException($"Unable to handle unknown note type '{noteInfo.NoteType}'");
        }

        if (noteObject != null)
        {
            noteObject.transform.parent = NoteParent.transform;
            noteObject.transform.position = notePosition;
        }

        return noteObject;
    }

    private GameObject GetCorrectChannelNote(GameObject leftPrefab, GameObject downPrefab, GameObject upPrefab, GameObject rightPrefab, NoteChannel noteChannel)
    {
        switch (noteChannel)
        {
            case NoteChannel.Left:
                {
                    GameObject noteObject = Instantiate(leftPrefab);
                    return noteObject;
                }
            case NoteChannel.Up:
                {
                    GameObject noteObject = Instantiate(upPrefab);
                    return noteObject;
                }
            case NoteChannel.Down:
                {
                    GameObject noteObject = Instantiate(downPrefab);
                    return noteObject;
                }
            case NoteChannel.Right:
                {
                    GameObject noteObject = Instantiate(rightPrefab);
                    return noteObject;
                }
            default:
                {
                    throw new InvalidOperationException($"Unsupported Note Channel of '{noteChannel}'");
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
