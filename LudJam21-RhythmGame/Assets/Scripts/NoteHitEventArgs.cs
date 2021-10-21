using Assets.Scripts.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class NoteHitEventArgs
    {
        public NoteBlock HitNote { get; set; }
        public NoteChannelInfo AttemptedChannel { get; set; }
    }
}
