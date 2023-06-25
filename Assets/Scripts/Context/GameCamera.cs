using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Context
{
    [ExecuteInEditMode]
    public class GameCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Min(1)] private int targetRatioX = 9;
        [SerializeField, Min(1)] private int targetRatioY = 16;
        [Space]
        [SerializeField] private float size = 6;

        private Camera Camera
        {
            get
            {
                if(cam == null)
                {
                    cam = Camera.main;
                }

                return cam;
            }
        }
        private Camera cam;

        private void FitCamera()
        {
            float targetAspectRatio = (float)targetRatioX / (float)targetRatioY;
            float currentAspectRatio = (float)Screen.width / Screen.height;
            float scaleHeight = targetAspectRatio / currentAspectRatio;

            float orthographicSize = size;
            orthographicSize *= scaleHeight;

            Camera.orthographicSize = orthographicSize;
        }

        private void Start()
        {
            FitCamera();
        }
        private void Update()
        {
#if UNITY_EDITOR
            FitCamera();
#endif
        }
    }
}
