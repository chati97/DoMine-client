using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoMine
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] GameObject player = null;
        [SerializeField] Rigidbody2D playerRB = null;
        public float power;
        public float xspeed, yspeed;
        public float breakCool;
        float breakCoolBase = 1;
        public float returnCool;
        float returnCoolBase = 1;
        public MapController mapCtrl;
        public ItemController itemCtrl;
        public GameController gameCtrl;

        public void MovePlayer(GameObject player, Vector2 location)
        {
            player.transform.position = location;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (breakCool > 0)
            {
                breakCool -= Time.deltaTime;
            }
            if (breakCool < 0)
            {
                breakCool = 0;
            }
            if (returnCool > 0)
            {
                returnCool -= Time.deltaTime;
            }
            if (returnCool < 0)
            {
                returnCool = 0;
            }
            if (Input.GetKey(KeyCode.LeftArrow) == true)
            {
                playerRB.AddForce(Vector2.left * power);
            }
            if (Input.GetKey(KeyCode.RightArrow) == true)
            {
                playerRB.AddForce(Vector2.right * power);
            }
            if (Input.GetKey(KeyCode.UpArrow) == true)
            {
                playerRB.AddForce(Vector2.up * power);
            }
            if (Input.GetKey(KeyCode.DownArrow) == true)
            {
                playerRB.AddForce(Vector2.down * power);
            }
            xspeed = playerRB.velocity.x;
            yspeed = playerRB.velocity.y;

            if(Input.GetKey(KeyCode.R) == true)
            {
                if (returnCool == 0)
                {
                    MovePlayer(player, new Vector2(50, 50));
                    returnCool = returnCoolBase;
                }
                else
                {
                    Debug.Log("in Return-Cooltime");
                }

            }

            if (Input.GetKey(KeyCode.A) == true)
            {
                if(breakCool == 0)
                {
                    if (Vector2.Distance(player.transform.position, mapCtrl.nearestWall.transform.position) < 0.8)
                    {
                        mapCtrl.DestroyWall(mapCtrl.nearestWallX, mapCtrl.nearestWallY);
                        breakCool = breakCoolBase;
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
                    if (Vector2.Distance(player.transform.position, itemCtrl.nearestItem.item.transform.position) < 0.5)
                    {
                        itemCtrl.GetItem(gameCtrl.playerInfo, itemCtrl.nearestItem);
                    }
                }
            }
        }
    }
}

