using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoteBlockBase : MonoBehaviour, INoteBlock
{
    public abstract void OnFallenOnto();
    public abstract void OnLanded();
    public abstract void OnPlay();
    public abstract void OnRewind();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
