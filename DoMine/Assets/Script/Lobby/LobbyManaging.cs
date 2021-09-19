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
        int playerdiscount;
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                Startbtn.SetActive(true);
                Readybtn.SetActive(false);
                playercount = 1;
                playerdiscount = 0;
            }
        }
        
        public override void Connected(BoltConnection connection)
        {
            playercount++;
            Debug.Log("player count is " + playercount);
        }
        public override void Disconnected(BoltConnection connection)
        {
            playercount--;
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
            
            BoltLauncher.Shutdown();
        }

        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
        {
            
            SceneManager.LoadScene("Title");
            
        }
    }
}

