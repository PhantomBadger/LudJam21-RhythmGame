using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuController : MonoBehaviour
{
    private enum MenuButtons
    {
        Start,
        Rebind,
    }

    public GameObject MainMenu;
    public GameObject SelectionArrow;
    public SpriteRenderer StartButton;
    public SpriteRenderer RebindButton;
    public Vector3 ArrowOffset;
    public PauseController PauseController;

    [Header("Rebinding")]
    public Sprite[] AlphaSprites;
    public SpriteRenderer FirstChar;
    public SpriteRenderer SecondChar;
    public SpriteRenderer ThirdChar;
    public SpriteRenderer FourthChar;
    public SpriteRenderer SelectionUnderline;
    public Vector3 UnderlineOffset;

    [Header("Events")]
    public UnityEvent OnStart;

    private MenuButtons activeButton;
    private bool isMenuActive;
    private bool isRebinding;
    private KeyCode firstKeyPressed = KeyCode.None;
    private KeyCode secondKeyPressed = KeyCode.None;
    private KeyCode thirdKeyPressed = KeyCode.None;
    private KeyCode fourthKeyPressed = KeyCode.None;

    private const int EnumLetterStartIndex = 97;
    private const int EnumLetterEndIndex = 122;
    private readonly KeyCode[] ValidKeyCodes = new KeyCode[]
    {
        KeyCode.A,
        KeyCode.B,
        KeyCode.C,
        KeyCode.D,
        KeyCode.E,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.I,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.M,
        KeyCode.N,
        KeyCode.O,
        KeyCode.P,
        KeyCode.Q,
        KeyCode.R,
        KeyCode.S,
        KeyCode.T,
        KeyCode.U,
        KeyCode.V,
        KeyCode.W,
        KeyCode.X,
        KeyCode.Y,
        KeyCode.Z,
    };

    // Start is called before the first frame update
    public void Start()
    {
        isMenuActive = true;
        isRebinding = false;
        PauseController.SetCanPause(false);
        ExitRebind();
        activeButton = MenuButtons.Start;

        FirstChar.sprite = GetSpriteForKeyCode(Keybindings.LeftKey, AlphaSprites[0]);
        SecondChar.sprite = GetSpriteForKeyCode(Keybindings.DownKey, AlphaSprites[0]);
        ThirdChar.sprite = GetSpriteForKeyCode(Keybindings.UpKey, AlphaSprites[0]);
        FourthChar.sprite = GetSpriteForKeyCode(Keybindings.RightKey, AlphaSprites[0]);
    }

    // Update is called once per frame
    public void Update()
    {
        if (isMenuActive)
        {
            if (isRebinding)
            {
                FirstChar.sprite = GetSpriteForKeyCode(firstKeyPressed, AlphaSprites[0]);
                SecondChar.sprite = GetSpriteForKeyCode(secondKeyPressed, AlphaSprites[0]);
                ThirdChar.sprite = GetSpriteForKeyCode(thirdKeyPressed, AlphaSprites[0]);
                FourthChar.sprite = GetSpriteForKeyCode(fourthKeyPressed, AlphaSprites[0]);

                if (firstKeyPressed == KeyCode.None)
                {
                    SelectionUnderline.transform.position = FirstChar.transform.position + new Vector3(UnderlineOffset.x, -FirstChar.sprite.bounds.extents.y + UnderlineOffset.y, UnderlineOffset.z);
                    firstKeyPressed = GetPressedKey();
                }
                else if (secondKeyPressed == KeyCode.None)
                {
                    SelectionUnderline.transform.position = SecondChar.transform.position + new Vector3(UnderlineOffset.x, -SecondChar.sprite.bounds.extents.y + UnderlineOffset.y, UnderlineOffset.z);
                    secondKeyPressed = GetPressedKey();
                }
                else if (thirdKeyPressed == KeyCode.None)
                {
                    SelectionUnderline.transform.position = ThirdChar.transform.position + new Vector3(UnderlineOffset.x, -ThirdChar.sprite.bounds.extents.y + UnderlineOffset.y, UnderlineOffset.z);
                    thirdKeyPressed = GetPressedKey();
                }
                else if (fourthKeyPressed == KeyCode.None)
                {
                    SelectionUnderline.transform.position = FourthChar.transform.position + new Vector3(UnderlineOffset.x, -FourthChar.sprite.bounds.extents.y + UnderlineOffset.y, UnderlineOffset.z);
                    fourthKeyPressed = GetPressedKey();
                }
                else
                {
                    if (firstKeyPressed == secondKeyPressed ||
                        firstKeyPressed == thirdKeyPressed  ||
                        firstKeyPressed == fourthKeyPressed ||
                        secondKeyPressed == thirdKeyPressed ||
                        secondKeyPressed == fourthKeyPressed ||
                        thirdKeyPressed == fourthKeyPressed)
                    {
                        // One of the keys entered was invalid!
                        firstKeyPressed = KeyCode.None;
                        secondKeyPressed = KeyCode.None;
                        thirdKeyPressed = KeyCode.None;
                        fourthKeyPressed = KeyCode.None;
                    }
                    else
                    {
                        ExitRebind();
                    }
                }
            }
            else
            {
                // If a movement key was pressed, swap the 'active' one
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    activeButton = activeButton == MenuButtons.Rebind ? MenuButtons.Start : MenuButtons.Rebind;
                }

                // Position the arrow
                SpriteRenderer activeSprite = activeButton == MenuButtons.Rebind ? RebindButton : StartButton;
                SelectionArrow.transform.position = activeSprite.transform.position + new Vector3(-activeSprite.bounds.extents.x + ArrowOffset.x, ArrowOffset.y, ArrowOffset.z);

                // Handle 'Enter'
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    switch (activeButton)
                    {
                        case MenuButtons.Start:
                            MainMenu.SetActive(false);
                            SelectionArrow.SetActive(false);
                            PauseController.SetCanPause(true);
                            OnStart?.Invoke();
                            break;
                        case MenuButtons.Rebind:
                            EnterRebind();
                            break;

                    }
                }
            }
        }
    }

    private Sprite GetSpriteForKeyCode(KeyCode keyCode, Sprite defaultSprite)
    {
        int keyCodeIndex = (int)keyCode;
        keyCodeIndex = keyCodeIndex - EnumLetterStartIndex;
        if (keyCodeIndex < 0 || keyCodeIndex > EnumLetterEndIndex || keyCodeIndex >= AlphaSprites.Length)
        {
            return defaultSprite;
        }
        return AlphaSprites[keyCodeIndex];
    }

    private KeyCode GetPressedKey()
    {
        for (int i = 0; i <= ValidKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(ValidKeyCodes[i]))
            {
                return ValidKeyCodes[i];
            }
        }
        return KeyCode.None;
    }

    private void EnterRebind()
    {
        isRebinding = true;
        firstKeyPressed = KeyCode.None;
        secondKeyPressed = KeyCode.None;
        thirdKeyPressed = KeyCode.None;
        fourthKeyPressed = KeyCode.None;

        SelectionUnderline.gameObject.SetActive(true);

        FirstChar.gameObject.SetActive(true);
        SecondChar.gameObject.SetActive(true);
        ThirdChar.gameObject.SetActive(true);
        FourthChar.gameObject.SetActive(true);
    }

    private void ExitRebind()
    {
        isRebinding = false;

        SelectionUnderline.gameObject.SetActive(false);

        FirstChar.gameObject.SetActive(false);
        SecondChar.gameObject.SetActive(false);
        ThirdChar.gameObject.SetActive(false);
        FourthChar.gameObject.SetActive(false);

        Keybindings.LeftKey = firstKeyPressed;
        Keybindings.DownKey = secondKeyPressed;
        Keybindings.UpKey = thirdKeyPressed;
        Keybindings.RightKey = fourthKeyPressed;
    }
}
