using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Bolt;

namespace DoMine
{
    public class MapController : MonoBehaviour
    {
        public int[] mapArray = new int[10000];
        public GameObject[] mapObject = new GameObject[10000];
        public GameObject player = null;
        [SerializeField] UIController UC;
        [SerializeField] GameObject breakable = null;
        [SerializeField] GameObject unbreakable = null;
        [SerializeField] GameObject chest = null;
        [SerializeField] GameObject barricade = null;
        [SerializeField] Transform wallParent = null;
        [SerializeField] Transform chestParent = null;
        [SerializeField] GameObject wallIndicator = null;
        [SerializeField] GameObject chestSensor = null;
        public List<int> goldChest = new List<int>();
        public GameObject nearestWall = null;
        public int nearestWallX = -1;
        public int nearestWallY = -1;
        public int mapSize = 100;
        bool chestNearby;
        private void Start()
        {

        }

        private void Update()
        {

        }

        //정해진 맵 배열을 만드는 함수 현재는 맨끝만 만들었지만 이후 게임 방 정보에 따라 매개변수를 받아 다른종류의 맵을 만들도록 할 예정
        public int[] MakeMapArr()
        {
            int[] mapArray = new int[mapSize * mapSize];
            int half = mapSize / 2;
            for(int i = 0; i< mapSize; i++)
            {
                mapArray[i] = 2;
                mapArray[(mapSize-1)*100 + i] = 2;
                mapArray[i*100] = 2;
                mapArray[i*100 + (mapSize-1)] = 2;
            }
            for(int i = 1; i< mapSize-1; i++)
            {
                for(int j = 1; j < mapSize-1; j++)
                {
                    mapArray[i*100 + j] = 1; //나머지는 부서지는벽으로설정
                }
            }
            for(int i = -3; i < 3; i++)//게임 시작전 4*4크기를 감싸서 막는 용
            {
                mapArray[(half - 3) * 100 + half + i] = 2;
                mapArray[(half + 2) * 100 + half + i] = 2;
                mapArray[(half + i) * 100 + half - 3] = 2;
                mapArray[(half + i) * 100 + half + 2] = 2;
            }
            for (int i = -2; i < 2; i++)//중앙 4*4 공간
            {
                mapArray[(half+i) * 100 + half - 2] = 0;
                mapArray[(half+i) * 100 + half - 1] = 0;
                mapArray[(half+i) * 100 + half] = 0;
                mapArray[(half+i) * 100 + half + 1] = 0;
            }
            return mapArray;
        }


        // CreateMap 맵 정보를 받아 최초 맵생성 생성하는 정보만 있음
        public void CreateMap(int[] mapArray, GameObject[] mapObject)
        {
            for(int i = 0; i< mapSize; i++)
            {
                for(int j = 0; j< mapSize; j++)
                {
                    switch(mapArray[i*100 + j])
                    {
                        case 0:
                            break;
                        case 1:
                            mapObject[i * 100 + j] = Instantiate(breakable, new Vector2(i, j), Quaternion.identity, wallParent);
                            break;
                        case 2:
                            mapObject[i * 100 + j] = Instantiate(unbreakable, new Vector2(i, j), Quaternion.identity, wallParent);
                            break;
                        case 3:
                            mapObject[i * 100 + j] = Instantiate(chest, new Vector2(i, j), Quaternion.identity, wallParent);
                            break;
                    }
                }
            }
        }

        public int CreateWall(int type, int x, int y, bool callback)
        {
            GameObject _targetItem = null;
            Transform _targetParent = null;//추후 있을 상자 탐색시 최적화위해 추가
            if (mapArray[x * 100 + y] == 0)
            {
                switch (type)
                {
                    case 0:
                        break;
                    case 1:
                        _targetItem = breakable;
                        _targetParent = wallParent;
                        mapArray[x * 100 + y] = 1;
                        break;
                    case 2:
                        _targetItem = unbreakable;
                        _targetParent = wallParent;
                        mapArray[x * 100 + y] = 2;
                        break;
                    case 3:
                        _targetItem = chest;
                        _targetParent = chestParent;
                        mapArray[x * 100 + y] = 3;
                        break;
                    case 4:
                        _targetItem = barricade;
                        _targetParent = wallParent;
                        mapArray[x * 100 + y] = 4;
                        break;
                }
                mapObject[x * 100 + y] = Instantiate(_targetItem, new Vector2(x, y), Quaternion.identity, _targetParent);
                if (callback == false) //콜백이 false일시(본인이 처음 보내는거면)
                {
                    var evnt = WallCreated.Create();
                    evnt.Type = type;
                    evnt.LocationX = x;
                    evnt.LocationY = y;
                    evnt.Player = GameController.playerCode;
                    evnt.Send();
                }
                return 0;
            }
            else
            {
                return 1;
            }
        }


        //DestroyWall 부술수있는 벽을 부술때 호출되는 함수
        public void DestroyWall(int x, int y, bool callback, bool system, int playerCode) //callback이 false면 이벤트생성
        {
            if (playerCode != GameController.playerCode)
            {
                if (mapArray[x * 100 + y] != 2 || system == true)
                {
                    Destroy(mapObject[x * 100 + y]);
                    mapObject[x * 100 + y] = null;
                    if (callback == false) //콜백이 false일시(본인이 처음 보내는거면)
                    {
                        //Debug.Log(nearestWallX + "," + nearestWallY);
                        var evnt = WallDestoryed.Create();
                        evnt.LocationX = x;
                        evnt.LocationY = y;
                        evnt.Player = GameController.playerCode;
                        evnt.Send();
                        if (mapArray[x * 100 + y] == 3)//만약 상자를 부술때
                        {
                            foreach (int gold in goldChest)//goldchest에 있는 항목이면
                            {
                                if (gold == x * 100 + y)//전체메시지 발송
                                {
                                    GameController.MessageCreate("누군가 (" + x + ", " + y + ")에서 <color=yellow>코인</color>을 발견했습니다");
                                }
                            }
                        }
                    }
                    mapArray[x * 100 + y] = 0;//코인 발견 메시지 때문에 일단 뒤로 빼둠
                }
            }
            
            
        }


        //FindWall 가장 가까운 부술수 있는 벽을 표시
        public void FindWall(GameObject[] mapObject)
        {
            float _nearestDistance = 10000;
            float _sampleDistance;
            Vector2 _nearestVector = new Vector2(0, 0);
            int _currentPositionX = (int)Math.Ceiling(player.transform.position.x);
            int _currentPositionY = (int)Math.Ceiling(player.transform.position.y);
            for (int i = _currentPositionX - 1; i < _currentPositionX + 1; i++)// 본인둘러싼 총9칸을 비교하는 수 기존 10000개 다 search하던 것에서 최적화
            {
                for (int j = _currentPositionY - 1; j < _currentPositionY + 1; j++)
                {
                    if (mapObject[i * 100 + j] != null)
                    {
                        _sampleDistance = Vector2.Distance(player.transform.position, mapObject[i * 100 + j].transform.position);
                        if (_nearestDistance > _sampleDistance)
                        {
                            _nearestDistance = _sampleDistance;
                            _nearestVector = mapObject[i * 100 + j].transform.position;
                            nearestWall = mapObject[i * 100 + j];
                            nearestWallX = i;
                            nearestWallY = j;
                        }
                    }

                }
            }
            if (Vector2.Distance(_nearestVector, player.transform.position) < 0.8)
            {
                wallIndicator.gameObject.SetActive(true);
                wallIndicator.transform.position = _nearestVector;
            }
            else
            {
                wallIndicator.gameObject.SetActive(false);
            }
        }
        
        public void FindChest()//상자가 근처에 있으면 신호하는 함수
        {
            int i=0;
            Transform nearestChest;
            float nearestDistance = 10000;
            float tempDistance;
            foreach (Transform chest in chestParent)
            {
                tempDistance = Vector2.Distance(player.transform.position, chest.transform.position);
                if(tempDistance < 20)
                {
                    chestNearby = true;
                    i++;
                    if (nearestDistance > tempDistance)
                    {
                        nearestDistance = tempDistance;
                        nearestChest = chest;
                    }
                }
            }
            if (i == 0)
            {
                chestNearby = false;
            }
            if(chestNearby)
            {
                chestSensor.gameObject.SetActive(true);
                chestSensor.transform.position = player.transform.position + new Vector3(0,0.25f,0);
                if(nearestDistance < 5)//(302010서 20105로 축소)
                {
                    chestSensor.gameObject.GetComponent<Renderer>().material.color = Color.blue;
                }
                else if (nearestDistance < 10)
                {
                    chestSensor.gameObject.GetComponent<Renderer>().material.color = Color.green;
                }
                else if (nearestDistance < 15)
                {
                    chestSensor.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    chestSensor.gameObject.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            else
            {
                chestSensor.gameObject.SetActive(false);
            }

        }
    }
}

