﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using UdpKit;
using UnityEngine.SceneManagement;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        [SerializeField] ItemController IC = null;// 각 스크립트 연결
        [SerializeField] MapController MC = null;
        [SerializeField] UIController UC = null;
        public int[] playerList = {0,-1,-1,-1,-1,-1,-1,-1,-1,-1};//최대 10인 입장, 인덱스는 플레이어코드(-1 없음, 0 광부, 1 사보타지, 2는 입금한광부..?)
        public string[] playerNameList = {"", "", "", "", "", "", "", "", "", ""};
        public static int playerCode;//플레이어 코드
        public int playerNum; //최초입장한유저수
        int goldAmount = 0;//생성된 금 갯수
        int goldSaved = 0;//입금된 금 갯수
        int sabotages = 0;//사보타지 수
        public static float time; //게임시간
        public List<BoltEntity> players = new List<BoltEntity>(); //볼트엔티티 모으는 리스트
        public BoltEntity myPlayer;//본인 플레이어
        public static bool isSabotage;//플레이어가 사보타지인지 확인
        public static bool gameStarted;//게임 시작여부
        public static bool gameLoaded;//게임로딩여부
        public static bool gameEnded;//게임종료여부
        public GameObject mainbgm;
        public GameObject resultbgm;
        AudioSource main;
        AudioSource result;
        IPlayerState mystate = null;//본인 상태 수정위해 가지고 있는변수
        float timeBase = 600;
        public int lessSabotage = 0;

        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector3(UnityEngine.Random.Range(48, 52), UnityEngine.Random.Range(48, 52), 0);
            myPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPos, Quaternion.identity);
            myPlayer.TakeControl();
            MC.player = myPlayer;//Mc에 넣음
            IC.player = myPlayer;//Ic에 넣음
            myPlayer.TryFindState<IPlayerState>(out mystate);
            gameLoaded = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            Application.runInBackground = true;
            TitleMenu.isGameEnd = false;
            PlayerControl.canFindSabotage = true;
            playerCode = -1;
            isSabotage = false;
            sabotages = 0;
            goldSaved = 0;
            gameStarted = false;//게임 시작여부
            gameLoaded = false;//게임로딩여부
            gameEnded = false;
            main = mainbgm.gameObject.GetComponent<AudioSource>();
            result = resultbgm.gameObject.GetComponent<AudioSource>();
            if (BoltNetwork.IsServer)
            {
                playerNum = 1;
                playerCode = 0;
            }
            time = 20;
            MC.CreateMap(MC.mapArray = MC.MakeMapArr(), MC.mapObject);
        }

        public override void Connected(BoltConnection connection)
        {
            if(BoltNetwork.IsServer)
            {
                if(gameStarted)
                {
                    connection.Disconnect();
                }
            }
        }

        public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)//호스트가 튕겼을시
        {
            if(gameEnded !=true) // 게임 종료시엔 호스트가 나가도 종료화면이 그대로 이어지도록 만약 아닐시엔 팅김
            {
                TitleMenu.isNormal = false;
                time = 1000;//게임 중단
                UC.GameWinner(-2, false, false, null);//게임종료화면으로 이동
            }
        }
        public override void OnEvent(SabotageCaptured evnt)
        {
            if(evnt.Player == playerCode)
            {
                if (evnt.isSabotage == true)
                {
                    MessageCreate((playerNameList[evnt.Player] + " 는 <color=red>사보타지</color>였습니다.").ToString());//본인이 사보타지임을 알리는 메시지 발송
                    UC.MessagePrint("<color=red>당신은 발각되었습니다!</color>");
                    myPlayer.transform.position = new Vector2(49.5f, 49.5f); // 시작위치로 보내버리기
                    mystate.Blinded = true;
                    PlayerControl.blindCool = PlayerControl.blindCoolBase;
                    PlayerControl.blindCool = PlayerControl.blindCoolBase;
                    mystate.Paralyzed = true;
                    PlayerControl.paralyzeCool = PlayerControl.paralyzeCoolBase; // 그리고 공격받은 상태로 만들기
                    PlayerControl.barricadeBase = 3;
                    mystate.Inventory[0] = 0;
                    mystate.Inventory[2] = 0;
                }
                else
                {
                    MessageCreate((playerNameList[evnt.Player] + " 는 <color=green>광부</color>였습니다.").ToString());
                }
            }
        }

        public override void OnEvent(GoldSteal evnt)
        {
            if(evnt.Attacker == playerCode && mystate.Inventory[1] == 0 && playerList[playerCode] == 0)//금안넣은 광부인데 공격자가 나면
            {
                mystate.Inventory[1] = 1;
            }
        }

        public override void OnEvent(PlayerInteraction evnt)
        {
            switch(evnt.Action)
            {
                case 0 ://방해 공작 통일
                    if (playerCode == evnt.TargetPlayer)
                    {
                        if(mystate.Inventory[4] > 0)//본인이 힐 아이템 소지시
                        {
                            UC.MessagePrint("<color=green>방어!</color>");//힐아이템을 소진해서 방어
                            mystate.Inventory[4]--;
                        }
                        else//아니면 맞음
                        {
                            UC.MessagePrint("<color=red>공격당했습니다!</color>");
                            mystate.Inventory[0] = 0;
                            mystate.Inventory[2] = 0;
                            if (playerList[evnt.AttakingPlayer] == 0 && mystate.Inventory[1] == 1)//공격자가 광부이면 && 내가 금을 가지고 있으면
                            {
                                mystate.Inventory[1] = 0;//금도 뺏김
                                var evnt2 = GoldSteal.Create();
                                evnt2.Attacker = evnt.AttakingPlayer;
                                evnt2.Send(); //뺏었단 신호보냄(11/24버그픽스)
                            }
                            mystate.Blinded = true;
                            PlayerControl.blindCool = PlayerControl.blindCoolBase;
                            mystate.Paralyzed = true;
                            PlayerControl.paralyzeCool = PlayerControl.paralyzeCoolBase;
                        }
                    }
                    break;
                case 1://사보타지 색출
                    if (playerCode == evnt.TargetPlayer)
                    {
                        MessageCreate((playerNameList[evnt.AttakingPlayer] + "가 사보타지를 찾습니다").ToString());
                        var evnt2 = SabotageCaptured.Create();
                        evnt2.Player = playerCode;
                        if (isSabotage == true)
                        {
                            evnt2.isSabotage = true;
                        }
                        else
                        {
                            evnt2.isSabotage = false;
                        }
                        evnt2.Send();
                    }
                    break;
                case 2:// 도움을 받았을 때
                    if (playerCode == evnt.TargetPlayer)
                    {
                        mystate.Inventory[0] = PlayerControl.pickaxeAmountBase;
                        mystate.Inventory[2] = PlayerControl.barricadeBase;
                        mystate.Blinded = false;
                        PlayerControl.blindCool = 0;
                        mystate.Paralyzed = false;
                        PlayerControl.paralyzeCool = 0;
                    }
                    break;
            }
            

        }//유저끼리 시야방해, 이동불가, 곡괭이 파괴 등을 할때 콜백

        public override void OnEvent(MessageToAll evnt)
        {
            UC.MessagePrint(evnt.Message);
        }
        public override void OnEvent(SaveGold evnt)
        {
            playerList[evnt.Player] = 2;
            goldSaved++;
            UC.MessagePrint((playerNameList[evnt.Player] + "이(가) <color=yellow>코인</color>을 입금했습니다.").ToString());
        }//금 입금 콜백
        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true, true, evnt.Player);
        } //벽파괴 콜백

        public override void OnEvent(PlayerJoined evnt) // 플레이어 접속시 호출 접속한 플레이어에게 코드를 배정(키값-이름)
        {
            if(BoltNetwork.IsServer == true)
            {
                var code = PlayerCode.Create();
                code.Code = playerNum;
              //  if (gameStarted == true)
               // {
              //      code.Code = -1;
              //  }
                code.Name = evnt.PlayerName;
                code.Send();
                playerList[playerNum] = 0;
                playerNum++;
                //Debug.LogWarning(playerNum + " : 현재 플레이어 수");
            }
        }

        public override void OnEvent(PlayerCode evnt)//자신의 이름이 이벤트와 같다면 해당이벤트의 코드를 본인의 유저코드로설정(키값-이름)
        {
            if (mystate.PlayerName == evnt.Name)
            {
                //if (evnt.Code == -1)
              //  {
               //     BoltNetwork.Destroy(myPlayer);
               //     UC.GameWinner(-3, false, false, playerNameList);
              //  }
                //else
                {
                    playerCode = evnt.Code;
                    mystate.PlayerCode = playerCode;
                }
            }
        }

        public override void OnEvent(GameStart evnt)// 게임 시작 이벤트에 대한 콜백함수 일단 막고있는 벽을 없애는 용도 임시로 아이템 추가하는것도 넣음 호스트가 주체로 작동
        {
            time = evnt.TimeLeft;
            int _sabotage = evnt.Sabotage;
            UC.sabotagenum = evnt.Sabotage;
            playerNum = evnt.PlayerNum;
            PlayerControl.pickaxeAmountBase = (int)Math.Round((double)(100 / playerNum));
            mystate.Inventory[0] = PlayerControl.pickaxeAmountBase;
            mystate.Inventory[2] = PlayerControl.barricadeBase;
            if (gameStarted == false || BoltNetwork.IsServer)
            {
                for (int i = -3; i < 3; i++)
                {
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 - 3, false, true, -1);
                    MC.DestroyWall(MC.mapSize / 2 - 3, MC.mapSize / 2 + i, false, true, -1);
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 + 2, false, true, -1);
                    MC.DestroyWall(MC.mapSize / 2 + 2, MC.mapSize / 2 + i, false, true, -1);
                }
                if(BoltNetwork.IsServer)
                {
                    GoldCreate(GoldListCreate(playerNum));
                }
            }
            if(BoltNetwork.IsClient)
            {
                gameStarted = true;
                playerNum = evnt.PlayerNum;
            }
            UC.MessagePrint("게임이 시작되었습니다.");
            for(int i = 0; i<playerNum; i++)
            {
                playerList[i] = 0;
            }
            if (_sabotage != -1)
            {
                do
                {
                    playerList[_sabotage % 10] = 1;//해당 인덱스값을 받아서 1로만듬
                    //Debug.LogWarning("Player" + _sabotage % 10 + "is Sabotage");
                    _sabotage = _sabotage / 10;
                    sabotages++;
                } while (_sabotage != 0);
            }
            if (playerList[playerCode] == 1)//본인이 사보타지인지 확인하고 반영 기능
            {
                PlayerControl.canFindSabotage = false;
                PlayerControl.canUseFishing = true;
                PlayerControl.barricadeBase = 10;
                isSabotage = true;
                UC.MessagePrint("당신은 <color=red>사보타지</color>입니다");
                mystate.Inventory[3] = (int)Math.Floor((double)(playerNum - sabotages) / sabotages) * 2;
            }
            else
            {
                PlayerControl.canFindSabotage = true;
                PlayerControl.barricadeBase = 3;
                PlayerControl.canUseFishing = false;
                UC.MessagePrint("당신은 <color=green>광부</color>입니다");
            }
            var name = PlayerName.Create();
            name.Code = playerCode;
            name.Name = mystate.PlayerName;
            name.Send();
        }

        public override void OnEvent(WallCreated evnt)// 최초 생성이후 자잘한 바리케이트나 맵이벤트시 호출
        {
            if (playerCode != evnt.Player)
            {
                MC.CreateWall(evnt.Type, evnt.LocationX, evnt.LocationY, true);
            }
        }
        public override void OnEvent(ItemCreated evnt) // 아이템 생성 주체가 본인이 아니면 생성(본인이 보내고 콜백도받아서 중복생성방지)
        {
            if(playerCode != evnt.Player)
            {
                IC.CreateItem(evnt.LocationX, evnt.LocationY, evnt.Type, true);
            }
        }

        public override void OnEvent(ItemPicked evnt) // 아이템 주운것에 대한 콜백
        {
            IC.GetItem(evnt.LocationX, evnt.LocationY, null , true);
            if(evnt.Type == 1)
            {
                //UC.MessagePrint((playerNameList[evnt.Player] + "가 (" + evnt.LocationX + ", " + evnt.LocationY + ")에서 <color=yellow>코인</color>을 획득했습니다").ToString());
            }
        }

        public override void OnEvent(ItemUsed evnt)
        {
            return;
        }// 아이템 사용 함수(바리케이트, 곡괭이, 금에는 해당되지않음)

        public override void OnEvent(GameEnd evnt)//게임 종료이벤트 수신 함수
        {
            main.Stop();
            result.Play();
            TitleMenu.isGameEnd = true;
            TitleMenu.isNormal = true;
            gameEnded = true;
            bool _amIWin = false;
            int _temp = evnt.WinPlayer;
            if (evnt.WinPlayer != -1)
            {
                do
                {
                    if (_temp % 10 == playerCode)//우승자중 내 코드가 있다면
                    {
                        _amIWin = true;
                        break;
                    }
                    _temp = _temp / 10;
                } while (_temp != 0);
            }
            UC.GameWinner(evnt.WinPlayer, evnt.IsSabotageWin, _amIWin, playerNameList);
            isSabotage = false;
            playerCode = -1;
        }

        public override void OnEvent(PlayerName evnt) //게임 시작 후 유저 명단을 확정하는 함수
        {
            playerNameList[evnt.Code] = evnt.Name;
        }
        // Update is called once per frame
        void Update() 
        {
            if (time > 0 && time <= timeBase && gameStarted == false)
            {
                time -= Time.deltaTime;
            }
            else if (gameStarted == false)//여기서 호스트가 게임 시작 요청을 보냄
            {
                time = 1000;//update문 발동안되는 값
                if(BoltNetwork.IsServer)
                {
                    gameStarted = true;
                    //추후 플레이어 3인 이하일시 게임 종료기능 추가
                    var evnt = GameStart.Create();
                    evnt.TimeLeft = timeBase;
                    evnt.PlayerNum = playerNum;
                    evnt.Sabotage = DividePlayer();
                    evnt.Send();
                }
            }
            else if (time > 0 && time <= timeBase && gameStarted == true)//게임 시작했다는 이벤트를 호스트포함 모두가 받으면 실행
            {
                time -= Time.deltaTime;//실험때는 여기에 배수를 곱해서 게임 빠르게 진행
                if (goldSaved == goldAmount)// 만약 생성된 모든 금이 입금되었다면게임종료
                {
                    if (BoltNetwork.IsServer && goldAmount != 0)
                    {
                        time = 1000;//update문 발동안되는 값
                        GameEnd();
                    }
                }
            }
            else if (time <= 0 && gameStarted == true) // 게임 종료시
            {
                time = 1000;//update문 발동안되는 값
                if(BoltNetwork.IsServer)
                {
                    GameEnd();
                }
            }
        }

        public static void MessageCreate(string message)
        {
            var evnt = MessageToAll.Create();
            evnt.Message = message;
            evnt.Send();
        }
        int DividePlayer()//사보타지 코드를 리턴하는 함수 (ex 사보타지가 9, 4 ,3, 0 이면 9430 리턴, 없을시 -1)
        {
            lessSabotage = UnityEngine.Random.Range(0, 2);
            int _sabotage = (int)(playerNum * 0.43) - lessSabotage; //현재 접속인원에 맞춰서 현재인원/ 0.43 에서 랜덤으로 1을 빼거나 더해서 사보타지수를 구함
            int _code = 0;
            int _temp;
            List<int> _sabolist = new List<int>();
            bool _crash;

            if (playerNum < 3)
            {
                _sabotage = 0;
            }
            //Debug.LogWarning("총 사보타지" + _sabotage);
            if (_sabotage != 0)
            {
                for (int i = 0; i < _sabotage; i++)
                {
                    do
                    {
                        _crash = false;
                        _temp = UnityEngine.Random.Range(0, playerNum);
                        //Debug.LogWarning("랜덤" + _temp);
                        foreach (int previous in _sabolist)
                        {
                            if (previous == _temp)
                            {
                                _crash = true;
                                break;
                            }
                        }
                    } while (_crash == true);
                    _sabolist.Add(_temp);
                }
                _sabolist.Sort();//정렬후 역순으로 입력해서 플레이어0(방장)이 배열 맨첫번째로 와서 자릿수 안엉키도록 방지
                for (int i = _sabotage - 1; i >= 0 ; i--)
                {
                    _code = _code * 10 + _sabolist[i];
                }
            }
            else
            {
                _code = -1;//사보타지가 0일때
            }
            //Debug.LogWarning("코드" + _code); //사보타지코드
            return _code;
        }

        void GameEnd()//게임 종료이벤트 송신 함수
        {
            
            int _gold2win = (int)(playerNum*0.43) + 1 - lessSabotage;
            int _goldSavedPlayer = 0;
            int _winPlayer = 0;
            bool _isSabotageWin;

            for (int i = 0; i < playerNum; i++)
            {
                if(playerList[i] == 2)
                {
                    _goldSavedPlayer++;
                }
            }

            if (_goldSavedPlayer >= _gold2win && _goldSavedPlayer > 0)//광부 승리시
            {
                _isSabotageWin = false;
                for (int i = playerNum - 1; i >= 0; i--)
                {
                    if (playerList[i] == 2)//입금상태인 광부 측정
                    {
                        _winPlayer = _winPlayer * 10 + i;
                    }
                }
            }
            else//사보타지 혹은 승리한 인원이 없을 시
            {
                if (sabotages != 0)
                {
                    _isSabotageWin = true;
                    for (int i = playerNum - 1; i >= 0; i--)
                    {
                        if (playerList[i] == 1)//사보타지 수 측정
                        {
                            _winPlayer = _winPlayer * 10 + i;
                        }
                    }
                }
                else
                {
                    _isSabotageWin = false;//아무도 승리하지 못했을 시
                    _winPlayer = -1;
                }
            }
            var evnt = Photon.Bolt.GameEnd.Create();
            evnt.WinPlayer = _winPlayer;
            evnt.IsSabotageWin = _isSabotageWin;
            evnt.Send();
        }
        public List<int> GoldListCreate(int playerNum) // 금 생성을 위해 인원수에 맞게 중복되지않는 좌표리스트를 출력하는 함수
        {
            List<int> _goldList = new List<int>();
            int _temp;
            bool _crash;
            int _gold = (int)(playerNum * 0.43) + 1;
            if(playerNum<3)
            {
                _gold = 1;//1,2인 플레이용으로 일단 넣어놓음
            }
            goldAmount = _gold;
            for (int i = 0; i < _gold * 7; i++) 
            {
                do
                {
                    _crash = false;
                    _temp = (50 + UnityEngine.Random.Range(10, 49) * (-1 + (2 * UnityEngine.Random.Range(0, 2)))) * 100 + (50 + UnityEngine.Random.Range(10, 49) * (-1 + (2 * UnityEngine.Random.Range(0, 2))));//1~40 60~99의 x y 수를 선택

                    foreach (int previous in _goldList)
                    {
                        if (previous == _temp)
                        {
                            _crash = true;
                            break;
                        }
                    }
                } while (_crash == true);
                _goldList.Add(_temp);
            }

            //foreach (int golds in _goldList)
            //{
            //    Debug.LogWarning(golds)
            //}
            return _goldList;
        } 
        public void GoldCreate(List<int> list) // 입력 받은 리스트로 금을 생성하는 함수
        {
            int i = 0;
            //먼저 해당 리스트에 있는 벽들 다 부숨
            foreach (int item in list)
            {
                MC.DestroyWall(item / 100, item % 100, false, true, -1);
            }

            
            foreach (int item in list)
            {
                if (i < goldAmount) // 금생성
                {
                    //Debug.LogWarning("gold" + item);
                    IC.CreateItem(item / 100, item % 100, 1, false);
                }
                else if (i < goldAmount * 4)
                {
                    //Debug.LogWarning("hit" + item);
                    IC.CreateItem(item / 100, item % 100, 3, false);
                }
                else if (i < goldAmount * 6)
                {
                    //Debug.LogWarning("heal" + item);
                    IC.CreateItem(item / 100, item % 100, 4, false);
                }
                else if (i < goldAmount * 7) 
                {
                    //Debug.LogWarning("telescope" + item);
                    IC.CreateItem(item / 100, item % 100, 5, false);
                }
                else
                {
                    //Debug.LogWarning("empty" + item);
                }
                i++;
            }
            //부쉈던 벽들 보물상자 벽으로 새로 다 덮어버림
            foreach (int item in list)
            {
                MC.CreateWall(3, item / 100, item % 100, false);
            }
            MessageCreate("코인과 아이템들이 생성되었습니다.");
            MessageCreate(("코인은 총 " + goldAmount + "개 입니다").ToString());
        }


    }
}

