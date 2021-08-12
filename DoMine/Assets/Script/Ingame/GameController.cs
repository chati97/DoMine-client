using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

namespace DoMine
{
    public class GameController : EntityBehaviour<IGameInfo>
    {
        [SerializeField] GameObject player = null;
        [SerializeField] ItemController itemcontroller = null;
        [SerializeField] MapController MC = null;
        [SerializeField] Text timeLeft = null;
        public float time;

        // Start is called before the first frame update
        void Start()
        {
            //if (BoltNetwork.IsServer)
            //{
                //state.TimeLeft = 900;
            //}

        }


        // Update is called once per frame
        void Update()
        {
            //time = state.TimeLeft;
            //if (BoltNetwork.IsServer && time > 0)
            //    state.TimeLeft -= Time.deltaTime;
            //timeLeft.text = time.ToString();
            
        }


    }
}

