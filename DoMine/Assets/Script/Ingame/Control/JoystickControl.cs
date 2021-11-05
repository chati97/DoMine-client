using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Bolt;

namespace DoMine
{
    public class JoystickControl : EntityBehaviour<IPlayerState>, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform joyStick;

        private Image bgimg;
        private float radius;

        private Vector3 moveVec;

        public Button sabotageSkill;
        public Button minerSkill;
        public Button heal;
        public Button barricade;
        public Button noPick;
        public Button pick;
        public Button baseCamp;

        public static int btnNum = 0;

        void Start()
        {
            bgimg = GetComponent<Image>();
            radius = background.rect.width * 0.29f;
            pick.onClick.AddListener(onclickPick);
            barricade.onClick.AddListener(onClickBarricade);
            heal.onClick.AddListener(onClickHeal);
            noPick.onClick.AddListener(onClickNoPick);
            baseCamp.onClick.AddListener(onClickBaseCamp);
            sabotageSkill.onClick.AddListener(onClickSabSkill);
            minerSkill.onClick.AddListener(onClickMinSkill);
            if (GameController.isSabotage)
            {
                minerSkill.gameObject.SetActive(false);
                sabotageSkill.gameObject.SetActive(true);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 stickVec = eventData.position - (Vector2)background.position;
            Vector2 pos;

            stickVec = Vector2.ClampMagnitude(stickVec, radius);
            joyStick.localPosition = stickVec;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgimg.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
            {
                pos.x = pos.x / bgimg.rectTransform.sizeDelta.x;
                pos.y = pos.y / bgimg.rectTransform.sizeDelta.y;

                moveVec = new Vector3(pos.x * 2 + 1, pos.y * 2 - 1, 0);
                moveVec = (moveVec.magnitude > 1.0f) ? moveVec.normalized : moveVec;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            joyStick.localPosition = Vector3.zero;
            moveVec = Vector3.zero;
        }

        public float GetHorizontal()
        {
            return moveVec.x;
        }

        public float GetVertical()
        {
            return moveVec.y;
        }

        void onclickPick()
        {
            btnNum = 1;
        }

        void onClickBarricade()
        {
            btnNum = 2;
        }

        void onClickHeal()
        {
            btnNum = 3;
        }

        void onClickNoPick()
        {
            btnNum = 4;
        }

        void onClickBaseCamp()
        {
            btnNum = 5;
        }

        void onClickSabSkill()
        {
            btnNum = 6;
        }

        void onClickMinSkill()
        {
            btnNum = 7;
        }
    }
}