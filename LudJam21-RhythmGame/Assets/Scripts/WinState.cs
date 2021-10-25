using Assets.Scripts;
using Assets.Scripts.Song;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinState : MonoBehaviour
{
    [Header("Condition")]
    public AudioSource TargetAudioSource;
    public float PercentageToWin;

    [Header("Settings")]
    public float DisplayFadeTimeInSeconds = 2.5f;

    [Header("Percentage Display")]
    public SpriteRenderer PercentageDisplayFirstChar;
    public SpriteRenderer PercentageDisplaySecondChar;
    public SpriteRenderer PercentageDisplayPercentageChar;

    [Header("Clear Text")]
    public SpriteRenderer[] ClearSpriteRenderers;
    public float SinWaveDistance;
    public float SinWaveSpeed;
    public float BetweenCharacterTimeOffsetInSeconds;

    [Header("Background")]
    public BackgroundSongScroll BackgroundSongScroll;

    [Header("Player")]
    public GameObject PlayerObject;
    public Animator PlayerAnimator;
    public Transform PlayerWinPos;
    public float PlayerMoveTimeInSeconds = 5f;

    [Header("Gameplay Scripts")]
    public SongPlayer SongPlayer;
    public NoteHitDetector NoteHitDetector;
    public PlayerController PlayerController;

    private bool isInPostGame;
    private float characterFadeCounter = 0;
    private float playerMoveCounter = 0;
    private Vector3 playerWinStartPos;
    private List<Vector3> clearSpriteStartPos;

    // Start is called before the first frame update
    public void Start()
    {
        isInPostGame = false;

        // Hide all of the sprite renderers for the clear text
        clearSpriteStartPos = new List<Vector3>();
        for (int i = 0; i < ClearSpriteRenderers.Length; i++)
        {
            Color color = ClearSpriteRenderers[i].color;
            color.a = 0;
            ClearSpriteRenderers[i].color = color;

            clearSpriteStartPos.Add(ClearSpriteRenderers[i].transform.position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isInPostGame)
        {
            isInPostGame = IsWinStateMet();
        }
        else
        {
            // We have won!

            // Start Looping the BG
            BackgroundSongScroll.SetPostGameState(true);

            // Handle fading of characters and displays
            if (characterFadeCounter < DisplayFadeTimeInSeconds)
            {
                characterFadeCounter += Time.fixedDeltaTime;
                float normalizedValue = characterFadeCounter / DisplayFadeTimeInSeconds;

                // Fade in the Clear Sprites
                for (int i = 0; i < ClearSpriteRenderers.Length; i++)
                {
                    Color clearColour = ClearSpriteRenderers[i].color;
                    clearColour.a = Mathf.Lerp(0f, 1f, normalizedValue);
                    ClearSpriteRenderers[i].color = clearColour;
                }

                // Fade out the display
                Color displayColour = PercentageDisplayFirstChar.color;
                displayColour.a = Mathf.Lerp(1f, 0f, normalizedValue);
                PercentageDisplayFirstChar.color = displayColour;
                PercentageDisplaySecondChar.color = displayColour;
                PercentageDisplayPercentageChar.color = displayColour;
            }
            else
            {
                for (int i = 0; i < ClearSpriteRenderers.Length; i++)
                {
                    Color clearColour = ClearSpriteRenderers[i].color;
                    clearColour.a = 1;
                    ClearSpriteRenderers[i].color = clearColour;
                }

                Color displayColour = PercentageDisplayFirstChar.color;
                displayColour.a = 0;
                PercentageDisplayFirstChar.color = displayColour;
                PercentageDisplaySecondChar.color = displayColour;
                PercentageDisplayPercentageChar.color = displayColour;
            }

            // Disable any player/game state scripts
            SongPlayer.PauseSong();
            SongPlayer.enabled = false;
            NoteHitDetector.EnableHitDetection(false);
            NoteHitDetector.enabled = false;
            PlayerController.enabled = false;

            // Change the Player's sprite and start lerping to finish pos
            PlayerAnimator.SetTrigger("PlayerWinTrigger");
            if (playerMoveCounter < PlayerMoveTimeInSeconds)
            {
                playerMoveCounter += Time.fixedDeltaTime;
                float normalizedValue = playerMoveCounter / PlayerMoveTimeInSeconds;

                PlayerObject.transform.position = Vector3.Lerp(playerWinStartPos, PlayerWinPos.position, normalizedValue);
            }

            // Wiggle the clear text
            for (int i = 0; i < ClearSpriteRenderers.Length; i++)
            {
                ClearSpriteRenderers[i].transform.position = clearSpriteStartPos[i] + new Vector3(0, Mathf.Sin((BetweenCharacterTimeOffsetInSeconds * i) + (Time.time * SinWaveSpeed)) * SinWaveDistance);
            }
        }
    }

    private bool IsWinStateMet()
    {
        if (SongPlayer.IsSongLoaded)
        {
            float percentage = TargetAudioSource.time / TargetAudioSource.clip.length;
            if (percentage >= PercentageToWin)
            {
                playerWinStartPos = PlayerObject.transform.position;
                return true;
            }
        }
        return false;
    }
}
