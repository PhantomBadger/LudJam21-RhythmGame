using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract base implementation of <see cref="INoteBlock"/> which implements <see cref="MonoBehaviour"/>
/// so we can get the NoteBlock implementations from a generic root
/// </summary>
public abstract class NoteBlockBase : MonoBehaviour, INoteBlock
{
    public abstract void OnFallenOnto();
    public abstract void OnLanded();
    public abstract void OnPlay();
    public abstract void OnRewind();
    public abstract Vector3 GetLandedPosition();

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
