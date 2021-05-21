using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DoMine
{
    public class ItemController : MonoBehaviour
    {
        List<Item> item = new List<Item>();
        [SerializeField] GameObject gold = null;
        int listIndex;
        public Item nearestItem = null;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(Player player)
        {
            player.inventory.gold = false;
        }

        public void GetItem(Player player, Item item)
        {
            switch (item.itemCode)
            {
                case 0: player.inventory.gold = true;
                    
                break;
            }
        }
        
        // instantiate로 만들고 list에 삽입
        public void createItem(GameObject target, int x, int y)
        {
            return;
        }
    }
}

