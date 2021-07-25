using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoMine
{
    [Serializable]
    public class Player
    {
        public Inventory inventory = new Inventory();
        public float x_location;
        public float y_location;
        public bool sabotage;
    }        
}

