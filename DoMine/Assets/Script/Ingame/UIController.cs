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
                timeLeft.text = "���ӽ��۱���" + ((int)Math.Floor(GameController.time)).ToString();
            }
            else if (GameController.gameStarted == false)//���⼭ ȣ��Ʈ�� ���� ���� ��û�� ����
            {
                timeLeft.text = "���� �غ���";
            }
            else if (GameController.time > 0 && GameController.time <= 900 && GameController.gameStarted == true)//���� �����ߴٴ� �̺�Ʈ�� ȣ��Ʈ���� ��ΰ� ������ ����
            {
                timeLeft.text = "�����������" + ((int)Math.Floor(GameController.time) / 60).ToString() + " : " + ((int)Math.Floor(GameController.time) % 60).ToString();
            }
            else if (GameController.time <= 0 && GameController.gameStarted == true) // ���� �����
            {
                panel.gameObject.SetActive(true);
                gameInfo.text = "GameEnded";
            }
        }
    }
}