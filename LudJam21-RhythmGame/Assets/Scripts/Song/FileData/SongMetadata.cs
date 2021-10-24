using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Song.FileData
{
    /// <summary>
    /// An aggregate class containing info about the Song and all its Notes
    /// </summary>
    public class SongMetadata
    {
        public string Title { get; set; }
        public float Offset { get; set; }
        public float BPM { get; set; }
        public string MusicFilePath { get; set; }
        public List<NoteBarInfo> Bars { get; set; }

        public SongMetadata()
        {
            Bars = new List<NoteBarInfo>();
            BPM = 1;
        }
    }
}
