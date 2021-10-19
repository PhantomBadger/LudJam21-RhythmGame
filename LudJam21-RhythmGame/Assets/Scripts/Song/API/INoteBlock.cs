using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INoteBlock
{
    void OnRewind();

    void OnPlay();

    void OnFallenOnto();

    void OnLanded();
}
