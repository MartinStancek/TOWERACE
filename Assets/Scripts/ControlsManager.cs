using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputIcons
{
    public Sprite right;
    public Sprite left;
    public Sprite up;
    public Sprite down;

    public Sprite ready;
    public Sprite enter;
    public Sprite back;
}

public enum InputType
{
    right, left, up, down, ready, enter, back
}

public class ControlsManager : MonoBehaviour
{
    #region Singleton
    private static ControlsManager _instance;
    public static ControlsManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<ControlsManager>();
            }
            return _instance;

        }
    }
    #endregion
    public InputIcons keyboardIcon;
    public InputIcons gamepadIcons;

    public Sprite GetSprite(int playerIndex, InputType type)
    {
        Debug.Log("Player" + playerIndex + ", type" + type);
        if(playerIndex == -1)
        {
            playerIndex = 0;
        }
        var scheme = GameController.Instance.players[playerIndex].controlScheme;
        Debug.Log("Scheme is: " + scheme);
        switch (scheme)
        {
            case "Keyboard":
                return GetType(type, keyboardIcon);
            case "Controller":
                return GetType(type, gamepadIcons);
            default:
                return null;
        }
    }

    private Sprite GetType(InputType type, InputIcons icons)
    {
        switch (type)
        {
            case InputType.right:
                return icons.right;
            case InputType.left:
                return icons.left;
            case InputType.up:
                return icons.up;
            case InputType.down:
                return icons.down;
            case InputType.ready:
                return icons.ready;
            case InputType.enter:
                return icons.enter;
            case InputType.back:
                return icons.back;
            default:
                return null;
        }
    }
}
