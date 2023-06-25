using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using System;


namespace Game.Sprites
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteContainer : MonoBehaviour, IErasedEvent
    {
        [Header("Settings")]
        [SerializeField] private int clearThreshold;
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Sprite sprite;
        private Texture2D texture;

        private Color32[] startPixels;
        private Color32[] currantPixels;

        public Sprite Sprite
        {
            get => sprite;
        }
        public int MaxPixelCount
        {
            get => texture.width * texture.height;
        }
        public Vector2Int Size
        {
            get
            {
                return new Vector2Int(texture.width, texture.height);
            }
        }
        public Color32[] Pixels
        {
            get
            {
                return currantPixels;
            }
        }
        public SpriteArea Area
        {
            get;
            private set;
        }


        public event Action OnUpdated;
        public event Action OnInitialized;
        public event Action OnErased;


        private void Start()
        {
            CopySprite();

            OnInitialized?.Invoke();
        }

        private void CopySprite()
        {
            sprite = spriteRenderer.sprite.Copy();
            spriteRenderer.sprite = sprite;
            texture = sprite.texture;

            Area = new SpriteArea(spriteRenderer);

            startPixels = texture.GetPixels32();
            currantPixels = new Color32[startPixels.Length];
            Array.Copy(startPixels, currantPixels, startPixels.Length);
        }


        public void ClearPixels(IEnumerable<Vector2Int> coords)
        {
            int width = texture.width;
            Color32 color = new Color32(0, 0, 0, 0);
            foreach (Vector2Int coord in coords)
            {
                currantPixels[coord.x + coord.y * width] = color;
            }

            texture.SetPixels32(currantPixels);
            texture.Apply();

            OnUpdated?.Invoke();
        }

        public void EndErase()
        {
            if (AllCleaned())
            {
                ClearAll();
                OnErased?.Invoke();
            }
            else
            {
                Restore();
            }
        }

        private bool AllCleaned()
        {
            int oraque = 0;
            for (int i = 0; i < currantPixels.Length; i++)
            {
                if (currantPixels[i].a > 0.5f)
                {
                    oraque++;
                }
            }

            return oraque <= clearThreshold;
        }
        private void ClearAll()
        {
            int width = texture.width;
            Color32 color = new Color32(0, 0, 0, 0);
            for (int i = 0; i < currantPixels.Length; i++)
            {
                currantPixels[i] = color;
            }

            texture.SetPixels32(currantPixels);
            texture.Apply();

            OnUpdated?.Invoke();
        }
        private void Restore()
        {
            texture.SetPixels32(startPixels);
            texture.Apply();

            Array.Copy(startPixels, currantPixels, startPixels.Length);

            OnUpdated?.Invoke();
        }
    }
}