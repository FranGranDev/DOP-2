using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Services;

namespace Game.Sprites
{
    [RequireComponent(typeof(IBrushControl))]
    public class BrushController : MonoBehaviour, IBindable<IGameEvent>
    {
        private IBrushControl brushControl;
        private IGameEvent gameEvents;

        private GameStates gameStates;

        private bool touched;

        private void Awake()
        {
            brushControl = GetComponent<IBrushControl>();
        }

        public void Bind(IGameEvent obj)
        {
            gameEvents = obj;

            gameEvents.OnStateChanged += OnStateChanged;
            gameEvents.OnLevelLoaded += OnLevelLoaded;
        }



        private void MouseInput()
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetKey(KeyCode.Mouse0) && !touched)
            {
                Tap(point);
            }
            if (!Input.GetKey(KeyCode.Mouse0) && touched)
            {
                Release();
            }

            if (touched)
            {
                Move(point);
            }
        }
        private void ScreenInput()
        {
            if (Input.touchCount > 0 && !touched)
            {
                Tap(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
            }
            if (Input.touchCount == 0 && touched)
            {
                Release();
            }

            if(touched)
            {
                Move(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
            }
        }


        private void Tap(Vector2 point)
        {
            brushControl.StartErase(point);
            touched = true;
        }
        private void Release()
        {
            brushControl.EndErase();
            touched = false;
        }
        private void Move(Vector2 point)
        {
            brushControl.InputPoint = point;
        }

        private void OnStateChanged(GameStates obj)
        {
            gameStates = obj;

            if(obj != GameStates.Game && touched)
            {
                if(touched)
                {
                    Release();
                }    
            }
        }
        private void OnLevelLoaded(int index, string label)
        {
            brushControl.Clear();
        }


        private void Update()
        {
            if (gameStates != GameStates.Game)
                return;

#if UNITY_EDITOR
            MouseInput();
#else
            ScreenInput();
#endif
        }
    }
}
