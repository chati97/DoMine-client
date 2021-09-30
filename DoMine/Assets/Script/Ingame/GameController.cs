using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        [SerializeField] ItemController IC = null;
        [SerializeField] MapController MC = null;
        [SerializeField] UIController UC = null;
        public int[] playerList = {0,-1,-1,-1,-1,-1,-1,-1,-1,-1};//최대 10인 입장, 인덱스는 플레이어코드(-1 없음, 0 광부, 1 사보타지, 2는 입금한광부..?)
        public static int playerCode = -1;
        public int playerNum;
        public static float time;
        public List<BoltEntity> players = new List<BoltEntity>();
        public BoltEntity myPlayer;
        public static bool isSabotage = false;
        public static bool gameStarted = false;
        public static bool gameLoaded = false;
        IPlayerState mystate = null;

        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector3(UnityEngine.Random.Range(48, 51), UnityEngine.Random.Range(48, 51), 0);
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
            if (BoltNetwork.IsServer)
            {
                playerNum = 1;
                playerCode = 0;
            }
            time = 5;
            MC.CreateMap(MC.mapArray = MC.MakeMapArr(), MC.mapObject);
            IC.Init(0);
        }
        public override void OnEvent(SaveGold evnt)
        {
            playerList[evnt.Player] = 2;
            Debug.LogWarning("Player" + evnt.Player + " Saved Gold");
        }
        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true, true);
        }

        public override void OnEvent(PlayerJoined evnt) // 플레이어 접속시 호출 접속한 플레이어에게 코드를 배정
        {
            IPlayerState state;
            var code = PlayerCode.Create();
            code.Code = playerNum;
            code.Send();
            playerList[playerNum] = 0;
            playerNum++;
            Debug.LogWarning(playerNum + " : 현재 플레이어 수");
            foreach (BoltEntity entity in players)
            {
                entity.TryFindState<IPlayerState>(out state);//신기한 함수 플레이어가 접속했을때 인원 추가하고 볼트엔티티 로그 남기는 기능
                //Debug.LogWarning("[" + state.PlayerName + "] playing");
            }
        }

        public override void OnEvent(PlayerCode evnt)//자신의 코드값이 -1(최초)값이면 보낸 코드값대로 자신의 코드를 설정
        {
            if(playerCode == -1)
            {
                playerCode = evnt.Code;
            }
        }

        public override void OnEvent(GameStart evnt)// 게임 시작 이벤트에 대한 콜백함수 일단 막고있는 벽을 없애는 용도 임시로 아이템 추가하는것도 넣음
        {
            time = evnt.TimeLeft;
            int _sabotage = evnt.Sabotage;
            if(gameStarted == false || BoltNetwork.IsServer)
            {
                for (int i = -3; i < 3; i++)
                {
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 - 3, false, true);
                    MC.DestroyWall(MC.mapSize / 2 - 3, MC.mapSize / 2 + i, false, true);
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 + 2, false, true);
                    MC.DestroyWall(MC.mapSize / 2 + 2, MC.mapSize / 2 + i, false, true);
                }
                if(BoltNetwork.IsServer)
                {
                    IC.CreateItem(48, 52, 1, false);
                    IC.CreateItem(49, 52, 1, false);
                    IC.CreateItem(50, 52, 1, false);
                    IC.CreateItem(51, 52, 1, false);
                }
            }
            if(BoltNetwork.IsClient)
            {
                gameStarted = true;
            }
            if(_sabotage != -1)
            {
                do
                {
                    playerList[_sabotage % 10] = 1;//해당 인덱스값을 받아서 1로만듬
                    Debug.LogWarning("Player" + _sabotage % 10 + "is Sabotage");
                    _sabotage = _sabotage / 10;
                } while (_sabotage != 0);
            }
            if (playerList[playerCode] == 1)//본인이 사보타지인지 확인하고 반영 기능
            {
                isSabotage = true;
                Debug.LogWarning("you are Sabotage");
                mystate.Inventory[2] = 10;//사보타지일 경우엔 바리케이트 10개 지급
            }
            else
            {
                Debug.LogWarning("you are Miner");
            }
        }

        public override void OnEvent(WallCreated evnt)// 최초 생성이후 자잘한 바리케이트나 맵이벤트시 호출
        {
            if (playerCode != evnt.Player)
            {
                MC.CreateWall(MC.mapObject, evnt.Type, evnt.LocationX, evnt.LocationY, true);
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
        }

        public override void OnEvent(ItemUsed evnt)
        {
            return;
        }

        // Update is called once per frame
        void Update() 
        {
            if (time > 0 && time <= 900 && gameStarted == false)
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
                    evnt.TimeLeft = 900;
                    evnt.PlayerNum = playerNum;
                    evnt.Sabotage = DividePlayer();
                    evnt.Send();
                }
            }
            else if (time > 0 && time <= 900 && gameStarted == true)//게임 시작했다는 이벤트를 호스트포함 모두가 받으면 실행
            {
                time -= Time.deltaTime;
            }
            else if (time <= 0 && gameStarted == true) // 게임 종료시
            {
                time = 1000;//update문 발동안되는 값
            }
        }

        int DividePlayer()//사보타지 코드를 리턴하는 함수 (ex 사보타지가 9, 4 ,3, 0 이면 9430 리턴, 없을시 -1)
        {
            int _sabotage = (int)(playerNum * 0.43) - UnityEngine.Random.Range(0,1); //현재 접속인원에 맞춰서 현재인원/ 0.43 에서 랜덤으로 1을 빼거나 더해서 사보타지수를 구함
            int _code = 0;
            int _temp = -1;
            List<int> _sabolist = new List<int>();
            bool _crash = false;

            if (playerNum < 3)
            {
                _sabotage = 0;
            }
            Debug.LogWarning("총 사보타지" + _sabotage);
            if (_sabotage != 0)
            {
                for (int i = 0; i < _sabotage; i++)
                {
                    do
                    {
                        _crash = false;
                        _temp = UnityEngine.Random.Range(0, playerNum - 1);
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
            Debug.LogWarning("코드" + _code);
            return _code;
        }

        void GameEnd()//게임 종료 함수
        {

        }

    }
}

