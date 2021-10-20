using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface representing a landable note block
/// </summary>
public interface INoteBlock
{
    /// <summary>
    /// Called on Rewind
    /// </summary>
    void OnRewind();

    /// <summary>
    /// Called when we start Playing
    /// </summary>
    void OnPlay();

    /// <summary>
    /// Called when the player falls onto it during a rewind
    /// </summary>
    void OnFallenOnto();

    /// <summary>
    /// Called when the player lands on it during play
    /// </summary>
    void OnLanded();

    /// <summary>
    /// Gets the position for the player to land on
    /// </summary>
    Vector3 GetLandedPosition();
}
