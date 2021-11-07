using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

namespace DoMine
{
    public class CameraController : EntityBehaviour<IPlayerState>
    {
        public Camera entityCamera;                                    // 프리팹에 장착된 개인 카메라

        public override void Attached()
        {
            if(entity.IsOwner)
            {
                entityCamera.gameObject.SetActive(true);
            }
        }
        void Update()
        {
            //망원경획득시 카메라 원경 변경
            Vector3 temp = entityCamera.transform.position;
            if (state.Inventory[5] == 1 && state.Blinded == false)
            {
                temp.z = -7.5f;
                entityCamera.transform.position = temp;
            }
            else if (temp.z == -15)
            {
                temp.z = temp.z / 2;
                entityCamera.transform.position = temp;
            }
        }
    }
}

