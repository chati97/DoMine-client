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
        public Transform gridTr;
        public OptionSetting op;
        public Action click;
        public void StartServer()
        {
            if (NameInput.text == "" || RoomInput.text == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltLauncher.StartServer();
        }

        public override void BoltStartDone()
        {
            //PlayerPrefs.SetString("nick", NameInput.text);
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
                Roomdata roomData = _room.GetComponent<Roomdata>();
                roomData.roomName.text = photonSession.HostName;
                roomData.maxPlayer = photonSession.ConnectionsMax;
                roomData.currentPlayer = photonSession.ConnectionsCurrent;
                //roomData.UpdateInfo(photonSession, click);
                Button join = roomData.GetComponentInChildren<Button>();
                join.onClick.AddListener(() => OnClickRoom(roomData.roomName.text));
            }
            
            /*string log = "";
            foreach (var session in sessionList)
            {
                UdpSession photonSession = session.Value;
                if (photonSession.Source == UdpSessionSource.Photon)
                    log += $"{photonSession.HostName}\n";
            }
            LogText.text = log;*/
        }
        public void OnClickRoom(string roomName)
        {
            BoltMatchmaking.JoinSession(roomName);
            //PlayerPrefs.SetString("nick", NameInput.text);
        }
        public void LeaveGame()
        {
            BoltLauncher.Shutdown();
        }
    }
}