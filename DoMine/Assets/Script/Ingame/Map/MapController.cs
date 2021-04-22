using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class MapController : MonoBehaviour
    {
        int[,] mapArray = new int[100,100];
        GameObject[,] mapObject = new GameObject[100, 100];
        [SerializeField] GameObject breakable;
        [SerializeField] GameObject unbreakable;
        [SerializeField] Transform wallParent;

        private void Start()
        {
            CreateMap(MakeMapArr() ,ref mapObject);
        }


        public int[,] MakeMapArr()
        {
            int[,] mapArray = new int[100, 100];
            for(int i = 0; i<100; i++)
            {
                mapArray[0, i] = 2;
                mapArray[99, i] = 2;
                mapArray[i, 0] = 2;
                mapArray[i, 99] = 2;
            }
            for(int i = 1; i<99; i++)
            {
                for(int j = 1; j < 99; j++)
                {
                    mapArray[i, j] = 1; //나머지는 부서지는벽으로설정
                }
            }
            mapArray[49, 49] = 0;
            mapArray[49, 50] = 0;
            mapArray[50, 49] = 0;
            mapArray[50, 50] = 0;
            return mapArray;
        }

        public void CreateMap(int[,] mapArray, ref GameObject[,] mapObject)
        {
            for(int i = 0; i<100; i++)
            {
                for(int j = 0; j<100; j++)
                {
                    switch(mapArray[i,j])
                    {
                        case 0:
                            break;
                        case 1:
                            mapObject[i, j] = Instantiate(breakable, new Vector2(i, j), Quaternion.identity, wallParent);
                            break;
                        case 2:
                            mapObject[i, j] = Instantiate(unbreakable, new Vector2(i, j), Quaternion.identity, wallParent);
                            break;
                    }
                }
            }
        }
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
        
    }
}

