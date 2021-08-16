using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        [SerializeField] ItemController IC = null;
        [SerializeField] MapController MC = null;
        [SerializeField] Text timeLeft = null;
        float time;

        // Start is called before the first frame update
        void Start()
        {
            MC.CreateMap(MC.mapArray = MC.MakeMapArr(), ref MC.mapObject);
            if (BoltNetwork.IsServer)
            {
                time = 600;
            }
        }

        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true);
        }

        public override void OnEvent(PlayerJoined evnt)
        {
            if(BoltNetwork.IsServer)
            {
                var reply = TimeAlert.Create();
                reply.TimeLeft = time;
                reply.Send();
            }            
        }

        public override void OnEvent(TimeAlert evnt)
        {
            time = evnt.TimeLeft; 
        }

        // Update is called once per frame
        void Update() 
        {
            time -= Time.deltaTime;
            timeLeft.text = ((int)Math.Floor(time)/60).ToString() + " : " + ((int)Math.Floor(time) % 60).ToString();
        }


    }
}

