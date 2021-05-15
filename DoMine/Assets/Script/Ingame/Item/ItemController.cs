using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DoMine
{
    public class ItemController : MonoBehaviour
    {
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

        public void GetItem(Player player, int item)
        {
            switch (item)
            {
                case 0: player.inventory.gold = true;
                break;
            }
        }
    }
}

