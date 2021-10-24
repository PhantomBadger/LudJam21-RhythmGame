using Assets.Scripts.Song;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PercentageDisplay : MonoBehaviour
{
    [Header("Sprites")]
    public List<Sprite> NumberSprites;
    public Sprite PercentageSprite;

    [Header("Audio")]
    public AudioSource TargetAudioSource;
    public SongPlayer SongPlayer;

    [Header("Renderers")]
    public SpriteRenderer FirstCharacterRenderer;
    public SpriteRenderer SecondCharacterRenderer;
    public SpriteRenderer PercentageRenderer;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    public void Start()
    {
        if (NumberSprites == null) { throw new ArgumentNullException(nameof(NumberSprites)); }
        if (PercentageSprite == null) { throw new ArgumentNullException(nameof(PercentageSprite)); }
        if (TargetAudioSource == null) { throw new ArgumentNullException(nameof(TargetAudioSource)); }
        if (SongPlayer == null) { throw new ArgumentNullException(nameof(SongPlayer)); }
        if (FirstCharacterRenderer == null) { throw new ArgumentNullException(nameof(FirstCharacterRenderer)); }
        if (SecondCharacterRenderer == null) { throw new ArgumentNullException(nameof(SecondCharacterRenderer)); }
        if (PercentageRenderer == null) { throw new ArgumentNullException(nameof(PercentageRenderer)); }

        FirstCharacterRenderer.sprite = NumberSprites[0];
        SecondCharacterRenderer.sprite = NumberSprites[0];
        PercentageRenderer.sprite = PercentageSprite;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public void Update()
    {
        if (SongPlayer.IsSongLoaded)
        {
            float percentage = TargetAudioSource.time / TargetAudioSource.clip.length;

            int percentageInt = Mathf.RoundToInt(percentage * 100f) % 100;
            List<int> digits = SplitInt(percentageInt);
            if (digits.Count == 1)
            {
                FirstCharacterRenderer.sprite = NumberSprites[0];
                SecondCharacterRenderer.sprite = NumberSprites[digits[0]];
            }
            else if (digits.Count > 1)
            {
                FirstCharacterRenderer.sprite = NumberSprites[digits[0]];
                SecondCharacterRenderer.sprite = NumberSprites[digits[1]];
            }
            else
            {
                // Something went wrong lol
                throw new InvalidOperationException();
            }
        }
    }

    private List<int> SplitInt(int input)
    {
        List<int> rawOutput = new List<int>();

        if (input == 0)
        {
            rawOutput.Add(0);
        }
        else
        {
            int curValue = input;
            while (curValue > 0)
            {
                rawOutput.Add(curValue % 10);
                curValue = curValue / 10;
            }
        
            rawOutput.Reverse();
        }

        return rawOutput;
    }
}
