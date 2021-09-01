﻿using System;
using UnityEngine;
using Photon.Bolt;
using TMPro;

namespace DoMine
{
    public class PlayerControl : EntityBehaviour<IPlayerState>
    {
        [SerializeField] GameObject player = null;
        [SerializeField] GameObject aimIndicator = null;
        [SerializeField] GameObject playerName = null;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;
        public float breakCool;
        float breakCoolBase = 0.5f;
        public float returnCool;
        float returnCoolBase = 0.5f;
        public bool canCreateWall = true;
        public Vector2 aim;
        int lookingAt = -1;//왼쪽부터 시계방향으로 0123

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
            gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
            if (entity.IsOwner)
            {
                aimIndicator = GameObject.Find("AimIndicator");
                state.Inventory[1] = 10;
                state.PlayerName = PlayerPrefs.GetString("nick");
            }
            playerName.GetComponent<TextMeshPro>().text = state.PlayerName;
            gameCtrl.GC.players.Add(entity);
        }
        void OnDestroy()
        {
            gameCtrl.GC.players.Remove(entity);
        }

        public override void SimulateOwner()
        {
            var speed = 4f;
            var movement = Vector3.zero;
            int output = -1;

            if (Input.GetKey(KeyCode.LeftArrow) == true)
            {
                movement.x -= 1f;
                lookingAt = 0;
            }
            if (Input.GetKey(KeyCode.RightArrow) == true)
            {
                movement.x += 1f;
                lookingAt = 2;
            }
            if (Input.GetKey(KeyCode.UpArrow) == true)
            {
                movement.y += 1f;
                lookingAt = 1;
            }
            if (Input.GetKey(KeyCode.DownArrow) == true)
            {
                movement.y -= 1f;
                lookingAt = 3;
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

            if (Input.GetKey(KeyCode.D) == true)
            {
                if (state.Inventory[1] > 0 && canCreateWall)
                {
                    output = mapCtrl.CreateWall(mapCtrl.mapObject, 2, (int)aim.x, (int)aim.y, false);
                    if(output == 0)
                        --state.Inventory[1];
                }
                else
                {
                    Debug.Log("barricade error");
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
            Aiming();
        }

        void Aiming()
        {
            int i;
            switch (lookingAt)
            {
                case 0:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x) - 1, (int)Math.Round(state.Location.Position.y));
                    break;
                case 1:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) + 1);
                    break;
                case 2:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x) + 1, (int)Math.Round(state.Location.Position.y));
                    break;
                case 3:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) - 1);
                    break;
            }
            aimIndicator.transform.position = aim;
            i = 0;
            foreach (BoltEntity player in gameCtrl.players)
            {
                if (Vector2.Distance(player.GetState<IPlayerState>().Location.Transform.position, aim) < 0.9)
                {
                    canCreateWall = false;
                }
                else
                    i++;
            }
            if(i == gameCtrl.playerNum)
            {
                canCreateWall = true;
            }
        }
    }
}

