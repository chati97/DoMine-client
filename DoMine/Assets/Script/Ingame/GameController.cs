using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] GameObject player = null;
        [SerializeField] ItemController itemcontroller = null;
        Player playerInfo = new Player();
        public bool gold = true;
        // Start is called before the first frame update
        void Start()
        {
            player.transform.position = new Vector2(50, 50);
            itemcontroller.Init(playerInfo);
        }

        // Update is called once per frame
        void Update()
        {
            gold = playerInfo.inventory.gold;
        }
    }
}

