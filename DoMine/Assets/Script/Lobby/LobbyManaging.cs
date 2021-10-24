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
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
            }
            
        }
        /*public override void OnEvent(PlayerConnectLobby evnt)
        {
            var spawnPos = new Vector3(UnityEngine.Random.Range(100, 105), 300, 0);
            //a.GetComponent<TextMeshPro>().text = evnt.PlayerNick;
            BoltNetwork.Instantiate(a, spawnPos, Quaternion.identity);
            Debug.LogError(a.GetComponent<TextMeshPro>().text);
        }*/
        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            if(BoltNetwork.IsServer)
            {
                playerNameList[0] = PlayerPrefs.GetString("nick");
            }
            if(BoltNetwork.IsClient)
            {
                var evnt = PlayerJoined.Create();
                evnt.PlayerName = PlayerPrefs.GetString("nick");
                evnt.Send();
            }
            //var spawnPos = new Vector3(UnityEngine.Random.Range(100, 105), 300, 0);
            //BoltNetwork.Instantiate(a, spawnPos, Quaternion.identity);
           // var evnt = PlayerConnectLobby.Create();
           // evnt.PlayerNick = PlayerPrefs.GetString("nick");
           // evnt.Send();
        }
        public override void OnEvent(PlayerName evnt)
        {
            if(BoltNetwork.IsClient)
            {
                playerNameList[evnt.Code] = evnt.Name;
            }
        }
        public override void OnEvent(PlayerJoined evnt)
        {
            if(BoltNetwork.IsServer)
            {
                int i = 0;
                playerNameList[playercount] = evnt.PlayerName;
                playercount++;
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
        public override void Connected(BoltConnection connection)
        {
            if(BoltNetwork.IsServer)
            {
                playercount++;
            }
            var evnt = PlayerJoined.Create();
        }
        public override void Disconnected(BoltConnection connection)
        {
            playercount--;
        }
        public void LoadGame()
        {
            if(playercount < 1)
            {
                Debug.Log("Can't Start");
            }
            else
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

