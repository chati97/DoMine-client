using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Bolt;

namespace DoMine
{
    public class MapController : MonoBehaviour
    {
        public int[,] mapArray = new int[100,100];
        public GameObject[,] mapObject = new GameObject[100, 100];
        [SerializeField] GameObject player = null;
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
            FindWall(mapObject);
        }

        //MakeMapArr 맵 배열을 생성하는 함수 나중엔 서버에서 생성할것or호스트가 생성
        //현재 함수는 중간 공간을 제외한 모든 공간에 부술수있는벽을 채우고 맨끝은 부서지지 않는 벽으로 채움
        //추후에 변수값을 받아서 원하는 크기로 맵을 만들게 할 예정
        public int[,] MakeMapArr()
        {
            int[,] mapArray = new int[mapSize, mapSize];
            for(int i = 0; i< mapSize; i++)
            {
                mapArray[0, i] = 2;
                mapArray[mapSize-1, i] = 2;
                mapArray[i, 0] = 2;
                mapArray[i, mapSize-1] = 2;
            }
            for(int i = 1; i< mapSize-1; i++)
            {
                for(int j = 1; j < mapSize-1; j++)
                {
                    mapArray[i, j] = 1; //나머지는 부서지는벽으로설정
                }
            }
            mapArray[mapSize / 2 - 1, mapSize / 2 - 1] = 0;
            mapArray[mapSize / 2 - 1, mapSize / 2] = 0;
            mapArray[mapSize / 2, mapSize / 2 - 1] = 0;
            mapArray[mapSize / 2, mapSize / 2] = 0;
            return mapArray;
        }


        // CreateMap 맵 정보를 받아 최초 맵생성 생성하는 정보만 있음
        public void CreateMap(int[,] mapArray, ref GameObject[,] mapObject)
        {
            for(int i = 0; i< mapSize; i++)
            {
                for(int j = 0; j< mapSize; j++)
                {
                    switch(mapArray[i,j])
                    {
                        case 0:
                            break;
                        case 1:
                            mapObject[i, j] = BoltNetwork.Instantiate(BoltPrefabs.Wall2, new Vector2(i, j), Quaternion.identity);
                            break;
                        case 2:
                            mapObject[i, j] = BoltNetwork.Instantiate(BoltPrefabs.Wall, new Vector2(i, j), Quaternion.identity);
                            break;
                    }
                    BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector2(i, j), Quaternion.identity, );
                }
            }
        }


        //UpdateMap 추후구현 게임 중간 중간 맵 상태를 업데이트함
        //다른플레이어가 중간에 벽을 파괴 생성할때도 벽을 파괴하는 신호를 날리고 받는 함수도 있을거지만
        //중간에 통신오류로 플레이어간 꼬일거를 예측해서 전체적으로 한번 로딩하는 함수
        public void UpdateMap(int[,] mapArray, ref GameObject[,] mapObject)
        {
            foreach (int type in mapArray)
            {
                switch(type)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;

                }

            }
        }


        //DestroyWall 부술수있는 벽을 부술때 호출되는 함수
        public void DestroyWall(int x, int y)
        {
            Debug.Log(mapArray[x, y]);
            if (mapArray[x,y] == 1)
            {
                Destroy(mapObject[x, y]);
                mapArray[x, y] = 0;
                Debug.Log(nearestWallX + "," + nearestWallY);
            }
            
        }


        //FindWall 가장 가까운 부술수 있는 벽을 표시
        public void FindWall(GameObject[,] mapObject)
        {
            float _nearestDistance = 10000;
            float _sampleDistance;
            Vector2 _nearestVector = new Vector2(0, 0);
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (mapObject[i, j] != null)
                    {
                        _sampleDistance = Vector2.Distance(player.transform.position, mapObject[i, j].transform.position);
                        if (_nearestDistance > _sampleDistance)
                        {
                            _nearestDistance = _sampleDistance;
                            _nearestVector = mapObject[i, j].transform.position;
                            nearestWall = mapObject[i, j];
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

