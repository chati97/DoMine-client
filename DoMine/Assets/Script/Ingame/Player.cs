using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;

namespace DoMine
{
    [Serializable]
    public class Player : MonoBehaviourPunCallbacks
    {
        public Inventory inventory = new Inventory();
        public int x_location;
        public int y_location;
        public bool sabotage;
    }        
}

