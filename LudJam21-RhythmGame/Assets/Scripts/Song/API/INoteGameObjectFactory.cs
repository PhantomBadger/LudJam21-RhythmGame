using Assets.Scripts.Song.FileData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Song.API
{
    /// <summary>
    /// An interface which knows how to make a note gameobject from the provided noteinfo
    /// </summary>
    public interface INoteGameObjectFactory
    {
        public GameObject GetNote(Vector3 notePosition, NoteInfo noteInfo); 
    }
}
