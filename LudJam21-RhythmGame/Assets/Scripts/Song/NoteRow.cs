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
        public GameObject DownNoteObject { get; set; }
        public GameObject UpNoteObject { get; set; }
        public GameObject RightNoteObject { get; set; }
    }
}
