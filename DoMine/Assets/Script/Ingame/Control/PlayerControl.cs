using System;
using UnityEngine;
using Photon.Bolt;


namespace DoMine
{
    public class PlayerControl : EntityBehaviour<IPlayerState>
    {
        [SerializeField] GameObject player = null;
        [SerializeField] GameObject aimIndicator = null;
        public float breakCool;
        float breakCoolBase = 0.5f;
        public float returnCool;
        float returnCoolBase = 0.5f;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;
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
            aimIndicator = GameObject.Find("AimIndicator");
            state.Inventory[1] = 10;
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
                if (state.Inventory[1] > 0)
                {
                    switch (lookingAt)
                    {
                        case 0:
                            output = mapCtrl.CreateWall(mapCtrl.mapObject, 2, (int)Math.Round(state.Location.Position.x) - 1, (int)Math.Round(state.Location.Position.y), false);
                            break;
                        case 1:
                            output = mapCtrl.CreateWall(mapCtrl.mapObject, 2, (int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) + 1, false);
                            break;
                        case 2:
                            output = mapCtrl.CreateWall(mapCtrl.mapObject, 2, (int)Math.Round(state.Location.Position.x) + 1, (int)Math.Round(state.Location.Position.y), false);
                            break;
                        case 3:
                            output = mapCtrl.CreateWall(mapCtrl.mapObject, 2, (int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) - 1, false);
                            break;
                    }
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
            switch (lookingAt)
            {
                case 0:
                    aimIndicator.transform.position = new Vector2((int)Math.Round(state.Location.Position.x) - 1, (int)Math.Round(state.Location.Position.y));
                    break;
                case 1:
                    aimIndicator.transform.position = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) + 1);
                    break;
                case 2:
                    aimIndicator.transform.position = new Vector2((int)Math.Round(state.Location.Position.x) + 1, (int)Math.Round(state.Location.Position.y));
                    break;
                case 3:
                    aimIndicator.transform.position = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y) - 1);
                    break;
            }
        }
    }
}

