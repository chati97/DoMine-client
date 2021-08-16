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
        
        public GameObject SpawnPrefab;

        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector3(Random.Range(50, 52), 50, 0);
            myPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPos, Quaternion.identity);
            myPlayer.TakeControl();
            MC.player = myPlayer;
        }
    }
}
