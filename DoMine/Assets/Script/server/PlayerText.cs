using System;
using UnityEngine;
using Photon.Bolt;
using TMPro;

namespace DoMine
{
    public class PlayerText : EntityBehaviour<IPlayerState>
    {
        [SerializeField] GameObject player = null;
        [SerializeField] GameObject playerName = null;
        // Start is called before the first frame update
        void Start()
        {
            if(entity.IsOwner)
            {
                state.PlayerName = PlayerPrefs.GetString("nick");
                Debug.LogError(PlayerPrefs.GetString("nick"));
                
            }
            playerName.GetComponent<TextMeshPro>().text = state.PlayerName;
        }

    }

}
