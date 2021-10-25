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
        private readonly NoteChannelInfo leftChannelInfo;
        private readonly NoteChannelInfo upChannelInfo;
        private readonly NoteChannelInfo rightChannelInfo;
        private readonly NoteChannelInfo downChannelInfo;
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
            NoteChannelInfo leftChannelInfo,
            NoteChannelInfo downChannelInfo,
            NoteChannelInfo upChannelInfo,
            NoteChannelInfo rightChannelInfo,
            float fallDistancePerSecond)
        {
            this.noteFactory = noteFactory ?? throw new ArgumentNullException(nameof(noteFactory));
            this.leftChannelInfo = leftChannelInfo;
            this.downChannelInfo = downChannelInfo;
            this.upChannelInfo = upChannelInfo;
            this.rightChannelInfo = rightChannelInfo;
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
            float distancePerBeat = fallDistancePerSecond / (bpm / 60f);
            for (int i = 0; i < songMetaData.Bars.Count; i++)
            {
                NoteBarInfo noteBarInfo = songMetaData.Bars[i];
                float distancePerBar = (distancePerBeat * 4);
                float distancePerNoteInBar = distancePerBar / noteBarInfo.NoteRows.Count;

                for (int j = 0; j < noteBarInfo.NoteRows.Count; j++)
                {
                    NoteRowInfo noteRowInfo = noteBarInfo.NoteRows[j];
                    NoteRow noteRow = new NoteRow()
                    {
                        NoteRowInfo = noteRowInfo,
                        TimeInSong = distanceCounter / fallDistancePerSecond,
                    };

                    // Get the appropriate GameObject for each note
                    Vector3 leftNotePosition = leftChannelInfo.GoalPos + ((-leftChannelInfo.Direction) * distanceCounter);
                    noteRow.LeftNoteObject = noteFactory.GetNote(leftNotePosition, noteRowInfo.LeftNoteInfo);
                    noteRow.LeftNoteStartPos = leftNotePosition;
                    noteRow.LeftNoteHasBeenHitProcessed = false;

                    Vector3 downNotePosition = downChannelInfo.GoalPos + ((-downChannelInfo.Direction) * distanceCounter);
                    noteRow.DownNoteObject = noteFactory.GetNote(downNotePosition, noteRowInfo.DownNoteInfo);
                    noteRow.DownNoteStartPos = downNotePosition;
                    noteRow.DownNoteHasBeenHitProcessed = false;

                    Vector3 upNotePosition = upChannelInfo.GoalPos + ((-upChannelInfo.Direction) * distanceCounter);
                    noteRow.UpNoteObject = noteFactory.GetNote(upNotePosition, noteRowInfo.UpNoteInfo);
                    noteRow.UpNoteStartPos = upNotePosition;
                    noteRow.UpNoteHasBeenHitProcessed = false;

                    Vector3 rightNotePosition = rightChannelInfo.GoalPos + ((-rightChannelInfo.Direction) * distanceCounter);
                    noteRow.RightNoteObject = noteFactory.GetNote(rightNotePosition, noteRowInfo.RightNoteInfo);
                    noteRow.RightNoteStartPos = rightNotePosition;
                    noteRow.RightNoteHasBeenHitProcessed = false;

                    // Add it to our row list
                    noteRows.Add(noteRow);

                    // Increment the counter for the next row
                    distanceCounter += distancePerNoteInBar;
                }
            }
            return noteRows;
        }
    }
}
