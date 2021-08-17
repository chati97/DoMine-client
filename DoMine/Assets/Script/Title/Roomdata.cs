using System;
using UnityEngine;
using UnityEngine.UI;
using UdpKit;

public class Roomdata : MonoBehaviour
{
    public Text roomName;
    public Text playerCount;
    public Button JoinButton;

    public void UpdateInfo(UdpSession session, Action click)
    {
        roomName.text = session.HostName;
        playerCount.text = string.Format("{0}/{1}", session.ConnectionsCurrent, session.ConnectionsMax);

        JoinButton.onClick.RemoveAllListeners();
        JoinButton.onClick.AddListener(click.Invoke);
    }
}
