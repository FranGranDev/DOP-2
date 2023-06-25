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


        private void Update()
        {
            brushControl.InputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                brushControl.StartErase();
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                brushControl.EndErase();
            }
        }
    }
}
