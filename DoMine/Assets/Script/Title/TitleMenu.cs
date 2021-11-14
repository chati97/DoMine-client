using System;
using System.Collections;
using System.Collections.Generic;
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
        public GameObject hostLoading;
        public GameObject loadingPanel;
        public GameObject namePanel;
        public GameObject roomNameError;
        public GameObject nicknameError;
        public GameObject EnterRoomError;
        public Button NoRoomName;
        public Button NoNickName;
        public Button Apply;
        public Button Host;
        public Transform gridTr;
        public OptionSetting op;
        public Action click;
        public Text loading;
        public Text playerName;

        public void Start()
        {
            playerName.text = PlayerPrefs.GetString("nick");
            if(PlayerPrefs.GetString("nick") != "")
            {
                namePanel.gameObject.SetActive(false);
            }
            NoRoomName.onClick.AddListener(onClickNoRoom);
            NoNickName.onClick.AddListener(onClickNoNick);
        }

        private void onClickNoNick()
        {
            Apply.GetComponent<Button>().interactable = true;
        }

        private void onClickNoRoom()
        {
            Host.GetComponent<Button>().interactable = true;
        }

        public void StartServer()
        {
            if (RoomInput.text == "" || PlayerPrefs.GetString("nick") == "")
            {
                roomNameError.gameObject.SetActive(true);
                Host.GetComponent<Button>().interactable = false;
                Debug.LogError("Please check your input");
                return;
            }
            BoltLauncher.StartServer();
            hostLoading.gameObject.SetActive(true);
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
            if (PlayerPrefs.GetString("nick") == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltLauncher.StartClient();

        }

        public void JoinSession()
        {
            if (PlayerPrefs.GetString("nick") == "" || RoomInput.text == "")
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
                roomData.playerCount.text = string.Format("{0}/{1}", roomData.currentPlayer, roomData.maxPlayer);
                //roomData.UpdateInfo(photonSession, click);
                Button join = roomData.GetComponentInChildren<Button>();
                join.onClick.AddListener(() => OnClickRoom(roomData.roomName.text));
            }
            
        }

        public void ApplyName()
        {
            if(NameInput.text == "") 
            { 
                nicknameError.gameObject.SetActive(true);
                Apply.GetComponent<Button>().interactable = false;
            }
            else
            {            
                playerName.text = NameInput.text;
                PlayerPrefs.SetString("nick", playerName.text);
                namePanel.gameObject.SetActive(false);
            }
            
        }

        public void OnClickRoom(string roomName)
        {
            loadingPanel.gameObject.SetActive(true);
            StartClient();
            BoltMatchmaking.JoinSession(roomName);
            //PlayerPrefs.SetString("nick", NameInput.text);

        }
        public void LeaveGame()
        {
            BoltLauncher.Shutdown();
        }
        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
        {
            loadingPanel.gameObject.SetActive(false);
            EnterRoomError.gameObject.SetActive(true);
            // 게임 이미 시작했다고 메시지 날리기
            //SceneManager.LoadScene("Title");
        }
    }
}