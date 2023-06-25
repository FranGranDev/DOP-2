using Game.Sprites.Calculation;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Sprites
{
    public class SpriteEraser : MonoBehaviour, IBrushControl
    {
        [Header("Settings")]
        [SerializeField, Range(1f, 10)] private int iterations = 1;
        [Header("Brush Settings")]
        [SerializeField, Range(0.1f, 1f)] private float brushSmooth = 1f;
        [SerializeField, Min(0)] private float brushMaxSpeed = 5f;
        [Header("Components")]
        [SerializeField] private Brush brush;


        private ICoordTransform coordTransform;
        private Stack<SpriteContainer> visitedSprites;

        private Vector2 brushPoint;
        private Vector2 brushPrev;
        private bool isBrushing;

        public Vector2 InputPoint { get; set; }


        private void Initialize()
        {
            coordTransform = new FastCoordTransform(brush.Area);

            visitedSprites = new Stack<SpriteContainer>();
        }

        private void TryBrushErase()
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(brushPoint, brush.Size, 0);
            foreach(Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out SpriteContainer sprite))
                {
                    BrushErase(sprite);

                    if (!visitedSprites.Contains(sprite))
                    {
                        visitedSprites.Push(sprite);
                    }
                }
            }
        }
        private void BrushErase(SpriteContainer sprite)
        {
            for (int i = 0; i < iterations; i++)
            {
                Vector3 point = Vector3.Lerp(brushPrev, brushPoint, (float)i / (float)iterations);

                IEnumerable<Vector2Int> points = coordTransform.Execute(point, sprite, brush.Area);

                sprite.ClearPixels(points);
            }
        }
        private void MoveBrush()
        {
            brushPrev = brushPoint;
            brushPoint = Vector2.Lerp(brush.Position, InputPoint, brushSmooth);
            Vector2 velocity = (brushPoint - brushPrev) / Time.fixedDeltaTime;
            if (velocity.magnitude > brushMaxSpeed)
            {
                brushPoint = brushPrev + velocity.normalized * brushMaxSpeed * Time.fixedDeltaTime;
            }

            brush.Position = brushPoint;


            brush.Hidden = !isBrushing;
        }


        public void StartErase(Vector2 point)
        {
            brushPoint = point;
            brushPrev = point;
            brush.Position = brushPoint;            

            isBrushing = true;
        }
        public void EndErase()
        {
            isBrushing = false;

            foreach (SpriteContainer sprite in visitedSprites)
            {
                sprite.EndErase();
            }

            visitedSprites.Clear();
        }
        public void Clear()
        {
            visitedSprites.Clear();
            isBrushing = false;
        }

        private void Start()
        {
            Initialize();
        }
        private void FixedUpdate()
        {
            MoveBrush();
        }
        private void Update()
        {
            if (isBrushing)
            {
                TryBrushErase();
            }
        }
    }
}