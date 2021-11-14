﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using TMPro;
using System.Collections;
namespace DoMine
{
    public class PlayerControl : EntityBehaviour<IPlayerState>
    {
        [SerializeField] GameObject player = null;
        Rigidbody2D playerRB;
        [SerializeField] GameObject aimIndicator = null;
        [SerializeField] GameObject playerName = null;
        [SerializeField] GameObject hammer = null;
        [SerializeField] GameObject gold = null;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;
        public UIController uiCtrl;
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
        float breakCoolBase = 0.6f;
        public float returnCool;
        float returnCoolBase = 60f;
        public bool canCreateWall = true;
        public float createCool;
        float createCoolBase = 0.1f;
        public Vector2 aim;
        int lookingAt = -1;//왼쪽부터 시계방향으로 0123
        SpriteRenderer spr;
        SpriteRenderer spr_hammer;
        SpriteRenderer spr_gold;
        public Animator playerAnimator;
        public Animator hammerAnimator;
        public JoystickControl joystick;
        int pickaxeAmountBase = 2000;
        int barricadeBase = 5;
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
            playerRB = player.GetComponent<Rigidbody2D>();
            mapCtrl = GameObject.Find("GameController").GetComponent<MapController>();
            itemCtrl = GameObject.Find("GameController").GetComponent<ItemController>();
            gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
            uiCtrl = GameObject.Find("GameController").GetComponent<UIController>();
            joystick = GameObject.FindObjectOfType<JoystickControl>();
            gameCtrl.players.Add(entity);
            spr = player.gameObject.GetComponentInChildren<SpriteRenderer>();
            spr_hammer = hammer.gameObject.GetComponentInChildren<SpriteRenderer>();
            spr_gold = gold.gameObject.GetComponentInChildren<SpriteRenderer>();
            if(entity.IsOwner)
            {
                state.headRight = false;
                state.isMoving = true;
                state.isMining = false;
                state.isBreak = true;
                state.makeWall = false;
            }
            
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
            if (GameController.isSabotage)
            {
                joystick.minerSkill.gameObject.SetActive(false);
                joystick.sabotageSkill.gameObject.SetActive(true);
            }
        }
        void OnDestroy()
        {
            gameCtrl.players.Remove(entity);
        }

        public override void SimulateOwner()//플레이어 조작 관련 코드
        {
            var speed = 2f;
            var movement = Vector3.zero;
            int output = -1;
            string output2 = null;
            // 이동 관련 코드

            if (state.WindWalking)
            {
                speed = 4f;
                state.Color = new Color(0, 0, 0, 0);
            }
            else
            {
                state.Color = new Color(255, 255, 255, 255);//시껌댕이 탈출
            }
            if(!state.Paralyzed)
            {
                //조이스틱 컨트롤 부분(현재 xy 단순값 입력되어서 lookingAt변수에 오류가있음, 이는 나중에 xy값계산해서 제일 가까운 값으로 배정하는식으로 변경해야할듯.)

                lookingAt = JoystickControl.lookAt; // joystickcontrol의 방향번호랑 그냥 동기화시켜버림 if문 필요없게 바꿈
                state.headRight = JoystickControl.headRight; // 머리돌리기도 그대로 동기화

                //키보드컨트롤 기존방식과 동일 lookingAt제대로 동작
                if (Input.GetKey(KeyCode.LeftArrow) == true)
                {
                    if (state.headRight)
                    {
                        state.headRight = false;
                    }
                    movement.x -= 1f;
                    lookingAt = 0;
                }
                if (Input.GetKey(KeyCode.RightArrow) == true)
                {
                    if(!state.headRight)
                    {
                        state.headRight = true;
                    }
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

                /*if (movement != Vector3.zero)
                {
                    //ani.SetBool("Walking", true);
                    playerRB.position = playerRB.position + (Vector2)(movement.normalized * speed * 2 * BoltNetwork.FrameDeltaTime);
                    state.isMoving = true;
                }*/
                if (joystick.Horizontal != 0 || joystick.Vertical != 0)
                {
                    Vector3 upMovement = Vector3.up * joystick.Vertical;
                    Vector3 rightMovement = Vector3.right * joystick.Horizontal;
                    playerRB.position += ((Vector2)upMovement + (Vector2)rightMovement) * speed * JoystickControl.distance * BoltNetwork.FrameDeltaTime;
                    //state.isMoving = true;
                    state.Act = 1; //플레이어 이동시 애니메이션
                }
                else
                {
                    //ani.SetBool("Walking", false);
                    //state.isMoving = false;
                    state.Act = 0; //정지시 애니메이션
                }

                if (Input.GetKey(KeyCode.R) == true || JoystickControl.btnNum == 5)//귀환 - 금을 내려놓고 기지로 귀환
                {
                    if (returnCool == 0)
                    {
                        if(state.Inventory[1] == 1)
                        {
                            itemCtrl.CreateItem((int)player.transform.position.x, (int)player.transform.position.y, 1, false);
                            state.Inventory[1] = 0;
                            state.isMoving = true;
                        }
                        MovePlayer(player, new Vector2(50, 50));
                        returnCool = returnCoolBase;
                        state.Paralyzed = true;
                        paralyzeCool = paralyzeCoolBase/5; //귀환시 5초간 정지
                    }
                    else
                    {
                        uiCtrl.MessagePrint("귀환 쿨타임 중입니다.");
                    }
                    JoystickControl.btnNum = 0;
                }
            }
            
            //벽 파괴
            if (Input.GetKey(KeyCode.A) == true || JoystickControl.btnNum == 1)
            {
                if (breakCool == 0 && mapCtrl.nearestWall != null)
                {
                    
                    if (Vector2.Distance(player.transform.position, mapCtrl.nearestWall.transform.position) < 0.8 && state.Inventory[0] > 0)
                    {
                        state.isMining = true;
                        //mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY, false, false, -1);
                        breakCool = breakCoolBase;
                        state.Inventory[0]--;//곡괭이 갯수 소진
                        if(state.Inventory[0] == 0)
                        {
                            joystick.pick.GetComponent<Button>().interactable = false; //곡괭이 갯수 전부 소진 시 버튼 비활성화
                        }
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
                    state.makeWall = true;
                    createCool = createCoolBase;
                    output = mapCtrl.CreateWall(4, (int)aim.x, (int)aim.y, false);
                    if (output == 0)
                        --state.Inventory[2];
                    else
                        uiCtrl.MessagePrint("해당위치에 만들 수 없습니다");
                }
                else
                {
                    uiCtrl.MessagePrint("해당위치에 만들 수 없습니다");
                }
                JoystickControl.btnNum = 0;
            }
            
            // 플레이어에게 스킬을 사용하는 파트
            if (Input.GetKeyUp(KeyCode.Q) == true || JoystickControl.btnNum == 4) // 방해
            {
                if(state.Inventory[3]>0 && targetPlayer != null /*&& GameController.time < 600 */) //현재는 시간대별로 사용하는거 막아놓음
                {
                    uiCtrl.MessagePrint((targetPlayer.GetState<IPlayerState>().PlayerName + "를 <color=red>공격</color>").ToString());
                    var evnt = PlayerInteraction.Create();
                    evnt.AttakingPlayer = GameController.playerCode;
                    evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                    evnt.Action = 0;
                    evnt.Send();
                    --state.Inventory[3];
                }
                else
                {
                    uiCtrl.MessagePrint("사용 할 수 없습니다.");
                }
                JoystickControl.btnNum = 0;
            }
            if (Input.GetKeyUp(KeyCode.D) == true || JoystickControl.btnNum == 6 || JoystickControl.btnNum == 7) // 사보타지 색출,  윈드웤(은신)
            {
                if (!GameController.isSabotage || JoystickControl.btnNum == 7)
                {
                    if (targetPlayer != null && canFindSabotage == true/* && GameController.time < 600*/)
                    {
                        uiCtrl.MessagePrint(("사보타지인지 확인합니다 : " + targetPlayer.GetState<IPlayerState>().PlayerName).ToString());
                        var evnt = PlayerInteraction.Create();
                        evnt.AttakingPlayer = GameController.playerCode;
                        evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                        evnt.Action = 1;
                        evnt.Send();
                        canFindSabotage = false;
                        joystick.minerSkill.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        uiCtrl.MessagePrint("사용 할 수 없습니다.");
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
                if (state.Inventory[4] > 0 && targetPlayer != null)
                {
                    if(targetPlayer.GetState<IPlayerState>().Blinded == true) // 대상이 시야가 축소된상태라면
                    {
                        uiCtrl.MessagePrint(("<color=green>치료</color> : " + targetPlayer.GetState<IPlayerState>().PlayerName).ToString());
                        var evnt = PlayerInteraction.Create();
                        evnt.AttakingPlayer = GameController.playerCode;
                        evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                        evnt.Action = 2;
                        evnt.Send();
                        --state.Inventory[4];
                    }
                }
                else
                {
                    uiCtrl.MessagePrint("사용 할 수 없습니다.");
                }
                JoystickControl.btnNum = 0;
            }
                //아이템 획득 코드
            if (itemCtrl.nearestItem != null)
            {
                if (Vector2.Distance(player.transform.position, itemCtrl.nearestItem.transform.position) < 0.5)
                {
                    
                    output2 = itemCtrl.GetItem(itemCtrl.nearestItemX, itemCtrl.nearestItemY, state, false);
                    if(output2 != null)
                    {
                        uiCtrl.MessagePrint(output2);
                    }

                }
                if (state.Inventory[1] == 1)
                    state.isMoving = false;
            }
            joystick.compasscontrol(player, new Vector2(49, 49), 4f); //나침반 돌아가는 함수(JoystickControl에 구현)
           
            if (state.Inventory[0] != 0)
            {
                joystick.pick.GetComponent<Button>().interactable = true;   //곡괭이 회복하면 버튼 활성화
            }
            joystick.NumOfItem(0, state.Inventory[2]);
            joystick.NumOfItem(1, state.Inventory[3]);
            joystick.NumOfItem(2, state.Inventory[4]);
        }
        void Update()
        {
            if (state.Paralyzed)
            {
                hammer.SetActive(false);
                state.Animator.Play("hit1_down");
            }
            else if(state.isMining)
            {
                hammer.SetActive(true);
                state.Animator.Play("hammring_ham");
            }
            else if(state.makeWall)
            {
                hammer.SetActive(false);
                state.Animator.Play("duck_side");
            }
            else
            {
                hammer.SetActive(false);
                switch (state.Act)
                {
                    case 0:
                        state.Animator.Play("Idle");
                        break;
                    case 1:
                        if (state.isMoving)
                        {
                            state.Animator.Play("walk_side");
                        }
                        else
                        {
                            gold.SetActive(true);
                            state.Animator.Play("carry_side");
                        }
                        break;
                    default:
                        break;
                }
            }
            
            if(state.headRight)
            {
                spr.flipX = true;
                spr_hammer.flipX = true;
                spr_gold.flipX = true;
            }
            else
            {
                spr.flipX = false;
                spr_hammer.flipX = false;
                spr_gold.flipX = false;
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
                    if(breakCool < 0.3f && state.isBreak)
                    {
                        state.isBreak = false;
                        mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY, false, false, -1);

                    }

                }
                if (breakCool < 0)
                {
                    state.isMining = false;
                    state.isBreak = true;
                    breakCool = 0;
                }
                if (returnCool > 0)
                {
                    returnCool -= Time.deltaTime;
                    joystick.home.gameObject.SetActive(true);
                    joystick.baseCamp.GetComponent<Button>().interactable = false;
                    joystick.CoolTimeUI(0, returnCool, returnCoolBase);
                }
                if (returnCool < 0)
                {
                    returnCool = 0;
                    joystick.baseCamp.GetComponent<Button>().interactable = true;
                    joystick.home.gameObject.SetActive(false);
                }
                if (windWalkCool > 0)
                {
                    windWalkCool -= Time.deltaTime;
                    joystick.windWalk.gameObject.SetActive(true);
                    joystick.sabotageSkill.GetComponent<Button>().interactable = false;
                    joystick.CoolTimeUI(1, windWalkCool, windWalkCoolBase);
                }
                if (windWalkCool < 0)
                {
                    windWalkCool = 0;
                    joystick.sabotageSkill.GetComponent<Button>().interactable = true;
                    joystick.windWalk.gameObject.SetActive(false);
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
                if(createCool > 0)
                {
                    createCool -= Time.deltaTime;
                }
                if(createCool < 0)
                {
                    state.makeWall = false;
                    createCool = createCool = 0;
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
                    if(state.Inventory[0] < pickaxeAmountBase || state.Inventory[2] < barricadeBase)
                    {
                        state.Inventory[0] = pickaxeAmountBase;
                        state.Inventory[2] = barricadeBase;
                        uiCtrl.MessagePrint("곡괭이와 바리케이드가 충전되었습니다");//중앙 안전지대 이동시 곡괭이 회복
                    }
                    if (state.Inventory[1] == 1 && gameCtrl.playerList[GameController.playerCode] == 0)
                    {
                        state.isMoving = true;
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
            
            switch (lookingAt)//최초엔 보는방향을 기준으로 에임을 둠 왼쪽부터 시계방향으로 0123
            {
                case 0:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x - 1), (int)Math.Round(state.Location.Position.y));
                    break;
                case 1:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y + 1));
                    break;
                case 2:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x + 1), (int)Math.Round(state.Location.Position.y));
                    break;
                case 3:
                    aim = new Vector2((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y - 1));
                    break;
            }
            
            i = 0;
            BoltEntity nearestPlayer = null;
            foreach (BoltEntity target in gameCtrl.players)
            {
                if (Vector2.Distance(target.GetState<IPlayerState>().Location.Transform.position, player.transform.position) < 0.5)//유저가 가까이있을시 유저를 조준하도록
                {
                    if (target == gameCtrl.myPlayer)
                    {
                    }
                    else if (nearestPlayer == null)
                    {
                        nearestPlayer = target;//처음 걸린유저
                    }
                    else if (Vector2.Distance(target.GetState<IPlayerState>().Location.Transform.position, aim) < Vector2.Distance(nearestPlayer.GetState<IPlayerState>().Location.Transform.position, player.transform.position))
                    {
                        nearestPlayer = target;// 더 가까운 유저가 있을 시엔 그 유저를 우선시
                    }
                }
            }
            foreach (BoltEntity target in gameCtrl.players)
            {
                if (Vector2.Distance(target.GetState<IPlayerState>().Location.Transform.position, aim) < 0.5)//유저가 가까이있을시 유저를 조준하도록
                {
                }
                else
                    i++;
            }

            if (i == gameCtrl.players.Count && Vector2.Distance(aim, new Vector2(49.5f, 49.5f)) > 5) // 중앙선에서 5칸 이내일시
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
        IEnumerator Actdelay()
        {
            
            yield return new WaitForSeconds(10f);
        }
    }
}

