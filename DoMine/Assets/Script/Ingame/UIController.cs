using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using UnityEngine.SceneManagement;
using TMPro;

namespace DoMine
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] Text timeLeft = null;
        [SerializeField] GameObject panel = null;
        [SerializeField] Text gameInfo = null;
        [SerializeField] Text winPlayers = null;
        [SerializeField] Text winSide = null;
        [SerializeField] Button endExit = null;
        [SerializeField] GameObject message = null;
        [SerializeField] Transform messageParent = null;
        List<GameObject> messageList = new List<GameObject>();
        [SerializeField] Text sabotage = null;
        [SerializeField] public Text goldLocation = null;
        [SerializeField] GameObject goldBoard = null;
        public int sabotagenum;
        private void Start()
        {
            panel.gameObject.SetActive(true);
            gameInfo.text = "Loading";
            endExit.gameObject.SetActive(false);
            winSide.gameObject.SetActive(false);
            winPlayers.gameObject.SetActive(false);
            sabotage.gameObject.SetActive(false);
            goldBoard.SetActive(false);
        }
        void Update()
        {
            if(GameController.isSabotage)
            {
                goldBoard.SetActive(true);
            }
            if(GameController.gameLoaded == true)
            {
                panel.gameObject.SetActive(false);
            }
            if (GameController.time > 0 && GameController.time <= 900 && GameController.gameStarted == false)
            {
                timeLeft.text = "���ӽ��۱��� " + ((int)Math.Floor(GameController.time+1)).ToString();
            }
            else if (GameController.gameStarted == false)//���⼭ ȣ��Ʈ�� ���� ���� ��û�� ����
            {
                timeLeft.text = "���� �غ���";
            }
            else if (GameController.time > 0 && GameController.time <= 900 && GameController.gameStarted == true)//���� �����ߴٴ� �̺�Ʈ�� ȣ��Ʈ���� ��ΰ� ������ ����
            {
                if((int)Math.Floor(GameController.time) % 60 / 10 < 1)
                {
                    timeLeft.text = "����������� " + ((int)Math.Floor(GameController.time) / 60).ToString() + " : 0" + ((int)Math.Floor(GameController.time) % 60).ToString();
                }
                else
                {
                    timeLeft.text = "����������� " + ((int)Math.Floor(GameController.time) / 60).ToString() + " : " + ((int)Math.Floor(GameController.time) % 60).ToString();
                }
            }
            else if (GameController.time <= 0 && GameController.gameStarted == true) // ���� �����
            {
                panel.gameObject.SetActive(true);
            }
        }

        public void GameWinner(int winPlayer, bool isSabotageWin, bool amIWin, string[] nameList)
        {
            int _temp = winPlayer;
            panel.gameObject.SetActive(true);
            endExit.gameObject.SetActive(true);
            winSide.gameObject.SetActive(true);
            winPlayers.gameObject.SetActive(true);
            sabotage.gameObject.SetActive(true);
            GameController.gameLoaded = false;
            if (winPlayer == -3)
            {
                gameInfo.text = "Game Over";
                winSide.text = "Game Already Started";
                winPlayers.text = "";
            }
            if (winPlayer == -2)
            {
                gameInfo.text = "Game Over";
                winSide.text = "Host has been \n shut down";
                winPlayers.text = "";
            }
            else if (winPlayer == -1)
            {
                gameInfo.text = "You Lose";
                winSide.text = "No One Won";
                winPlayers.text = "";
            }
            else
            {
                if (amIWin == true)
                {
                    gameInfo.text = "You Won";
                }
                else
                {
                    gameInfo.text = "You Lost";
                }
                if (isSabotageWin == true)
                {
                    winSide.text = "Sabotage Win!";
                }
                else
                {
                    winSide.text = "Miner Win!";
                }
                winPlayers.text = "Win players";
                do
                {
                    winPlayers.text = winPlayers.text + "\n" + nameList[_temp % 10];
                    _temp = _temp / 10;
                } while (_temp != 0);
                sabotage.text = "Sabotages";
                do
                {
                    sabotage.text = sabotage.text + "\n" + nameList[sabotagenum % 10];
                    sabotagenum = sabotagenum / 10;
                } while (sabotagenum != 0);
            }
        }
        public void GameExit()
        {
            TitleMenu.isGameEnd = true;
            BoltLauncher.Shutdown();
            SceneManager.LoadScene("Title");
        }
        public void MessagePrint(string input)
        {
            GameObject newMessage = Instantiate(message, messageParent);
            newMessage.GetComponent<Text>().text = input;
            if (messageParent.GetComponent<RectTransform>().sizeDelta.y > 0)
            {
                messageParent.GetComponent<RectTransform>().anchoredPosition = messageParent.GetComponent<RectTransform>().sizeDelta - new Vector2(340, -60);//�� �ϵ��ڵ����� ���� �޽��� ������ �ű�� �ڵ�
            }
        }

    }
}