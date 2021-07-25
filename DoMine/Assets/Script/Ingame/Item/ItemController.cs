using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DoMine
{
    public class ItemController : MonoBehaviour
    {
        int[,] mapArray = new int[100, 100];
        List<Item> items = new List<Item>();
        [SerializeField] GameObject player = null;
        [SerializeField] GameObject gold = null;
        [SerializeField] Transform itemParent = null;
        [SerializeField] GameObject mapIndicator = null;
        public Item nearestItem = null;
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            FindItem(items);
        }

        public void Init(Player player)
        {
            player.inventory.gold = false;
        }

        public void GetItem(Player player, Item item)
        {
            
            switch (item.itemCode)
            {
                case 0:
                    if (player.inventory.gold == false)
                        player.inventory.gold = true;
                    else
                    {
                        Debug.Log("Already having gold");
                        return;
                    }
                    break;
            }
            DeleteItem(ref item);
        }

        // instantiate로 만들고 list에 삽입
        public void CreateItem(int x, int y, int itemCode)
        {
            GameObject _targetItem = null;
            GameObject _target = null;
            switch (itemCode)
            {
                case 0:
                    _targetItem = gold;
                    break;
            }
            _target = Instantiate(_targetItem, new Vector2(x, y), Quaternion.identity, itemParent);
            items.Add(new Item(_target, itemCode, x, y));
            return;
        }

        public void DeleteItem(ref Item item)
        {
            Destroy(item.item);
            items.Remove(item);
        }

        public void FindItem(List<Item> items)
        {
            float _nearestDistance;
            float _sampleDistance;
            Vector2 _nearestVector = new Vector2(0, 0);

            _nearestDistance = 10000;
            foreach (Item item in items)
            {
                _sampleDistance = Vector2.Distance(player.transform.position, item.item.transform.position);
                if (_nearestDistance > _sampleDistance)
                {
                    _nearestDistance = _sampleDistance;
                    _nearestVector = item.item.transform.position;
                    nearestItem = item;
                }
            }
            if (Vector2.Distance(_nearestVector, player.transform.position) < 0.5)
            {
                mapIndicator.gameObject.SetActive(true);
                mapIndicator.transform.position = _nearestVector;
            }
            else
            {
                mapIndicator.gameObject.SetActive(false);
            }
        }
    }
}

