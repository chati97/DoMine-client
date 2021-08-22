using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;

public class LobbyManaging : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Startbtn;
    public GameObject Readybtn;
    void Start()
    {
        if(BoltNetwork.IsServer)
        {
            Startbtn.SetActive(true);
            Readybtn.SetActive(false);
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Ingame");
    }
}
