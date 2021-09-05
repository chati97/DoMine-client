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
        public Text PlayerList;
        int playercount;
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
                playercount = 1;
            }
            
        }
        public override void Connected(BoltConnection connection)
        {
            playercount++;
            Debug.Log("playercount : " + playercount);
        }
        public void LoadGame()
        {
            if(playercount < 4)
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
            playercount--;
            Debug.Log("DisConnected & playercount : " + playercount);
            BoltLauncher.Shutdown();
        }

        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
        {
            
            SceneManager.LoadScene("Title");
            
        }
    }
}

