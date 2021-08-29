using System;
using UnityEngine;
using UnityEngine.UI;
using UdpKit;

public class Roomdata : MonoBehaviour
{
    public Text roomName;
    public int currentPlayer = 0;
    public int maxPlayer = 0;
    public Text playerCount;
    public Button JoinButton;

    public void UpdateInfo(UdpSession session, Action click)
    {
        roomName.text = session.HostName;
        currentPlayer = session.ConnectionsCurrent;
        maxPlayer = session.ConnectionsMax;
        playerCount.text = string.Format("{0}/{1}", currentPlayer, maxPlayer);

        JoinButton.onClick.RemoveAllListeners();
        JoinButton.onClick.AddListener(click.Invoke);
    }

}
