﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;


namespace DoMine
{
    public class PlayerControl : EntityBehaviour<IPlayerState>
    {
        [SerializeField] GameObject player = null;
        public float breakCool;
        float breakCoolBase = 0.5f;
        public float returnCool;
        float returnCoolBase = 0.5f;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;

        public void MovePlayer(GameObject player, Vector2 location)
        {
            player.transform.position = location;
        }

        public override void Attached()
        {
            state.SetTransforms(state.Location, transform);
        }

        // Start is called before the first frame update
        void Start()
        {
            mapCtrl = GameObject.Find("GameController").GetComponent<MapController>();
            itemCtrl = GameObject.Find("GameController").GetComponent<ItemController>();
        }

        public override void SimulateOwner()
        {
            var speed = 4f;
            var movement = Vector3.zero;

            if (Input.GetKey(KeyCode.LeftArrow) == true)
            {
                movement.x -= 1f;
            }
            if (Input.GetKey(KeyCode.RightArrow) == true)
            {
                movement.x += 1f;
            }
            if (Input.GetKey(KeyCode.UpArrow) == true)
            {
                movement.y += 1f;
            }
            if (Input.GetKey(KeyCode.DownArrow) == true)
            {
                movement.y -= 1f;
            }
            
            if (movement != Vector3.zero)
            {
                transform.position = transform.position + (movement.normalized * speed * BoltNetwork.FrameDeltaTime);
            }

            if (Input.GetKey(KeyCode.R) == true)
            {
                if (returnCool == 0)
                {
                    MovePlayer(player, new Vector2(50, 50));
                    returnCool = returnCoolBase;
                }
                else
                {
                    //Debug.Log("in Return-Cooltime");
                }

            }

            if (Input.GetKey(KeyCode.A) == true)
            {
                if (breakCool == 0)
                {
                    if (Vector2.Distance(player.transform.position, mapCtrl.nearestWall.transform.position) < 0.8)
                    {
                        mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY, false);
                        breakCool = breakCoolBase;
                    }
                }
                else
                {
                    //Debug.Log("in Breaking-Cooltime");
                }

            }
            
            if (Input.GetKey(KeyCode.S) == true)
            {
                if (itemCtrl.nearestItem != null)
                {
                    if (Vector2.Distance(player.transform.position, itemCtrl.nearestItem.transform.position) < 0.5)
                    {
                        itemCtrl.GetItem(itemCtrl.nearestItemX, itemCtrl.nearestItemY, state, false);
                    }
                }
            }
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (breakCool > 0)
            {
                breakCool -= Time.deltaTime;
            }
            if (breakCool < 0)
            {
                breakCool = 0;
            }
            if (returnCool > 0)
            {
                returnCool -= Time.deltaTime;
            }
            if (returnCool < 0)
            {
                returnCool = 0;
            }
            mapCtrl.FindWall(mapCtrl.mapObject);
            itemCtrl.FindItem(itemCtrl.itemObject);
        }
    }
}

