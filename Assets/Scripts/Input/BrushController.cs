using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Sprites
{
    [RequireComponent(typeof(IBrushControl))]
    public class BrushController : MonoBehaviour
    {
        private IBrushControl brushControl;


        private bool isBrusing;

        private void Awake()
        {
            brushControl = GetComponent<IBrushControl>();
        }

        private void MouseInput()
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.Mouse0) && !isBrusing)
            {
                brushControl.StartErase(point);
                isBrusing = true;
            }
            if (Input.GetKeyUp(KeyCode.Mouse0) && isBrusing)
            {
                brushControl.EndErase();
                isBrusing = false;
            }

            if (isBrusing)
            {
                brushControl.InputPoint = point;
            }
        }
        private void ScreenInput()
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);

            if (Input.touchCount > 0 && !isBrusing)
            {
                brushControl.StartErase(point);
                isBrusing = true;
            }
            if (Input.touchCount == 0 && isBrusing)
            {
                brushControl.EndErase();
                isBrusing = false;
            }

            if(isBrusing)
            {
                brushControl.InputPoint = point;
            }
        }


        private void Update()
        {
#if UNITY_EDITOR
            MouseInput();
#else
            ScreenInput();
#endif
        }
    }
}
