using Assets.Scripts;
using Assets.Scripts.Song.FileData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class HitResponseSpawner : MonoBehaviour
    {
        public GameObject HitResponsePrefab;
        public GameObject MissResponsePrefab;
        public Vector3 SpawnOffset;

        private DateTime lastSpawned;
        private NoteChannel lastChannelSpawned;
        private float minSecondsBetweenSpawns = 0.05f;

        public void Start()
        {
            lastSpawned = DateTime.Now;
        }

        public void SpawnHitResponse(NoteHitEventArgs args)
        {
            TimeSpan timeBetweenLastSpawn = DateTime.Now - lastSpawned;

            if (timeBetweenLastSpawn.TotalSeconds < minSecondsBetweenSpawns 
                && (lastChannelSpawned == args.AttemptedChannel.NoteChannel))
            {
                return;
            }

            lastSpawned = DateTime.Now;
            lastChannelSpawned = args.AttemptedChannel.NoteChannel;
            GameObject.Instantiate(HitResponsePrefab, args.HitNote.transform.position + SpawnOffset, Quaternion.identity);
            
        }

        public void SpawnMissResponse(NoteMissEventArgs args)
        {
            // Always communicate misses
            GameObject.Instantiate(MissResponsePrefab, args.AttemptedChannel.GoalPos + SpawnOffset, Quaternion.identity);
        }
    }
}
