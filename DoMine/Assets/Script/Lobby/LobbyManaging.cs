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
            var spawnPos = new Vector3(UnityEngine.Random.Range(100, 105), 300, 0);
            BoltNetwork.Instantiate(a, spawnPos, Quaternion.identity);
           // var evnt = PlayerConnectLobby.Create();
           // evnt.PlayerNick = PlayerPrefs.GetString("nick");
           // evnt.Send();
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

