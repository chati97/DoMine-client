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
        public static bool canFindSabotage = true;
        public static float paralyzeCool;
        public static float paralyzeCoolBase = 10f;
        public static float blindCool;
        public static float blindCoolBase = 30f;
        public float windWalkCool;
        public float windWalkDuration;
        float windWalkDurationBase = 10f;
        float windWalkCoolBase = 60f;
        public float breakCool;
        float breakCoolBase = 0.5f;
        public float returnCool;
        float returnCoolBase = 60f;
        public bool canCreateWall = true;
        public Vector2 aim;
        int lookingAt = -1;//왼쪽부터 시계방향으로 0123
        SpriteRenderer spr;
        public Animator playerAnimator;
        public JoystickControl joystick = new JoystickControl();

        int pickaxeAmountBase = 20;
            
        public void MovePlayer(GameObject player, Vector2 location)
        {
            player.transform.position = location;
        }

        public override void Attached()
        {
            state.SetTransforms(state.Location, transform);
            state.SetAnimator(playerAnimator);
        }

        // Start is called before the first frame update
        void Start()
        {
            mapCtrl = GameObject.Find("GameController").GetComponent<MapController>();
            itemCtrl = GameObject.Find("GameController").GetComponent<ItemController>();
            gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
            gameCtrl.players.Add(entity);
            spr = player.gameObject.GetComponentInChildren<SpriteRenderer>();
            state.headRight = false;
            if (entity.IsOwner)
            {
                state.Inventory[0] = pickaxeAmountBase;
                state.Color = new Color(0, 0, 0, 255);
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

        public override void SimulateOwner()//플레이어 조작 관련 코드
        {
            var speed = 4f;
            var movement = Vector3.zero;
            int output = -1;
            // 이동 관련 코드

            if (state.WindWalking)
            {
                speed = 8f;
                state.Color = new Color(0, 0, 0, 0);
            }
            else
            {
                state.Color = new Color(255, 255, 255, 255);//시껌댕이 탈출
            }
            if(!state.Paralyzed)
            {
                if (Input.GetKey(KeyCode.LeftArrow) == true || JoystickControl.directionX == 1)
                {
                    if(state.headRight)
                    {
                        state.headRight = false;
                    }
                    movement.x -= 1f;
                    lookingAt = 0;
                }
                if (Input.GetKey(KeyCode.RightArrow) == true || JoystickControl.directionX == 2)
                {
                    if(!state.headRight)
                    {
                        state.headRight = true;
                    }
                    movement.x += 1f;
                    lookingAt = 2;
                }
                if (Input.GetKey(KeyCode.UpArrow) == true || JoystickControl.directionY == 1)
                {
                    movement.y += 1f;
                    lookingAt = 1;
                }
                if (Input.GetKey(KeyCode.DownArrow) == true || JoystickControl.directionY == 2)
                {
                    movement.y -= 1f;
                    lookingAt = 3;
                }
                if (Input.GetKey(KeyCode.R) == true || JoystickControl.btnNum == 5)//귀환 - 금을 내려놓고 기지로 귀환
                {
                    if (returnCool == 0)
                    {
                        if(state.Inventory[1] == 1)
                        {
                            itemCtrl.CreateItem((int)player.transform.position.x, (int)player.transform.position.y, 1, false);
                            state.Inventory[1] = 0;
                        }
                        MovePlayer(player, new Vector2(50, 50));
                        returnCool = returnCoolBase;
                    }
                    else
                    {
                        //Debug.Log("in Return-Cooltime");
                    }
                    JoystickControl.btnNum = 0;
                }
            }
            if (movement != Vector3.zero)
            {
                //ani.SetBool("Walking", true);
                transform.position = transform.position + (movement.normalized * speed * BoltNetwork.FrameDeltaTime);
                state.isMoving = true;
            }
            else
            {
                //ani.SetBool("Walking", false);
                state.isMoving = false;
            }
            //벽 파괴
            if (Input.GetKey(KeyCode.A) == true || JoystickControl.btnNum == 1)
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
                JoystickControl.btnNum = 0;
            }

            //파괴가능 벽 생성(바리케이드)
            if (Input.GetKey(KeyCode.S) == true || JoystickControl.btnNum == 2)
            {
                if (state.Inventory[2] > 0 && canCreateWall)
                {
                    output = mapCtrl.CreateWall(1, (int)aim.x, (int)aim.y, false);
                    if (output == 0)
                        --state.Inventory[2];
                }
                else
                {
                    Debug.Log("Cannot Create Barricade");
                }
                JoystickControl.btnNum = 0;
            }

            // 플레이어에게 스킬을 사용하는 파트
            if (Input.GetKeyUp(KeyCode.Q) == true || JoystickControl.btnNum == 4) // 방해
            {
                if(targetPlayer != null /*&& GameController.time < 600 */) //현재는 시간대별로 사용하는거 막아놓음
                {
                    Debug.LogWarning("interrupt : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 0;
                    evnt.Send();
                }
                else
                {
                    Debug.LogError("Cannot Use Now");
                }
                JoystickControl.btnNum = 0;
            }
            if (Input.GetKeyUp(KeyCode.D) == true || JoystickControl.btnNum == 6 || JoystickControl.btnNum == 7) // 사보타지 색출,  윈드웤(은신)
            {
                if (!GameController.isSabotage || JoystickControl.btnNum == 7)
                {
                    if (targetPlayer != null && canFindSabotage == true/* && GameController.time < 600*/)
                    {
                        Debug.LogWarning("FindSabotage : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                        var evnt = PlayerInteraction.Create();
                        evnt.AttakingPlayer = GameController.playerCode;
                        evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                        evnt.Action = 1;
                        evnt.Send();
                        canFindSabotage = false;
                    }
                    else
                    {
                        Debug.LogError("Cannot Use Now");
                    }
                }
                else
                {
                    if (windWalkCool == 0)
                    {
                        windWalkDuration = windWalkDurationBase;
                        windWalkCool = windWalkCoolBase;
                        state.WindWalking = true;
                    }
                }
                JoystickControl.btnNum = 0;
            }
            if (Input.GetKeyUp(KeyCode.E) == true || JoystickControl.btnNum == 3) // 회복
            {
                if (targetPlayer != null)
                {
                    Debug.LogWarning("heal : " + targetPlayer.GetState<IPlayerState>().PlayerName);
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 2;
                    evnt.Send();
                }
                else
                {
                    Debug.LogError("Cannot Use Now");
                }
                JoystickControl.btnNum = 0;
            }
                //아이템 획득 코드
            if (itemCtrl.nearestItem != null)
            {
                if (Vector2.Distance(player.transform.position, itemCtrl.nearestItem.transform.position) < 0.5)
                {
                    itemCtrl.GetItem(itemCtrl.nearestItemX, itemCtrl.nearestItemY, state, false);
                }
            }
        }
        void Update()
        {
            if (state.isMoving)
            {
                state.Animator.Play("walk_side");
            }
            else
            {
                state.Animator.Play("Idle");
            }
            if(state.headRight)
            {
                spr.flipX = true;
            }
            else
            {
                spr.flipX = false;
            }
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            //플레이어 은신에 관한 내용
            entity.GetComponentInChildren<SpriteRenderer>().color = state.Color;
            if(state.Color == new Color(0,0,0,0))
            {
                playerName.SetActive(false);
            }
            else
            {
                playerName.SetActive(true);
            }

            if (true)//쿨타임관련 코드
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
                if (windWalkCool > 0)
                {
                    windWalkCool -= Time.deltaTime;
                }
                if (windWalkCool < 0)
                {
                    windWalkCool = 0;
                }
                if (windWalkDuration > 0)
                {
                    windWalkDuration -= Time.deltaTime;
                }
                if (windWalkDuration < 0)
                {
                    windWalkDuration = 0;
                    state.WindWalking = false;
                }
            }


            if (entity.IsOwner)
            {
                //플레이어 상태 코드
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
                    playerView.spotAngle = 75;
                }

                mapCtrl.FindWall(mapCtrl.mapObject);
                mapCtrl.FindChest();
                itemCtrl.FindItem(itemCtrl.itemObject);
                Aiming();

                if (Vector2.Distance(entity.transform.position, new Vector2(49.5f,49.5f)) < 1)
                {
                    if(state.Inventory[0] < pickaxeAmountBase)
                    {
                        state.Inventory[0] = pickaxeAmountBase;
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

