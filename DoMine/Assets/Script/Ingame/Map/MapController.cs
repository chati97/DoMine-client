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
        public int playerCode;
        [SerializeField] GameObject breakable = null;
        [SerializeField] GameObject unbreakable = null;
        [SerializeField] Transform wallParent = null;
        [SerializeField] GameObject wallIndicator = null;
        public GameObject nearestWall = null;
        public int nearestWallX = -1;
        public int nearestWallY = -1;
        int mapSize = 100;
        public int isHost = 0;
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
            mapArray[(mapSize / 2 - 1)*100 + mapSize / 2 - 1] = 0;
            mapArray[(mapSize / 2 - 1)* 100 + mapSize / 2] = 0;
            mapArray[(mapSize / 2)* 100 + mapSize / 2 - 1] = 0;
            mapArray[(mapSize / 2)* 100 + mapSize / 2] = 0;
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
                    }
                }
            }
        }

        public void CreateWall(GameObject[] mapObject ,int type, int x, int y, bool callback)
        {
            if(mapObject[x*100+y] != null)
            {
                switch (type)
                {
                    case 0:
                        break;
                    case 1:
                        mapObject[x * 100 + y] = Instantiate(breakable, new Vector2(x, y), Quaternion.identity, wallParent);
                        mapArray[x * 100 + y] = 1;
                        break;
                    case 2:
                        mapObject[x * 100 + y] = Instantiate(unbreakable, new Vector2(x, y), Quaternion.identity, wallParent);
                        mapArray[x * 100 + y] = 2;
                        break;

                }
                if (callback == false) //콜백이 false일시(본인이 처음 보내는거면)
                {
                    var evnt = WallCreated.Create();
                    evnt.Type = type;
                    evnt.LocationX = x;
                    evnt.LocationY = y;
                    evnt.Player = playerCode;
                    evnt.Send();
                }
            }
        }


        //DestroyWall 부술수있는 벽을 부술때 호출되는 함수
        public void DestroyWall(int x, int y, bool callback) //callback이 false면 이벤트생성
        {
            Debug.Log(mapArray[x * 100 + y]);
            if (mapArray[x * 100 + y] == 1)
            {
                Destroy(mapObject[x * 100 + y]);
                mapArray[x * 100 + y] = 0;
                Debug.Log(nearestWallX + "," + nearestWallY);

                if(callback == false) //콜백이 false일시(본인이 처음 보내는거면)
                {
                    var evnt = WallDestoryed.Create();
                    evnt.LocationX = x;
                    evnt.LocationY = y;
                    evnt.Send();
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
    }
}

