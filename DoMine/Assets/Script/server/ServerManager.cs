using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

namespace Photon.Bolt
{
    public class ServerManager : GlobalEventListener
    {
        public static ServerManager NM;
        private void Awake() => NM = this;


        public List<BoltEntity> players = new List<BoltEntity>();
        public BoltEntity myPlayer;

        public GameObject SpawnPrefab;


        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector2(Random.Range(50, 52), 50);
            myPlayer = BoltNetwork.Instantiate(SpawnPrefab, spawnPos, Quaternion.identity);
        }

    }
}