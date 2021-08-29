using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSetting : MonoBehaviour
{
    public Text playerName;
    public InputField Nickname;

    public void Start()
    {
        playerName.text = PlayerPrefs.GetString("nick");
    }

    public void ApplyOption()
    {
        playerName.text = Nickname.text;
        PlayerPrefs.SetString("nick", playerName.text);
    }
}
