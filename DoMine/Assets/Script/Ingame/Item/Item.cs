using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoMine
{
    [Serializable]
    public class Item
    {
        public GameObject item = new GameObject();    
        public int itemCode;
        public int x_location;
        public int y_location;

        public Item(GameObject _item, int _itemCode, int _x_location, int _y_location)
        {
            item = _item;
            itemCode = _itemCode;
            x_location = _x_location;
            y_location = _y_location;
        }
    }
}
