using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSetting : MonoBehaviour
{
    public Text playerName;
    public InputField Nickname;

    public void ApplyOption()
    {
        playerName.text = Nickname.text;
    }
}
