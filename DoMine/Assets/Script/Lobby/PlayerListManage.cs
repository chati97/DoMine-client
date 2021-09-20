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
    public Button ready;
    private List<LobbyPlayerData> _players = new List<LobbyPlayerData>();

    public void AddPlayer(LobbyPlayerData player)
    {
        if (player == null) 
        { 
            return; 
        }

        if (_players.Contains(player))
        {
            return;
        }

        _players.Add(player);
        player.transform.SetParent(lobbyGrid, false);
    }

    public void RemovePlayer(LobbyPlayerData player)
    {
        if (player == null) { return; }

        if (_players.Contains(player))
        {
            _players.Remove(player);
        }
    }

    public void SetPlayer()
    {
        GameObject _player = Instantiate(Player, lobbyGrid);
        LobbyPlayerData lobbyPlayer = _player.GetComponent<LobbyPlayerData>();
        lobbyPlayer.PlayerName = Nickname;
        lobbyPlayer.Ready = false;
    }

    public void EntityAttachedEventHandler(BoltEntity entity)
    {
        var player = entity.gameObject.GetComponent<LobbyPlayerData>();
        AddPlayer(player);
    }

    public void EntityDetachedEventHandler(BoltEntity entity)
    {
        var player = entity.gameObject.GetComponent<LobbyPlayerData>();
        RemovePlayer(player);
    }

    public override void EntityAttached(BoltEntity entity)
    {
        EntityAttachedEventHandler(entity);

        var photonPlayer = entity.GetComponent<LobbyPlayerData>();
        if(photonPlayer)
        {
            SetPlayer();
        }
  
    }

    public override void EntityDetached(BoltEntity entity)
    {
        EntityDetachedEventHandler(entity);
    }
}

