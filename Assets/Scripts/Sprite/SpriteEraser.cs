using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Game.Utils;
using System.Threading.Tasks;

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

        private Vector2 brushPoint;
        private Vector2 brushPrev;
        private bool isBrushing;


        public Vector2 InputPoint { get; set; }

        public void StartErase()
        {
            isBrushing = true;
        }
        public void EndErase()
        {
            isBrushing = false;
            //CheckForEnd;
        }


        private void TryBrushErase()
        {
            Collider2D collider = Physics2D.OverlapBox(brushPoint, brush.Size, 0);
            if(collider && collider.TryGetComponent(out SpriteContainer sprite))
            {
                BrushErase(sprite);
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
        }


        private void Start()
        {
            coordTransform = new FastCoordTransform(brush.Area);
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