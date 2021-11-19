using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

namespace DoMine
{
    public class ItemController : MonoBehaviour
    {
        public int[] itemArray = new int[10000];
        [SerializeField] MapController MC = null;
        [SerializeField] GameController GC = null;
        [SerializeField] UIController UC = null;
        public GameObject player = null;
        public GameObject[] itemObject = new GameObject[10000];
        [SerializeField] GameObject gold = null;
        [SerializeField] GameObject hit = null;
        [SerializeField] GameObject heal = null;
        [SerializeField] GameObject telescope = null;
        [SerializeField] Transform itemParent = null;
        [SerializeField] GameObject itemIndicator = null;
        public GameObject nearestItem = null;
        public int nearestItemX = -1;
        public int nearestItemY = -1;

        // 인벤토리는 0=곡괭이, 1=금, 2=바리케이트, 3=뒤통수, 4=치료, 5=망원경
        // 맵정보에서 0=공백, 1=금, 2=바리케이트  , 3=뒤통수, 4=치료, 5=망원경

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < 10000; i++)//1차로 비우는 절차
            {
                itemArray[i] = 0;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        

        public string GetItem(int x, int y, IPlayerState player, bool callback)//playercontrol에서 호출시엔 인벤토리를 늘려주고 이벤트날리면서 받음, 콜백으로 GC에서 호출시엔 단순 아이템삭제
        {
            int _type = itemArray[x * 100 + y];
            string output = null;

            if(callback == false)
            {
                switch (itemArray[x * 100 + y])
                {
                    case 1:
                        if (player.Inventory[1] == 0 && GC.playerList[GameController.playerCode] == 0)//입금안한 광부여야만 획득가능
                        {
                            player.Inventory[1] = 1;
                            output = "<color=yellow>코인</color>을 획득했습니다.";
                        }
                        else
                        {
                            //추후 여기에서 이미 확보해둔상태라고 UI에서 메세지 보내는거로 호출
                            return "금을 획득할 수 없습니다.";
                        }
                        break;
                    case 3:
                        player.Inventory[3]++;
                        output = " <color=red>무기</color>를 획득했습니다.";
                        break;
                    case 4:
                        player.Inventory[4]++;
                        output = "<color=green>구급상자</color>를 획득했습니다.";
                        break;
                    case 5:
                        player.Inventory[5] = 1;
                        output = "지도를 획득했습니다.";
                        break;
                }
                var evnt = ItemPicked.Create();
                evnt.LocationX = x;
                evnt.LocationY = y;
                evnt.Type = _type;
                evnt.Player = GameController.playerCode;
                evnt.Send();
            }

            Destroy(itemObject[x * 100 + y]);
            itemArray[x * 100 + y] = 0;
            return output;
        }

        // instantiate로 만들고 list에 삽입
        public void CreateItem(int x, int y, int itemCode, bool callback)
        {
            GameObject _targetItem = null;
            switch (itemCode)
            {
                case 0:
                    break;
                case 1:
                    _targetItem = gold;
                    itemArray[x * 100 + y] = 1;
                    MC.goldChest.Add(x * 100 + y);
                    UC.goldLocation.text = UC.goldLocation.text + "(" + x + "," + y + ") ";
                    UC.goldLocation2.text = UC.goldLocation2.text + "\n (" + x + "," + y + ")";
                    break;
                case 2:
                    break;
                case 3:
                    _targetItem = hit;
                    itemArray[x * 100 + y] = 3;
                    break;
                case 4:
                    _targetItem = heal;
                    itemArray[x * 100 + y] = 4;
                    break;
                case 5:
                    _targetItem = telescope;
                    itemArray[x * 100 + y] = 5;
                    break;
            }
            itemObject[x * 100 + y] = Instantiate(_targetItem, new Vector2(x, y), Quaternion.identity, itemParent);
            if (callback == false)
            {
                var evnt = ItemCreated.Create();
                evnt.LocationX = x;
                evnt.LocationY = y;
                evnt.Type = itemCode;
                evnt.Player = GameController.playerCode;
                evnt.Send();
            }
        }

        public void FindItem(GameObject[] itemObject)//mapController와 동일한 알고리즘 기존 아이템 리스트전체를 탐색하는 방식이었지만 아이템 구현방식을 map구현과 동일하게 수정하면서 같이 수정됨
        {
            float _nearestDistance = 10000;
            float _sampleDistance;
            Vector2 _nearestVector = new Vector2(0, 0);
            int _currentPositionX = (int)Math.Ceiling(player.transform.position.x);
            int _currentPositionY = (int)Math.Ceiling(player.transform.position.y);
            for (int i = _currentPositionX - 1; i < _currentPositionX + 1; i++)
            {
                for (int j = _currentPositionY - 1; j < _currentPositionY + 1; j++)
                {
                    if (itemObject[i * 100 + j] != null)
                    {
                        _sampleDistance = Vector2.Distance(player.transform.position, itemObject[i * 100 + j].transform.position);
                        if (_nearestDistance > _sampleDistance)
                        {
                            _nearestDistance = _sampleDistance;
                            _nearestVector = itemObject[i * 100 + j].transform.position;
                            nearestItem = itemObject[i * 100 + j];
                            nearestItemX = i;
                            nearestItemY = j;
                        }
                    }

                }
            }
            if (Vector2.Distance(_nearestVector, player.transform.position) < 0.8)
            {
                itemIndicator.gameObject.SetActive(true);
                itemIndicator.transform.position = _nearestVector;
            }
            else
            {
                itemIndicator.gameObject.SetActive(false);
            }
        }
    }
}

