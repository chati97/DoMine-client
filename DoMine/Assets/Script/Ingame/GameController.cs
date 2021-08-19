using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

namespace DoMine
{
    public class GameController : GlobalEventListener
    {
        [SerializeField] ItemController IC = null;
        [SerializeField] MapController MC = null;
        [SerializeField] Text timeLeft = null;
        public int playerCode = -1;
        public int playerNum;
        float time;

        // Start is called before the first frame update
        void Start()
        {
            if (BoltNetwork.IsServer)
            {
                time = 600;
                playerNum = 1;
                playerCode = 0;
            }
            MC.CreateMap(MC.mapArray = MC.MakeMapArr(), MC.mapObject);
            IC.Init(playerNum);
        }

        public override void OnEvent(WallDestoryed evnt)
        {
            MC.DestroyWall(evnt.LocationX, evnt.LocationY, true);
        }

        public override void OnEvent(PlayerJoined evnt)
        {
            if(BoltNetwork.IsServer)
            {
                var reply = GameInfo.Create();
                reply.TimeLeft = time;
                playerNum++;
                reply.PlayerNum = playerNum;
                reply.Send();
                IC.CreateItem(48 + playerNum, 50, 1, false);// 현재 인게임씬연결이 동시에 되지 않으니 아이템 상호작용확인차 넣은 임시코드
                IC.CreateItem(49 + playerNum, 50, 1, false);
            }            
        }

        public override void OnEvent(GameInfo evnt)
        {
            time = evnt.TimeLeft;
            playerNum = evnt.PlayerNum;  //현재 플레이어 동시시작이 안되어서 일단 오는사람 받을때 정보 건네주는 역할, 나중에 로비 개발이후 한번에 접속인원 넘겨받을시에 수정
            if(playerCode == -1)
            {
                playerCode = evnt.PlayerNum - 1;
                IC.playerCode = playerCode;
                MC.playerCode = playerCode;
            }
            Debug.Log("[" + playerNum + "," +  playerCode + "]");
            
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
            time -= Time.deltaTime;
            timeLeft.text = ((int)Math.Floor(time)/60).ToString() + " : " + ((int)Math.Floor(time) % 60).ToString();
        }


    }
}

