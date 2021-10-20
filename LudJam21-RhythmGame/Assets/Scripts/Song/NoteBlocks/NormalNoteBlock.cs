using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Song.NoteBlocks
{
    public class NormalNoteBlock : NoteBlockBase
    {
        public Transform LandingPosition;

        public override Vector3 GetLandedPosition()
        {
            return LandingPosition.position;
        }

        public override void OnFallenOnto()
        {
            // Do nothing
        }

        public override void OnLanded()
        {
            // Do nothing
        }

        public override void OnPlay()
        {
            // Do nothing
        }

        public override void OnRewind()
        {
            // Do nothing
        }
    }
}
