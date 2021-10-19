using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Song
{
    [Serializable]
    public class NoteChannelInfo
    {
        public Vector3 GoalPos { get; set; }
        public Vector3 Direction { get; set; }

        public NoteChannelInfo()
        {

        }

        public NoteChannelInfo(Transform transform)
        {
            GoalPos = transform.position;
            Direction = transform.forward.normalized;
        }
    }
}
