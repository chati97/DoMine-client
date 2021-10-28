using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] GameObject player;
        [SerializeField] private float speed;

        private Vector3 moveVec;

        PlayerControl playerControl;

        void Start()
        {
            bgimg = GetComponent<Image>();
            radius = background.rect.width * 0.29f;
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
    }
}