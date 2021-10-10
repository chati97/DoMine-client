using System;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UnityEngine.UI;
using UdpKit;
using UnityEngine.SceneManagement;

namespace Photon.Bolt
{
    public class TitleMenu : GlobalEventListener
    {
        public Text LogText;
        public InputField RoomInput;
        public InputField NameInput;
        public GameObject room;
        public GameObject loadingPanel;
        public Transform gridTr;
        public OptionSetting op;
        public Action click;
        public Text loading;
        public Text playerName;

        public void Start()
        {
            playerName.text = PlayerPrefs.GetString("nick");
        }

        public void StartServer()
        {
            if (RoomInput.text == "" || PlayerPrefs.GetString("nick") == "")
            {
                Debug.LogError("Please check your input");
                return;
            }
            BoltLauncher.StartServer();
        }

        public override void BoltStartDone()
        {
            if (BoltNetwork.IsServer)
            {
                BoltMatchmaking.CreateSession(sessionID: RoomInput.text, sceneToLoad: "Lobby");
            }
            
        }

        

        public void StartClient()
        {
            if (NameInput.text == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltLauncher.StartClient();

        }

        public void JoinSession()
        {
            if (NameInput.text == "" || RoomInput.text == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltMatchmaking.JoinSession(RoomInput.text);
        }

        public override void SessionListUpdated(Map<System.Guid, UdpSession> sessionList)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOM"))
            {
                Destroy(obj);
            }

            foreach (var session in sessionList)
            {
                UdpSession photonSession = session.Value;
                GameObject _room = Instantiate(room, gridTr);
                loading.gameObject.SetActive(false);
                Roomdata roomData = _room.GetComponent<Roomdata>();
                roomData.roomName.text = photonSession.HostName;
                roomData.maxPlayer = photonSession.ConnectionsMax;
                roomData.currentPlayer = photonSession.ConnectionsCurrent;
                //roomData.UpdateInfo(photonSession, click);
                Button join = roomData.GetComponentInChildren<Button>();
                join.onClick.AddListener(() => OnClickRoom(roomData.roomName.text));
            }
            
        }

        public void ApplyName()
        {
            playerName.text = NameInput.text;
            PlayerPrefs.SetString("nick", playerName.text);
        }

        public void OnClickRoom(string roomName)
        {
            loadingPanel.gameObject.SetActive(true);
            BoltMatchmaking.JoinSession(roomName);
            //PlayerPrefs.SetString("nick", NameInput.text);
        }
        public void LeaveGame()
        {
            BoltLauncher.Shutdown();
        }
    }
}