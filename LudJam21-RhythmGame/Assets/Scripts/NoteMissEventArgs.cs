using Assets.Scripts.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class NoteMissEventArgs
    {
        public enum TypeOfMissedNote
        {
            NoAttemptedNote,
            IncorrectlyAttemptedNote,
        }
        public TypeOfMissedNote MissType { get; set; }
        public NoteChannelInfo AttemptedChannel { get; set; }
    }
}
