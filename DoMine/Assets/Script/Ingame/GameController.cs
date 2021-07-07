﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace DoMine
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] GameObject player = null;
        [SerializeField] ItemController itemcontroller = null;
        public Player playerInfo = new Player();
        public bool gold = true;
        public float time;

        // Start is called before the first frame update
        void Start()
        {
            time = 900;
            itemcontroller.Init(playerInfo);
            itemcontroller.CreateItem(50, 98, 0);
            itemcontroller.CreateItem(49, 49, 0);
            MovePlayer(player, new Vector2(50, 50));
        }


        // Update is called once per frame
        void Update()
        {
            gold = playerInfo.inventory.gold;
            playerInfo.x_location = player.transform.position.x;
            playerInfo.y_location = player.transform.position.y;
            if(time > 0)
                time -= Time.deltaTime;
        }

        public void MovePlayer(GameObject player, Vector2 location)
        {
            player.transform.position = location;
        }

    }
}

