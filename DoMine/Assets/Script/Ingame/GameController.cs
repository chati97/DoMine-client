using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        [SerializeField] GameObject player = null;
        [SerializeField] ItemController itemcontroller = null;
        [SerializeField] MapController MC = null;
        [SerializeField] Text timeLeft = null;
        public float time;

        // Start is called before the first frame update
        void Start()
        {
            MC.CreateMap(MC.mapArray = MC.MakeMapArr(), ref MC.mapObject);
        }

        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true);
        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}

