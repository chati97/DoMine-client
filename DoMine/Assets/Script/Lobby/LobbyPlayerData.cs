using System;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerData : MonoBehaviour
{
    public Text PlayerName;
    public Button KickButton;
    public bool Ready;
    public Button IsReady;

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

    public void OnClickReady()
    {
        if (Ready)
        {
            IsReady.interactable = true;
        }

        else
        {
            IsReady.interactable = false;
        }
    }

}
