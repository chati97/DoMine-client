using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
namespace DoMine
{
    public class ServerManager : GlobalEventListener
    {
        public static ServerManager NM;
        [SerializeField] MapController MC;
        private void Awake() => NM = this;
        public List<BoltEntity> players = new List<BoltEntity>();
        public BoltEntity myPlayer;
        public BoltEntity gameInfo;
        
        public GameObject SpawnPrefab;

        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector3(Random.Range(49, 50), Random.Range(49, 50), 0);
            myPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPos, Quaternion.identity);
            myPlayer.TakeControl();
            MC.player = myPlayer;//Mcø° ≥÷¿Ω
            if(BoltNetwork.IsClient)
            {
                var evnt = PlayerJoined.Create();
                evnt.Send();
            }
        }
    }
}
