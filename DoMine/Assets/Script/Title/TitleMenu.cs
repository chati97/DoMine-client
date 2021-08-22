using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UnityEngine.UI;
using UdpKit;

namespace Photon.Bolt
{
    public class TitleMenu : GlobalEventListener
    {
        public Text LogText;
        public InputField RoomInput;
        public InputField NameInput;
        public GameObject room;
        public Transform gridTr;

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
