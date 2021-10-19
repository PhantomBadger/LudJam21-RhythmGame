using Assets.Scripts.Song.FileData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Song
{
    /// <summary>
    /// An aggregate class of game note info
    /// </summary>
    public class NoteRow
    {
        public NoteRowInfo NoteRowInfo { get; set; }
        public GameObject LeftNoteObject { get; set; }
        public Vector3 LeftNoteStartPos { get; set; }
        public GameObject DownNoteObject { get; set; }
        public Vector3 DownNoteStartPos { get; set; }
        public GameObject UpNoteObject { get; set; }
        public Vector3 UpNoteStartPos { get; set; }
        public GameObject RightNoteObject { get; set; }
        public Vector3 RightNoteStartPos { get; set; }
    }
}
