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
    }
}

