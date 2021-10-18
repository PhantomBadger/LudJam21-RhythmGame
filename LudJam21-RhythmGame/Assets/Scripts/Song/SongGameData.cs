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
    /// An aggregate class of Song Metadata and GameObject data
    /// </summary>
    public class SongGameData
    {
        public SongMetadata SongMetadata;

        public List<NoteRow> NoteObjects;
    }
}
