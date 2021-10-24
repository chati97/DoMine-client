using System;
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
        BoltEntity targetPlayer = null;
        Light playerView = null;

        public static float paralyzeCool;
        public static float paralyzeCoolBase = 5f;
        public static float blindCool;
        public static float blindCoolBase = 10f;
        public float breakCool;
        float breakCoolBase = 0.5f;
        public float returnCool;
        float returnCoolBase = 60f;
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
            gameCtrl.players.Add(entity);
            if (entity.IsOwner)
            {
                aimIndicator = GameObject.Find("AimIndicator");
                state.PlayerName = PlayerPrefs.GetString("nick");
                playerView = entity.GetComponentInChildren<Light>();
                if (BoltNetwork.IsClient)
                {
                    var evnt = PlayerJoined.Create();
                    evnt.PlayerName = state.PlayerName;
                    evnt.Send();
                }
            }
            playerName.GetComponent<TextMeshPro>().text = state.PlayerName;
        }
        void OnDestroy()
        {
            gameCtrl.players.Remove(entity);
        }

        public override void SimulateOwner()
        {
            var speed = 4f;
            var movement = Vector3.zero;
            int output = -1;
            if(!state.Paralyzed)
            {
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
            }


            if (movement != Vector3.zero)
            {
                transform.position = transform.position + (movement.normalized * speed * BoltNetwork.FrameDeltaTime);
            }

            if (Input.GetKey(KeyCode.S) == true)
            {
                if (state.Inventory[2] > 0 && canCreateWall)
                {
                    output = mapCtrl.CreateWall(1, (int)aim.x, (int)aim.y, false);
                    if(output == 0)
                        --state.Inventory[2];
                }
                else
                {
                    Debug.Log("barricade error");
                }
            }

            if (Input.GetKey(KeyCode.A) == true)
            {
                if (breakCool == 0 && mapCtrl.nearestWall != null)
                {
                    if (Vector2.Distance(player.transform.position, mapCtrl.nearestWall.transform.position) < 0.8 && state.Inventory[0] > 0)
                    {
                        mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY, false, false, -1);
                        breakCool = breakCoolBase;
                        state.Inventory[0]--;//곡괭이 갯수 소진
                    }
                }
                else
                {
                    //Debug.Log("in Breaking-Cooltime");
                }

            }

            if (Input.GetKeyUp(KeyCode.D) == true)
            {
                //itemCtrl.Init(gameCtrl.playerNum);
            }

                if (Input.GetKey(KeyCode.Q) == true)
            {
                if(targetPlayer != null)
                {
                    Debug.LogWarning("AmingPlayer : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 0;
                    evnt.Send();
                }
            }
            if (Input.GetKey(KeyCode.W) == true)
            {
                if (targetPlayer != null)
                {
                    Debug.LogWarning("AmingPlayer : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 1;
                    evnt.Send();
                }
            }
            if (Input.GetKey(KeyCode.E) == true)
            {
                if (targetPlayer != null)
                {
                    Debug.LogWarning("AmingPlayer : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 2;
                    evnt.Send();
                }
            }

            if (itemCtrl.nearestItem != null)
            {
                if (Vector2.Distance(player.transform.position, itemCtrl.nearestItem.transform.position) < 0.5)
                {
                    itemCtrl.GetItem(itemCtrl.nearestItemX, itemCtrl.nearestItemY, state, false);
                }
            }
        }
        
        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (breakCool > 0)
            {
                breakCool -= Time.deltaTime;
            }//쿨타임관련 코드
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

            if(entity.IsOwner)
            {
                if (blindCool > 0)
                {
                    blindCool -= Time.deltaTime;
                }
                else
                {
                    blindCool = 0;
                    state.Blinded = false;
                }
                if (paralyzeCool > 0)
                {
                    paralyzeCool -= Time.deltaTime;
                }
                else
                {
                    paralyzeCool = 0;
                    state.Paralyzed = false;
                }
                if (state.Blinded == true)
                {
                    playerView.spotAngle = 10;
                }
                else
                {
                    playerView.spotAngle = 50;
                }

                mapCtrl.FindWall(mapCtrl.mapObject);
                mapCtrl.FindChest();
                itemCtrl.FindItem(itemCtrl.itemObject);
                Aiming();
                if (Vector2.Distance(entity.transform.position, new Vector2(49.5f,49.5f)) < 1)
                {
                    if(state.Inventory[0] < 15)
                    {
                        state.Inventory[0] = 15;
                        Debug.LogWarning("Pickaxe Recharged");//중앙 안전지대 이동시 곡괭이 회복
                    }
                    if (state.Inventory[1] == 1 && gameCtrl.playerList[GameController.playerCode] == 0)
                    {
                        var evnt = SaveGold.Create();
                        evnt.Player = GameController.playerCode;//금을 들고 중앙으로 이동시 금 입금
                        evnt.Send();
                    }
                    
                }
            }
        }

        void Aiming()//조준방향에 대한 함수 현재는 바리케이트 설치에만 관련, 유저 조준하는 기능도 추후 추가
        {
            int i;
            
            switch (lookingAt)//최초엔 보는방향을 기준으로 에임을 둠
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
            
            i = 0;
            BoltEntity nearestPlayer = null;
            foreach (BoltEntity player in gameCtrl.players)
            {
                if (Vector2.Distance(player.GetState<IPlayerState>().Location.Transform.position, aim) < 0.8)//유저위에 벽못깔게 하는코드, 유저가 가까이있을시 유저를 조준하도록
                {
                    if (player == gameCtrl.myPlayer)
                    {
                        //본인이면 스킵
                    }
                    else if(nearestPlayer == null)
                    {
                        nearestPlayer = player;//처음 걸린유저
                    }
                    else if(Vector2.Distance(player.GetState<IPlayerState>().Location.Transform.position, aim)< Vector2.Distance(nearestPlayer.GetState<IPlayerState>().Location.Transform.position, aim))
                    {
                        nearestPlayer = player;//만약 에임방향에 더 가까운 유저가 있을 시엔 바꿈
                    }
                }
                else
                    i++;
            }

            if(i == gameCtrl.players.Count && Vector2.Distance(aim, new Vector2(49.5f, 49.5f)) > 5) // 중앙선에서 5칸 이내일시
            {
                canCreateWall = true;
            }
            else
            {
                canCreateWall = false;
            }

            if (nearestPlayer != null) // 가까운타겟플레이어 조정
            {
                targetPlayer = nearestPlayer;
            }
            else
            {
                targetPlayer = null;
            }

            if(targetPlayer !=null) // 조준점위치 옮기기
            {
                aimIndicator.transform.position = targetPlayer.GetState<IPlayerState>().Location.Transform.position;
            }
            else
            {
                aimIndicator.transform.position = aim;
            }
        }
    }
}

