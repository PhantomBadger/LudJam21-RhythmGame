using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Song.FileData
{
    /// <summary>
    /// An aggregate class containing info about the notes on this row
    /// </summary>
    public class NoteRowInfo
    {
        public NoteInfo LeftNoteInfo { get; set; }
        public NoteInfo UpNoteInfo { get; set; }
        public NoteInfo DownNoteInfo { get; set; }
        public NoteInfo RightNoteInfo { get; set; }
    }
}
