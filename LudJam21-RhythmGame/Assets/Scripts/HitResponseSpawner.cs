using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class HitResponseSpawner : MonoBehaviour
    {
        public GameObject HitResponsePrefab;
        public GameObject MissResponsePrefab;

        public void SpawnHitResponse(NoteHitEventArgs args)
        {
            GameObject.Instantiate(HitResponsePrefab, args.HitNote.transform.position, Quaternion.identity);
        }

        public void SpawnMissResponse(NoteMissEventArgs args)
        {
            GameObject.Instantiate(MissResponsePrefab, args.AttemptedChannel.GoalPos, Quaternion.identity);
        }
    }
}
