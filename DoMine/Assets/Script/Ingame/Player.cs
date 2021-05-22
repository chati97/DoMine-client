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
        public int x_location;
        public int y_location;
        public bool sabotage;
    }        
}

