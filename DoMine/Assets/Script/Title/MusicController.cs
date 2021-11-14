using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public GameObject BGM;
    AudioSource mainBGM;
    void Awake()
    {
        mainBGM = BGM.gameObject.GetComponent<AudioSource>();
        //if(!mainBGM.isPlaying)
            //DontDestroyOnLoad(mainBGM);
    }
    public void MusicOn()
    {
        mainBGM.Play();
    }
    public void MusicOff()
    {
        mainBGM.Pause();
    }
}
