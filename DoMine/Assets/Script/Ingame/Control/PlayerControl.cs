using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoMine
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] Rigidbody2D player = null;
        public float power;
        public float xspeed, yspeed;
        public float breakCool;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(breakCool > 0)
            {
                breakCool -= Time.deltaTime;
            }
            if(breakCool < 0)
            {
                breakCool = 0;
            }
            if(Input.GetKey(KeyCode.LeftArrow) == true)
            {
                player.AddForce(Vector2.left * power);
            }
            if (Input.GetKey(KeyCode.RightArrow) == true)
            {
                player.AddForce(Vector2.right * power);
            }
            if (Input.GetKey(KeyCode.UpArrow) == true)
            {
                player.AddForce(Vector2.up * power);
            }
            if (Input.GetKey(KeyCode.DownArrow) == true)
            {
                player.AddForce(Vector2.down * power);
            }
            xspeed = player.velocity.x;
            yspeed = player.velocity.y;


            if(Input.GetKey(KeyCode.A) == true)
            {
                if(breakCool == 0)
                {
                    if (Vector2.Distance(player.position, mapCtrl.nearestWall.transform.position) < 0.8)
                    {
                        mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY);
                        breakCool = 1;
                    }
                }
                else
                {
                    Debug.Log("in Breaking-Cooltime");
                }
                
            }

            if (Input.GetKey(KeyCode.S) == true)
            {
                if(itemCtrl.nearestItem != null)
                {
                    if (Vector2.Distance(player.position, itemCtrl.nearestItem.item.transform.position) < 0.8)
                    {
                        itemCtrl.GetItem(gameCtrl.playerInfo, itemCtrl.nearestItem);
                    }
                }
            }
        }
    }
}

