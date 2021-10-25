using Assets.Scripts.Song.FileData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component representing info about a Note Block
/// </summary>
public class NoteBlock : MonoBehaviour
{
    public Transform LandingPosition;

    public NoteType NoteType;

    public Vector3 GetLandedPosition()
    {
        return LandingPosition.position;
    }
}
