using Assets.Scripts.Song.FileData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Song.FileData
{
    public class NoteBarInfo
    {
        public List<NoteRowInfo> NoteRows;

        public NoteBarInfo()
        {
            NoteRows = new List<NoteRowInfo>();
        }
    }
}
