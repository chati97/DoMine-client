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

        private Vector3 moveVec = Vector3.zero;

        public Button sabotageSkill;
        public Button minerSkill;
        public Button heal;
        public Button barricade;
        public Button noPick;
        public Button pick;
        public Button baseCamp;

        public float Horizontal { get { return moveVec.x; } }
        public float Vertical { get { return moveVec.y; } }

        public static int btnNum = 0;
        public static int directionX = 0;
        public static int lookAt = 0;
        public static float distance = 0;


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

            stickVec = Vector2.ClampMagnitude(stickVec, radius);
            joyStick.localPosition = stickVec;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgimg.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos))
            {
                pos.x = pos.x / bgimg.rectTransform.sizeDelta.x;
                pos.y = pos.y / bgimg.rectTransform.sizeDelta.y;

                moveVec = new Vector3(pos.x * 2 + 1, pos.y * 2 - 1, 0);
                moveVec = (moveVec.magnitude > 1.0f) ? moveVec.normalized : moveVec;

                distance = Vector2.Distance(background.position, joyStick.position) / (radius * 0.4f);

                if (moveVec.x == 0 && moveVec.y == 0)
                {
                    directionX = 0;
                }
                else
                {
                    Vector3 direction = new Vector3(moveVec.x, moveVec.y, 0);
                    float angle = Quaternion.FromToRotation(new Vector3(1, 0, 0), direction).eulerAngles.z;

                    if (angle >= 135 && angle < 225)
                    {
                        directionX = 1;
                        lookAt = 1;
                    }

                    if ((angle >= 315 && angle <= 360) || (angle <= 45 && angle >= 0))
                    {
                        directionX = 2;
                        lookAt = 2;
                    }
                    if (angle < 135 && angle >= 45)
                    {
                        lookAt = 3;
                    }
                    if (angle >= 225 && angle < 315)
                    {
                        lookAt = 4;
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            joyStick.localPosition = Vector3.zero;
            moveVec = Vector3.zero;
            directionX = 0;
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