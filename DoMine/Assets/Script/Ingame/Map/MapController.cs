using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class MapController : MonoBehaviour
    {
        int[,] mapArray = new int[100,100];
        [SerializeField] GameObject breakable;
        [SerializeField] GameObject unreakable;

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
            for(int i = 1; i<100; i++)
            {
                for(int j = 1; j < 100; j++)
                {
                    mapArray[i, j] = 1; //나머지는 부서지는벽으로설정
                }
            }
            mapArray[49, 49] = 0;
            return mapArray;
        }

        public void AddMap(int[,] mapArray)
        {
            foreach(int type in mapArray)
            {
                switch (type)
                {
                    case 0:
                        break;
                    case 1:
                        //부서지는 벽 생성
                        break;
                    case 2:
                        //안부서지는 벽 생성
                        break;

                }   
            }
        }
        
    }
}

