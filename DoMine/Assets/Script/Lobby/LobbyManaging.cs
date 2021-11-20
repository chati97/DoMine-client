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
            Application.runInBackground = true;
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
            }
            
        }
        void Update()
        {

        }
        public void ListOut()
        {
            int i = 0;
            PlayerNickList.text = "";
            foreach (string name in playerNameList)
            {
                PlayerNickList.text = PlayerNickList.text + playerNameList[i] + "     ";
                i++;
            }
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
            if(BoltNetwork.IsClient)
            {
                //int i = 0;
                playerNameList[evnt.Code] = evnt.Name;
                /*foreach(string name in playerNameList)
                {
                    if(playerNameList[i] != "" && playerNameList[i] == evnt.Name && i < evnt.Code)
                    {
                        evnt.Name += "0";
                        playerNameList[evnt.Code] = evnt.Name;
                        break;
                    }
                    i++;
                }*/
                if(evnt.Name == PlayerPrefs.GetString("nick"))
                {
                    playercode = evnt.Code;
                }
                
            }
            ListOut();
        }
        public override void OnEvent(PlayerJoined evnt)
        {
            if(BoltNetwork.IsServer)
            {
                int i = 0;
                int j = 0;
                playerNameList[playercount] = evnt.PlayerName;
                foreach(string name in playerNameList)
                {
                    if(playerNameList[j] != "" && playerNameList[j] == evnt.PlayerName && j < playercount)
                    {
                        evnt.PlayerName += playercount;
                        playerNameList[playercount] = evnt.PlayerName;
                        break;
                    }
                    j++;
                }
                playercount++;
                ListOut();
                foreach(string name in playerNameList)
                {
                    var evnt2 = PlayerName.Create();
                    evnt2.Name = playerNameList[i];
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
                playerNameList[0] = PlayerPrefs.GetString("nick");
                playercode = 0;
                var evnt = PlayerConnectLobby.Create();
                evnt.Send();
                ListOut();
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

