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
        public float x_location;
        public float y_location;
        public bool sabotage;
    }        
}

