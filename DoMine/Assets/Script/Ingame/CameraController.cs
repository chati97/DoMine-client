using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoMine
{
    public class CameraController : MonoBehaviour
    {
        public Transform Target;             // 따라다닐 타겟 오브젝트
        public Vector3 lookOffset = new Vector3(0,0,-10);            // 고정시킬 카메라의 z축의 값

        void Start()
        {

        }
        void Update()
        {
            transform.position = Target.position + lookOffset;
            transform.LookAt(Target.transform);
            
        }
    }
}

