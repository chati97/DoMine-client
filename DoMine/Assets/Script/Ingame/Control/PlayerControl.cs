using System;
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
        [SerializeField] GameObject gold_L = null;
        [SerializeField] GameObject gold_R = null;
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
        float breakCoolBase = 0.4f;
        public float returnCool;
        float returnCoolBase = 60f;
        public bool canCreateWall = true;
        public float createCool;
        float createCoolBase = 0.1f;
        public Vector2 aim;
        int lookingAt = -1;//왼쪽부터 시계방향으로 0123
        SpriteRenderer spr;
        SpriteRenderer spr_hammer;
        public Animator playerAnimator;
        public Animator hammerAnimator;
        public JoystickControl joystick;
        public static int pickaxeAmountBase = 0;
        public static int barricadeBase = 0;
        public static bool canUseFishing;
        public GameObject soundhammer;
        public GameObject para;
        public GameObject atk;
        public GameObject getit;
        public GameObject atkfail;
        public GameObject atksuc; //색출 성공 사운드
        public GameObject crewall;
        public GameObject hiding;
        AudioSource hammersound;
        AudioSource paralyzedsound;
        AudioSource attacksound;
        AudioSource getitemsound;
        AudioSource failsound;
        AudioSource successsound;
        AudioSource createwallsound;
        AudioSource hidesound;
       
        bool soundcheck = false;
        bool soundcheck2 = true;
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
            hammersound = soundhammer.gameObject.GetComponent<AudioSource>();
            paralyzedsound = para.gameObject.GetComponent<AudioSource>();
            attacksound = atk.gameObject.GetComponent<AudioSource>();
            getitemsound = getit.gameObject.GetComponent<AudioSource>();
            failsound = atkfail.gameObject.GetComponent<AudioSource>();
            successsound = atksuc.gameObject.GetComponent<AudioSource>();
            createwallsound = crewall.gameObject.GetComponent<AudioSource>();
            hidesound = hiding.gameObject.GetComponent<AudioSource>();
            if(entity.IsOwner)
            {
                state.headRight = false;
                state.isMoving = true;
                state.isMining = false;
                state.isBreak = true;
                state.makeWall = false;
                state.carryGold = false;
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
            pickaxeAmountBase = 0;
            barricadeBase = 0;
            for (int i = 0; i < 5; i++) //인벤초기화
            {
                state.Inventory[i] = 0;
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
                if (state.Inventory[1] == 1 || (GameController.isSabotage == true && state.carryGold))
                    speed = 1f;
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
                    playerRB.position += ((Vector2)upMovement + (Vector2)rightMovement) * 1.5f * speed * JoystickControl.distance * BoltNetwork.FrameDeltaTime;
                    //state.isMoving = true;
                    state.isMoving = true; //플레이어 이동시 애니메이션
                }
                else
                {
                    //ani.SetBool("Walking", false);
                    //state.isMoving = false;
                    state.isMoving = false; //정지시 애니메이션
                }

                if (Input.GetKey(KeyCode.R) == true || JoystickControl.btnNum == 5)//귀환 - 금을 내려놓고 기지로 귀환
                {
                    if (returnCool == 0)
                    {
                        if(state.Inventory[1] == 1)
                        {
                            itemCtrl.CreateItem((int)Math.Round(state.Location.Position.x), (int)Math.Round(state.Location.Position.y), 1, false);
                            state.Inventory[1] = 0;
                            state.isMoving = true;
                            GameController.MessageCreate(("(" + (int)Math.Round(state.Location.Position.x)+ ","+ (int)Math.Round(state.Location.Position.y)+ ") 에서 <color=yellow>코인</color>이 떨어졌습니다").ToString());
                        }
                        uiCtrl.MessagePrint(("(" + (int)Math.Round(state.Location.Position.x) + "," + (int)Math.Round(state.Location.Position.y) + ") 에서 귀환합니다").ToString());
                        MovePlayer(player, new Vector2(49.5f, 49.5f));
                        returnCool = returnCoolBase;
                        state.Paralyzed = true;
                        paralyzeCool = paralyzeCoolBase/2; //귀환시 5초간 정지
                    }
                    else
                    {
                        uiCtrl.MessagePrint("귀환 쿨타임 중입니다.");
                    }
                    JoystickControl.btnNum = 0;
                }
                if (!state.carryGold)
                {
                    //벽 파괴
                    if (Input.GetKey(KeyCode.A) == true || JoystickControl.btnNum == 1)
                    {
                        if (breakCool == 0 && mapCtrl.nearestWall != null)
                        {
                            if (Vector2.Distance(player.transform.position, mapCtrl.nearestWall.transform.position) < 0.8 && state.Inventory[0] > 0)
                            {
                                mapCtrl.tempWall = mapCtrl.nearestWall.transform.position;
                                state.isMining = true;
                                soundcheck = true;
                                //mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY, false, false, -1);
                                breakCool = breakCoolBase;
                                
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
                            {
                                createwallsound.Play();
                                --state.Inventory[2];
                            }
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
                        if (state.Inventory[3] > 0 && targetPlayer != null && targetPlayer.GetState<IPlayerState>().Paralyzed == false /*&& GameController.time < 600 */) //상대가 cc안걸리고 내가 공격템이 있고 타겟플레이어가 있으면
                        {
                            if (targetPlayer.GetState<IPlayerState>().Inventory[1] == 1 && gameCtrl.playerList[GameController.playerCode] == 0 && targetPlayer.GetState<IPlayerState>().Inventory[4] == 0)//만약 내가 입금안한 광부고 상대가 힐템없이 금을 가지고 있으면
                            {
                                state.Inventory[1] = 1;//금내꺼
                                GameController.MessageCreate((gameCtrl.playerNameList[GameController.playerCode] + "가 " + (int)Math.Round(state.Location.Position.x) + "," + (int)Math.Round(state.Location.Position.y) + ") 에서 <color=yellow>코인</color>을 획득했습니다.").ToString());
                            }
                            if (targetPlayer.GetState<IPlayerState>().Inventory[4] == 0)
                                attacksound.Play();
                            else
                                failsound.Play();
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
                            if (targetPlayer != null && canFindSabotage == true && GameController.time < 300)
                            {
                                uiCtrl.MessagePrint(("사보타지인지 확인합니다 : " + targetPlayer.GetState<IPlayerState>().PlayerName).ToString());
                                if (gameCtrl.playerList[targetPlayer.GetState<IPlayerState>().PlayerCode] == 1)
                                    successsound.Play();
                                var evnt = PlayerInteraction.Create();
                                evnt.AttakingPlayer = GameController.playerCode;
                                evnt.TargetPlayer = targetPlayer.GetState<IPlayerState>().PlayerCode;
                                evnt.Action = 1;
                                evnt.Send();
                                canFindSabotage = false;
                                joystick.minerSkill.GetComponent<Button>().interactable = false;
                                if(gameCtrl.playerList[targetPlayer.GetState<IPlayerState>().PlayerCode] != 1)//만약 사보타지가 아니면
                                {
                                    state.Inventory[0] = 0;
                                    state.Inventory[2] = 0;
                                    state.Blinded = true;
                                    blindCool = blindCoolBase;
                                    blindCool = blindCoolBase;
                                    state.Paralyzed = true;
                                    paralyzeCool = paralyzeCoolBase; //자업자득 파산엔딩
                                }
                            }
                            else if(GameController.time > 300)
                            {
                                uiCtrl.MessagePrint("5분뒤부터 사용 할 수 있습니다.");
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
                                hidesound.Play();
                                windWalkDuration = windWalkDurationBase;
                                windWalkCool = windWalkCoolBase;
                                state.WindWalking = true;
                            }
                           else
                            {
                                uiCtrl.MessagePrint("사용 할 수 없습니다.");
                            }
                        }
                        JoystickControl.btnNum = 0;
                    }
                    if (Input.GetKeyUp(KeyCode.E) == true || JoystickControl.btnNum == 3) // 회복
                    {
                        if (state.Inventory[4] > 0 && targetPlayer != null)
                        {
                            if (targetPlayer.GetState<IPlayerState>().Blinded == true) // 대상이 시야가 축소된상태라면
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

                    //가짜 금 낚시
                    if (Input.GetKey(KeyCode.F) == true || JoystickControl.btnNum == 8)
                    {
                        if(GameController.isSabotage == true && canUseFishing == true)
                        {
                            state.isMoving = false;
                            state.carryGold = true;
                            canUseFishing = false;
                            GameController.MessageCreate("누군가 (" + (int)state.Location.Position.x + ", " + (int)state.Location.Position.y + ")에서 <color=yellow>코인</color>을 발견했습니다");
                            GameController.MessageCreate((state.PlayerName + "가 (" + (int)state.Location.Position.x + ", " + (int)state.Location.Position.y + ")에서 <color=yellow>코인</color>을 획득했습니다").ToString());
                        }
                        JoystickControl.btnNum = 0;
                    }
                }
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
                    getitemsound.Play();
                }
                
            }
            joystick.compasscontrol(player, new Vector2(49, 49), 4f); //나침반 돌아가는 함수(JoystickControl에 구현)
           
            joystick.NumOfItem(0, state.Inventory[2]);  //barricade
            joystick.NumOfItem(1, state.Inventory[3]);  //attack
            joystick.NumOfItem(2, state.Inventory[4]);  //heal
            joystick.NumOfItem(3, state.Inventory[0]);  //pick

            joystick.Position.text = string.Format("({0}, {1})", (int)player.transform.position.x, (int)player.transform.position.y);
        }
        void Update()
        {
            if (GameController.isSabotage)
            {
                joystick.minerSkill.gameObject.SetActive(false);
                joystick.sabotageSkill.gameObject.SetActive(true);
                joystick.fakeGold.gameObject.SetActive(true);
            }

            if (state.Inventory[1] == 0 && GameController.isSabotage == false)
            {
                if (entity.IsOwner)
                {
                    state.carryGold = false;
                }
                gold_R.SetActive(false);
                gold_L.SetActive(false);
            }
            else if (state.Inventory[1] == 1 && GameController.isSabotage == false)
            {
                if(entity.IsOwner)
                {
                    state.isMoving = false;
                    state.carryGold = true;
                }
                
            }
            if (state.Paralyzed)
            {
                if(GameController.isSabotage == true)
                {
                    state.carryGold = false;
                    gold_R.SetActive(false);
                    gold_L.SetActive(false);
                }
                hammer.SetActive(false);
                state.Animator.Play("hit1_down");
            }
            else if(state.isMining)
            {
                hammer.SetActive(true);
                
                state.Animator.Play("hammering_ham");
            }
            else if(state.makeWall)
            {
                hammer.SetActive(false);
                state.Animator.Play("duck_side");
            }
            else
            {
                hammer.SetActive(false);
                if(!state.carryGold)
                {
                    if (state.isMoving)
                        state.Animator.Play("walk_side");
                    else
                        state.Animator.Play("Idle");
                }
                else
                {
                    if(state.headRight)
                    {
                        gold_R.SetActive(true);
                        gold_L.SetActive(false);
                    }
                    else
                    {
                        gold_L.SetActive(true);
                        gold_R.SetActive(false);
                    }
                    state.Animator.Play("carry_side");
                }
            }
            
            if(state.headRight)
            {
                spr.flipX = true;
                spr_hammer.flipX = true;
            }
            else
            {
                spr.flipX = false;
                spr_hammer.flipX = false;
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
                    if(breakCool < 0.2f && state.isBreak)
                    {
                        state.isBreak = false;
                        if (mapCtrl.tempWall == (Vector2)mapCtrl.nearestWall.transform.position)
                        {
                            mapCtrl.DestroyWall((int)mapCtrl.nearestWall.transform.position.x, (int)mapCtrl.nearestWall.transform.position.y, false, false, -1);
                            state.Inventory[0]--;//곡괭이 갯수 소진
                        }
                        mapCtrl.tempWall = new Vector2(-1, -1);
                    }
                    if (breakCool < 0.1f && soundcheck)
                    {
                        soundcheck = false;
                        hammersound.Play();
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
                    if(soundcheck2)
                    {
                        soundcheck2 = false;
                        paralyzedsound.Play();
                    }
                    paralyzeCool -= Time.deltaTime;
                }
                else
                {
                    paralyzeCool = 0;
                    soundcheck2 = true;
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
                        state.Inventory[1] = 0;
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
                if (Vector2.Distance(target.GetState<IPlayerState>().Location.Transform.position, player.transform.position) < 0.75)//유저가 가까이있을시 유저를 조준하도록
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
                if (Vector2.Distance(target.GetState<IPlayerState>().Location.Transform.position, aim) < 0.5)//에임위치에 유저있으면 벽 못세우도록
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

