using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoMine
{
    public class PlayerMove : MonoBehaviour
    {
        public float moveSpeed;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            float _inputX = Input.GetAxisRaw("Horizontal");
            float _inputY = Input.GetAxisRaw("Vertical");
            transform.Translate(new Vector2(_inputX,_inputY)*Time.deltaTime*moveSpeed);
        }
    }
}

