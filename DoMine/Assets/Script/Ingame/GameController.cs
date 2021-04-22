using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] Transform player;
        // Start is called before the first frame update
        void Start()
        {
            player.transform.position = new Vector2(50, 50);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

