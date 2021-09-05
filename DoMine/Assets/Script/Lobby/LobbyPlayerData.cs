using System;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerData : MonoBehaviour
{
    public Text PlayerName;
    public Button KickButton;

    public void KickButtonSetting()
    {
        if (BoltNetwork.IsServer)
        {
            KickButton.gameObject.SetActive(true);
        }
        else
        {
            KickButton.gameObject.SetActive(false);
        }
    }
}
