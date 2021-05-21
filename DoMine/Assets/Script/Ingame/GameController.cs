using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] GameObject player = null;
        [SerializeField] ItemController itemcontroller = null;
        public Player playerInfo = new Player();
        public bool gold = true;
        // Start is called before the first frame update
        void Start()
        {
            player.transform.position = new Vector2(50, 50);
            itemcontroller.Init(playerInfo);
            itemcontroller.CreateItem(50, 50, 0);
            itemcontroller.CreateItem(49, 49, 0);
        }

        // Update is called once per frame
        void Update()
        {
            gold = playerInfo.inventory.gold;
        }
    }
}

