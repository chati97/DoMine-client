using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt.Matchmaking;
using UnityEngine.UI;
using UdpKit;

namespace Photon.Bolt
{
    public class LobbyManager : Bolt.GlobalEventListener
    {
        public Text LogText;
        public InputField SessionInput;
        public InputField NickInput;

        public void StartServer()
        {
            if(NickInput.text =="" || SessionInput.text == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltLauncher.StartServer();
            
        }

        public override void BoltStartDone()
        {
            PlayerPrefs.SetString("nick", NickInput.text);
            BoltMatchmaking.CreateSession(sessionID:SessionInput.text, sceneToLoad:"Ingame");
        }

        public void StartClient()
        {
            if(NickInput.text =="")
            {
                Debug.LogError("Please check your input");
            }
            BoltLauncher.StartClient();
            
        }

        public void JoinSession()
        {
            if(NickInput.text =="" || SessionInput.text == "")
            {
                Debug.LogError("Please check your input");
            }
            BoltMatchmaking.JoinSession(SessionInput.text);
        }

        public override void SessionListUpdated(Map<System.Guid, UdpSession> sessionList)
        {
            string log = "";
            foreach (var session in sessionList)
            {
                UdpSession photonSession = session.Value;
                if (photonSession.Source == UdpSessionSource.Photon)
                    log += $"{photonSession.HostName}\n";
            }
            LogText.text = log;
        }
    }
}
