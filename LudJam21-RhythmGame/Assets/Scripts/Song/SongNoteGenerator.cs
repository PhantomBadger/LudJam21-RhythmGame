using Assets.Scripts.Song.API;
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
    /// Generates a Song's Notes using the provided information
    /// </summary>
    public class SongNoteGenerator
    {
        private readonly INoteGameObjectFactory noteFactory;
        private readonly float originYPos;
        private readonly float leftChannelXPos;
        private readonly float upChannelXPos;
        private readonly float rightChannelXPos;
        private readonly float downChannelXPos;
        private readonly float fallDistancePerSecond;

        /// <summary>
        /// Ctor for creating a <see cref="SongNoteGenerator"/>
        /// </summary>
        /// <param name="noteFactory">The <see cref="INoteGameObjectFactory"/> to use to generate the note Game Objects</param>
        /// <param name="originYPos">The Y position at 'origin' where we will spawn the notes relative to</param>
        /// <param name="leftChannelXPos">The X position for the left channel</param>
        /// <param name="downChannelXPos">The X position for the down channel</param>
        /// <param name="upChannelXPos">The X position for the up channel</param>
        /// <param name="rightChannelXPos">The X position for the right channel</param>
        /// <param name="fallDistancePerSecond">The distance a note will fall in a second</param>
        public SongNoteGenerator(
            INoteGameObjectFactory noteFactory, 
            float originYPos, 
            float leftChannelXPos, 
            float downChannelXPos,
            float upChannelXPos, 
            float rightChannelXPos,
            float fallDistancePerSecond)
        {
            this.noteFactory = noteFactory ?? throw new ArgumentNullException(nameof(noteFactory));
            this.originYPos = originYPos;
            this.leftChannelXPos = leftChannelXPos;
            this.downChannelXPos = downChannelXPos;
            this.upChannelXPos = upChannelXPos;
            this.rightChannelXPos = rightChannelXPos;
            this.fallDistancePerSecond = Math.Abs(fallDistancePerSecond);
        }

        /// <summary>
        /// Generates GameObjects for each NoteInfo within the provided song info
        /// </summary>
        /// <param name="songMetaData"></param>
        /// <returns></returns>
        public List<NoteRow> GenerateNotes(SongMetadata songMetaData)
        {
            float bpm = Math.Max(songMetaData.BPM, 1);

            float distanceCounter = 0;

            // Give ourselves the initial offset
            float initialOffsetDistance = Math.Abs(songMetaData.Offset) * fallDistancePerSecond;
            distanceCounter += initialOffsetDistance;

            // Iterate over each note row and generate the GameObject
            List<NoteRow> noteRows = new List<NoteRow>();
            float distancePerRow = fallDistancePerSecond / (bpm / 60f);
            for (int i = 0; i < songMetaData.Notes.Count; i++)
            {
                NoteRowInfo noteRowInfo = songMetaData.Notes[i];
                NoteRow noteRow = new NoteRow()
                {
                    NoteRowInfo = noteRowInfo
                };

                Vector3 newNotePosition = new Vector3();
                newNotePosition.y = distanceCounter;

                // Get the appropriate GameObject for each note
                newNotePosition.x = leftChannelXPos;

                noteRow.LeftNoteObject = noteFactory.GetNote(newNotePosition, noteRowInfo.LeftNoteInfo);

                newNotePosition.x = downChannelXPos;
                noteRow.DownNoteObject = noteFactory.GetNote(newNotePosition, noteRowInfo.DownNoteInfo);

                newNotePosition.x = upChannelXPos;
                noteRow.UpNoteObject = noteFactory.GetNote(newNotePosition, noteRowInfo.UpNoteInfo);

                newNotePosition.x = rightChannelXPos;
                noteRow.RightNoteObject = noteFactory.GetNote(newNotePosition, noteRowInfo.RightNoteInfo);

                // Add it to our row list
                noteRows.Add(noteRow);

                // Increment the counter for the next row
                distanceCounter += distancePerRow;
            }
            return noteRows;
        }
    }
}
