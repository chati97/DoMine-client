using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;


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
        public List<BoltEntity> playername = new List<BoltEntity>();
        public string[] playerNameList = { "", "", "", "", "", "", "", "", "", "" };
        int playercount = 1;
        int count = 0;
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
            }
        }
        public override void OnEvent(PlayerConnectLobby evnt)
        {
            if(BoltNetwork.IsServer)
            {
                playerNameList[count] = evnt.PlayerNick;
                Debug.LogError(evnt.PlayerNick);
                Debug.LogError("test");
                
                
                PlayerNickList.text += playerNameList[count];
                count++;
            }
            if(BoltNetwork.IsClient)
            {
                
            }
        }
        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var evnt = PlayerConnectLobby.Create();
            evnt.PlayerNick = PlayerPrefs.GetString("nick");
            evnt.Send();
        }
        
        void Update()
        {
            PlayerCountText.text = "Player : " + count + "/ 10";
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

