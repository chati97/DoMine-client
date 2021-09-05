using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListManage : GlobalEventListener
{
    public GameObject Player;
    public Transform lobbyGrid;
    public Text Nickname;
    private List<LobbyPlayerData> _players = new List<LobbyPlayerData>();

    public void AddPlayer()
    {
        foreach (var player in _players)
        {
            GameObject _player = Instantiate(Player, lobbyGrid);
            LobbyPlayerData lobbyPlayer = _player.GetComponent<LobbyPlayerData>();
            lobbyPlayer.PlayerName = Nickname;
            Button kick = lobbyPlayer.GetComponentInChildren<Button>();
            kick.onClick.AddListener(() => onClickKick());
        }
    }

    public void onClickKick()
    {

    }
}

