using System;
using UnityEngine;
using System.Collections;
using Photon.Bolt;

[BoltGlobalBehaviour]
public class NetworkCallbacks : Photon.Bolt.GlobalEventListener
{
    public void SceneLoadLocalDone(string map)
    {
    // randomize a position
        var pos = new Vector3(UnityEngine.Random.Range(-16, 16), 0, UnityEngine.Random.Range(-16, 16));

    // instantiate cube
        BoltNetwork.Instantiate(BoltPrefabs.Player, pos, Quaternion.identity);
    }
}