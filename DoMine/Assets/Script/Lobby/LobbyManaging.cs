using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;
using TMPro;


namespace Photon.Bolt
{
    public class LobbyManaging : GlobalEventListener
    {
        // Start is called before the first frame update
        public GameObject Startbtn;
        public GameObject Readybtn;
        public Text PlayerNickList;
        public Text PlayerCountText;
        public BoltEntity a;
        public string[] playerNameList = { "", "", "", "", "", "", "", "", "", "" };
        int playercount = 1;
        int playercode;
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
            }
            
        }
        void Update()
        {

        }
        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            if(BoltNetwork.IsServer)
            {
                playerNameList[0] = PlayerPrefs.GetString("nick");
                playercode = 0;
                PlayerNickList.text = PlayerNickList.text +  playerNameList[playercode] + "     ";
            }
            if(BoltNetwork.IsClient)
            {
                var evnt = PlayerJoined.Create();
                evnt.PlayerName = PlayerPrefs.GetString("nick");
                evnt.Send();
            }
        }
        public override void OnEvent(PlayerName evnt)
        {
            int i = 0;
            if(BoltNetwork.IsClient)
            {
                playerNameList[evnt.Code] = evnt.Name;
                if(evnt.Name == PlayerPrefs.GetString("nick"))
                {
                    playercode = evnt.Code;
                    foreach (string name in playerNameList)
                    {
                        PlayerNickList.text = PlayerNickList.text + playerNameList[i] + "     ";
                        i++;
                    }
                }
                
            }
        }
        public override void OnEvent(PlayerJoined evnt)
        {
            if(BoltNetwork.IsServer)
            {
                int i = 0;
                playerNameList[playercount] = evnt.PlayerName;
                PlayerNickList.text = PlayerNickList.text + evnt.PlayerName + "     ";
                playercount++;
                foreach(string name in playerNameList)
                {
                    var evnt2 = PlayerName.Create();
                    evnt2.Name = playerNameList[i];
                    //PlayerNickList.text = PlayerNickList.text + evnt2.Name;
                    evnt2.Code = i;
                    evnt2.Send();
                    i++;
                }
            }
        }
        public override void OnEvent(PlayerConnectLobby evnt)
        {
            if(BoltNetwork.IsClient)
            {
                var evnt2 = PlayerJoined.Create();
                evnt2.PlayerName = PlayerPrefs.GetString("nick");
                //PlayerNickList.text = PlayerNickList.text + evnt2.PlayerName + "     ";
                evnt2.Send();
            }
        }
        public override void Disconnected(BoltConnection connection)
        {
            if(BoltNetwork.IsServer)
            {
                playercount = 1;
                string[] tmparray = { "", "", "", "", "", "", "", "", "", "" };
                playerNameList = tmparray;
                PlayerNickList.text = "";
                playerNameList[0] = PlayerPrefs.GetString("nick");
                playercode = 0;
                PlayerNickList.text = PlayerNickList.text + playerNameList[playercode] + "     ";
                var evnt = PlayerConnectLobby.Create();
                evnt.Send();
            }
            /*if(BoltNetwork.IsClient)
            {
                Debug.LogError("Client Leave Game");
                var evnt = PlayerCode.Create();
                evnt.Name = PlayerPrefs.GetString("nick");
                evnt.Code = playercode;
                evnt.Send();
                var evnt = PlayerJoined.Create();
                evnt.PlayerName = PlayerPrefs.GetString("nick");
                evnt.Send();
            }*/
        }
        public void LoadGame()
        {
            //if(playercount >= 3)
            {
                BoltNetwork.LoadScene("Ingame");
            }
             
        }
        public void LeaveGame()
        {
            
            BoltLauncher.Shutdown();
        }

        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
        {
            SceneManager.LoadScene("Title");
            
        }
    }
}

