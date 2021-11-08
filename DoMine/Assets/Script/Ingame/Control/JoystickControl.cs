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
        public GameObject compass;

        public float Horizontal { get { return moveVec.x; } }
        public float Vertical { get { return moveVec.y; } }

        public static int btnNum = 0;
        public static bool headRight;
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

        public void compasscontrol(GameObject player, Vector2 home, float rotatespeed)
        {
            //basecamp 위치 나침반으로 가리키게 함
            Vector2 direction = new Vector2(    //palyer가 basecamp와 떨어진 방향
                player.transform.position.x - home.x,
                player.transform.position.y - home.y
            );

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion angleAxis = Quaternion.AngleAxis(angle + 90f, Vector3.forward); //+90f 안하면 파란 바늘이 아니라 옆면이 가리킴
            Quaternion rotation = Quaternion.Slerp(compass.transform.rotation, angleAxis, rotatespeed * Time.deltaTime);
            compass.transform.rotation = rotation;
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

                moveVec = new Vector3(pos.x * 2, pos.y * 2, 0);//+1 -1빼니까 상하로 속도쏠림 없어짐
                moveVec = (moveVec.magnitude > 1.0f) ? moveVec.normalized : moveVec;

                distance = Vector2.Distance(background.position, joyStick.position) / (radius * 0.4f);

                //먼 영문인진 모르겠지만 각도도 반대로 찍히고 각도도 45도 더 돌아가는터라 보정치넣고 각도 그냥 움직일방향맞춰서 때려넣었음 작동엔 문제없음
                Vector3 direction = new Vector3(moveVec.x, moveVec.y, 0);
                float angle = Quaternion.FromToRotation(background.position, direction).eulerAngles.z;
                angle -= 45;//보정치 삽입하는용도 얘 안넣으면 45도와 225도에서 좌우가 바뀌는 기현상일어남 45를 빼줄시 정상작동         
                if (angle >= 0 && angle < 180) 
                {
                    headRight = false;
                }
                else
                {
                    headRight = true;
                }

                if (angle >= 180 && angle < 270)
                {
                    lookAt = 3;
                }
                else if ((angle < 90 && angle >= 0))
                {
                    lookAt = 1;
                }
                else if (angle < 180 && angle >= 90)
                {
                    lookAt = 0;
                }
                else if (angle >= 270 && angle < 360)
                {
                    lookAt = 2;
                }

            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            joyStick.localPosition = Vector3.zero;
            moveVec = Vector3.zero;
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