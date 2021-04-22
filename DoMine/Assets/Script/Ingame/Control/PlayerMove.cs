using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoMine
{
    public class PlayerMove : MonoBehaviour
    {
        private Rigidbody2D player;
        public float power;
        public float xspeed, yspeed;

        // Start is called before the first frame update
        void Start()
        {
            player = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //float _inputX = Input.GetAxisRaw("Horizontal");
            //float _inputY = Input.GetAxisRaw("Vertical");
            //transform.Translate(new Vector2(_inputX,_inputY)*Time.deltaTime*moveSpeed);
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

        }
    }
}

