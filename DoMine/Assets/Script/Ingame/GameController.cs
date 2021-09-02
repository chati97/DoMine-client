using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using TMPro;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        public GameController GC;
        [SerializeField] ItemController IC = null;
        [SerializeField] MapController MC = null;
        [SerializeField] Text timeLeft = null;
        public int playerCode = -1;
        public int playerNum;
        float time;
        public List<BoltEntity> players = new List<BoltEntity>();
        public BoltEntity myPlayer;
        bool gameStarted = false;

        public override void SceneLoadLocalDone(string scene, IProtocolToken token)
        {
            var spawnPos = new Vector3(UnityEngine.Random.Range(48, 51), UnityEngine.Random.Range(48, 51), 0);
            myPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPos, Quaternion.identity);
            myPlayer.TakeControl();
            MC.player = myPlayer;//Mc에 넣음
            IC.player = myPlayer;//Ic에 넣음
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

        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true, true);
        }

        public override void OnEvent(PlayerJoined evnt)
        {
            IPlayerState state;
            playerNum = players.Count;
            Debug.LogWarning(players.Count + " : 현재 플레이어 수");
            foreach(BoltEntity entity in players)
            {
                entity.TryFindState<IPlayerState>(out state);//신기한 함수 플레이어가 접속했을때 인원 추가하고 볼트엔티티 로그 남기는 기능
                Debug.LogWarning("[" + state.PlayerName + "] playing");
            }
            
        }

        public override void OnEvent(GameInfo evnt)// 게임 시작 이벤트에 대한 콜백함수 일단 막고있는 벽을 없애는 용도 임시로 아이템 추가하는것도 넣음
        {
            time = evnt.TimeLeft;
            gameStarted = true;
            
            if(BoltNetwork.IsServer)
            {
                for (int i = -3; i < 3; i++)
                {
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 - 3, false, true);
                    MC.DestroyWall(MC.mapSize / 2 - 3, MC.mapSize / 2 + i, false, true);
                    MC.DestroyWall(MC.mapSize / 2 + i, MC.mapSize / 2 + 2, false, true);
                    MC.DestroyWall(MC.mapSize / 2 + 2, MC.mapSize / 2 + i, false, true);
                }
                IC.CreateItem(48, 52, 1, false);
                IC.CreateItem(49, 52, 1, false);
                IC.CreateItem(50, 52, 1, false);
                IC.CreateItem(51, 52, 1, false);
            }
        }

        public override void OnEvent(WallCreated evnt)
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

        public override void OnEvent(ItemPicked evnt)
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
                timeLeft.text = "게임시작까지" + ((int)Math.Floor(time)).ToString();
            }
            else if (gameStarted == false)
            {
                time = 1000;
                timeLeft.text = "게임 준비중";
                var evnt = GameInfo.Create();
                evnt.TimeLeft = 900;
                evnt.PlayerNum = players.Count;
                evnt.Send();
            }
            else if (time > 0 && time <= 900 && gameStarted == true)
            {
                time -= Time.deltaTime;
                timeLeft.text = "게임종료까지" + ((int)Math.Floor(time) / 60).ToString() + " : " + ((int)Math.Floor(time) % 60).ToString();
            }
            else if (time <= 0 && gameStarted == true)
            {
                time = 0;
                timeLeft.text = "게임종료";
            }
        }


    }
}

