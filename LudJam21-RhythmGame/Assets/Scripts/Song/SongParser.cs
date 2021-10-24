using Assets.Scripts.Song.FileData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Song
{
    /// <summary>
    /// A class which is responsible for parsing a song from a provided song file
    /// </summary>
    public class SongParser
    {
        public SongMetadata ParseSongFile(string filePath)
        {
            // Exit early if the file path is bad
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Debug.LogError($"Unable to parse Song File from '{filePath}', File doesn't exist!");
                return null;
            }

            // Load up the file
            List<string> fileContents = new List<string>();
            try
            {
                fileContents.AddRange(File.ReadAllLines(filePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"Encountered exception during Parsing of File '{filePath}': {e.ToString()}");
                return null;
            }

            // Begin parsing the file contents
            bool isInNotes = false;
            int rowIndex = 0;
            SongMetadata songMetadata = new SongMetadata();

            NoteBarInfo currentBar = new NoteBarInfo();

            for (int i = 0; i < fileContents.Count; i++)
            {
                string line = fileContents[i].Trim();

                // Skip Comments
                if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Begin reading in header information
                if (!isInNotes)
                {
                    if (line.StartsWith("#"))
                    {
                        isInNotes = ParseHeaderTags(line, ref songMetadata);
                    }
                }
                else
                { 
                    if (line.StartsWith(","))
                    {
                        songMetadata.Bars.Add(currentBar);
                        currentBar = new NoteBarInfo();
                        continue;
                    }

                    // Parse the Note information
                    NoteRowInfo noteRow = ParseNoteRow(line, rowIndex++);

                    if (noteRow != null)
                    {
                        currentBar.NoteRows.Add(noteRow);
                    }
                }
            }

            Debug.Log($"Song Parse complete! Identified '{songMetadata.Bars.Count}' Note Bars total!");
            return songMetadata;
        }

        /// <summary>
        /// Parses the Notes from the provided line
        /// </summary>
        private NoteRowInfo ParseNoteRow(string line, int rowIndex)
        {

            // Skip any commas used to break up bars in the song file, or skip any lines with not enough notes
            if (line.StartsWith(",") || line.Length < 4)
            {
                return null;
            }

            NoteRowInfo noteRow = new NoteRowInfo();
            noteRow.LeftNoteInfo = ParseNoteInfo(line[0], NoteChannel.Left, rowIndex);
            noteRow.DownNoteInfo = ParseNoteInfo(line[1], NoteChannel.Down, rowIndex);
            noteRow.UpNoteInfo = ParseNoteInfo(line[2], NoteChannel.Up, rowIndex);
            noteRow.RightNoteInfo = ParseNoteInfo(line[3], NoteChannel.Right, rowIndex);

            return noteRow;
        }

        /// <summary>
        /// Parses the appropriate <see cref="NoteType"/> and other metadata from the provided raw note value
        /// </summary>
        private NoteInfo ParseNoteInfo(char noteValue, NoteChannel noteChannel, int rowIndex)
        {
            // Init the note info to no note present
            NoteInfo noteInfo = new NoteInfo()
            {
                IsPresent = false,
                NoteType = NoteType.Unknown,
                NoteChannel = noteChannel,
            };

            // Exit early if there's no int here
            if (!int.TryParse(noteValue.ToString(), out int intNoteValue))
            {
                Debug.LogError($"Failed to parse Note of '{noteValue}' in row '{rowIndex}'");
                return noteInfo;
            }

            switch (intNoteValue)
            {
                // No Note present
                case 0:
                    {
                        noteInfo.IsPresent = false;
                        break;
                    }
                // A Normal note
                case 1:
                    {
                        noteInfo.IsPresent = true;
                        noteInfo.NoteType = NoteType.Normal;
                        break;
                    }
                // A note which causes a fall left when you land on it
                case 2:
                    {
                        noteInfo.IsPresent = true;
                        noteInfo.NoteType = NoteType.FallLeft;
                        break;
                    }
                // A note which causes a fall right when you land on it
                case 3:
                    {
                        noteInfo.IsPresent = true;
                        noteInfo.NoteType = NoteType.FallRight;
                        break;
                    }
                // A note which breaks when you land on it
                case 4:
                    {
                        noteInfo.IsPresent = true;
                        noteInfo.NoteType = NoteType.Fragile;
                        break;
                    }
            }
            return noteInfo;
        }

        /// <summary>
        /// Parses the expected header tags and populates the song meta data
        /// </summary>
        /// <returns><c>true</c> if we are done with the header and have started notes, <c>false</c> if we are still in the header</returns>
        private bool ParseHeaderTags(string line, ref SongMetadata songMetadata)
        {
            string key = line.Substring(0, line.IndexOf(':')).Trim('#');
            string value = line.Substring(line.IndexOf(':') + 1).Trim(';');

            switch (key.ToLower())
            {
                case "title":
                    {
                        Debug.Log($"Identifying TITLE as '{value}'");
                        songMetadata.Title = value;
                        break;
                    }
                case "offset":
                    {
                        if (float.TryParse(value, out float floatValue))
                        {
                            Debug.Log($"Identifying OFFSET as '{floatValue}'");
                            songMetadata.Offset = floatValue;
                        }
                        else
                        {
                            songMetadata.Offset = 0;
                            Debug.LogWarning($"Failed to parse OFFSET of '{value}', using '{songMetadata.Offset}' instead");
                        }
                        break;
                    }
                case "bpms":
                case "bpm":
                    {
                        if (float.TryParse(value, out float floatValue))
                        {
                            Debug.Log($"Identifying BPM as '{floatValue}'");
                            songMetadata.BPM = floatValue;
                        }
                        else
                        {
                            songMetadata.BPM = 120;
                            Debug.LogWarning($"Failed to parse BPM of '{value}', using '{songMetadata.BPM}' instead");
                        }
                        break;
                    }
                case "music":
                    {
                        Debug.Log($"Identifying MUSIC as '{value}'");
                        songMetadata.MusicFilePath = value;
                        break;
                    }
                case "notes":
                    {
                        Debug.Log($"Identified NOTES tag as finishing Header");
                        return true;
                    }
                default:
                    {
                        Debug.LogWarning($"Identified Unknown Tag of '{key}', Skipping!");
                        break;
                    }
            }
            return false;
        }
    }
}
