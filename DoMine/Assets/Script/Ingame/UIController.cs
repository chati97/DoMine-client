using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DoMine
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] Text timeLeft = null;
        [SerializeField] GameObject panel = null;
        [SerializeField] Text gameInfo = null;

        private void Start()
        {
            panel.gameObject.SetActive(true);
            gameInfo.text = "Loading";
        }
        void Update()
        {
            if(GameController.gameLoaded == true)
            {
                panel.gameObject.SetActive(false);
            }
            if (GameController.time > 0 && GameController.time <= 900 && GameController.gameStarted == false)
            {
                timeLeft.text = "게임시작까지" + ((int)Math.Floor(GameController.time)).ToString();
            }
            else if (GameController.gameStarted == false)//여기서 호스트가 게임 시작 요청을 보냄
            {
                timeLeft.text = "게임 준비중";
            }
            else if (GameController.time > 0 && GameController.time <= 900 && GameController.gameStarted == true)//게임 시작했다는 이벤트를 호스트포함 모두가 받으면 실행
            {
                timeLeft.text = "게임종료까지" + ((int)Math.Floor(GameController.time) / 60).ToString() + " : " + ((int)Math.Floor(GameController.time) % 60).ToString();
            }
            else if (GameController.time <= 0 && GameController.gameStarted == true) // 게임 종료시
            {
                panel.gameObject.SetActive(true);
                gameInfo.text = "GameEnded";
            }
        }
    }
}