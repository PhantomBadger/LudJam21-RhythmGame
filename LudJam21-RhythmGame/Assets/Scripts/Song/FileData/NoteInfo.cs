using Assets.Scripts.Song.FileData;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Song.FileData
{
    /// <summary>
    /// Metadata and info about the Note present
    /// </summary>
    public class NoteInfo
    {
        public bool IsPresent { get; set; }
        public NoteType NoteType { get; set; }
        public NoteChannel NoteChannel { get; set; }
    }
}