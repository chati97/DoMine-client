using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoMine
{
    [Serializable]
    public class Item : MonoBehaviour
    {
        public GameObject item;
        public int itemCode;
        public int index;
        public int x_location;
        public int y_location;
        void Update()
        {
        }

    }
}
