using System;
using UnityEngine;
using Photon.Bolt;

namespace DoMine
{
    public class ItemController : MonoBehaviour
    {
        public int[] itemArray = new int[10000];
        public GameObject player = null;
        public GameObject[] itemObject = new GameObject[10000];
        [SerializeField] GameObject gold = null;
        [SerializeField] Transform itemParent = null;
        [SerializeField] GameObject itemIndicator = null;
        public GameObject nearestItem = null;
        public int nearestItemX = -1;
        public int nearestItemY = -1;

        // 인벤토리는 0=곡괭이, 1=금, 2=바리케이트
        // 맵정보에서 0=공백, 1=금, 2=바리케이트  

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(int playernum)
        {
            for(int i = 0 ; i < 10000; i++)
            {
                itemArray[i] = 0;
            }
        }

        public void GetItem(int x, int y, IPlayerState player, bool callback)//playercontrol에서 호출시엔 인벤토리를 늘려주고 이벤트날리면서 받음, 콜백으로 GC에서 호출시엔 단순 아이템삭제
        {
            int _type = itemArray[x * 100 + y];

            if(callback == false)
            {
                switch (itemArray[x * 100 + y])
                {
                    case 1:
                        if (player.Inventory[1] == 0 && GameController.isSabotage == false)
                        {
                            player.Inventory[1] = 1;
                            Debug.LogWarning("Gold get");
                        }
                        else
                        {
                            Debug.LogWarning("you cannot pick gold");//추후 여기에서 이미 확보해둔상태라고 UI에서 메세지 보내는거로 호출
                            return;
                        }
                        break;
                }
                var evnt = ItemPicked.Create();
                evnt.LocationX = x;
                evnt.LocationY = y;
                evnt.Type = _type;
                evnt.Send();
            }

            Destroy(itemObject[x * 100 + y]);
            itemArray[x * 100 + y] = 0;
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

