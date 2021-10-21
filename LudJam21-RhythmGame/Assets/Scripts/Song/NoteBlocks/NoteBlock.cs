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

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    public virtual void Start()
    {
        
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public virtual void Update()
    {
        
    }
}
