using Game.Utils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace Game.Sprites.Calculation
{
    public interface ICoordTransform
    {
        IEnumerable<Vector2Int> Execute(Vector3 point, SpriteContainer target, SpriteArea brush);
    }


    public class FastCoordTransform : ICoordTransform
    {
        public FastCoordTransform(SpriteArea brush)
        {
            brushPoints = new NativeArray<Vector2Int>(brush.Points.ToArray(), Allocator.Persistent);
            for (int i = 0; i < brush.Points.Count; i++)
            {
                brushPoints[i] = brush.Points[i] - brush.TextureCenter;
            }

            output = new NativeArray<Vector2Int>(brushPoints.Length, Allocator.Persistent);
        }

        private NativeArray<Vector2Int> brushPoints;
        private NativeArray<Vector2Int> output;


        ~FastCoordTransform()
        {
            brushPoints.Dispose();
            output.Dispose();
        }

        public IEnumerable<Vector2Int> Execute(Vector3 point, SpriteContainer target, SpriteArea brush)
        {
            Vector2Int targetCoordStart = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(point), target.Sprite).Rounded();

            float ratio = target.Area.PixelsPerUnit / brush.PixelsPerUnit;
            float scaleX = brush.Scale.x * ratio;
            float scaleY = brush.Scale.y * ratio;


            PixelsCalculation calculation = new PixelsCalculation()
            {
                coords = targetCoordStart,

                points = brushPoints,

                width = target.Area.Width,
                height = target.Area.Height,

                scaleX = scaleX,
                scaleY = scaleY,

                result = output,
            };

            JobHandle jobHandle = calculation.Schedule(brushPoints.Length, 64);
            jobHandle.Complete();

            return output;
        }

        private struct PixelsCalculation : IJobParallelFor
        {
            [ReadOnly] public Vector2Int coords;

            [ReadOnly] public NativeArray<Vector2Int> points;


            [ReadOnly] public int width;
            [ReadOnly] public int height;

            [ReadOnly] public float scaleX;
            [ReadOnly] public float scaleY;

            [WriteOnly] public NativeArray<Vector2Int> result;

            public void Execute(int index)
            {
                Vector2Int transformed = points[index].Multilied(scaleX, scaleY) + coords;
                if (transformed.x >= 0 && transformed.x < width && transformed.y >= 0 && transformed.y < height)
                {
                    result[index] = transformed;
                }
                else
                {
                    result[index] = default;
                }
            }
        }
    }
    public class SlowCoordTransform : ICoordTransform
    {
        public IEnumerable<Vector2Int> Execute(Vector3 point, SpriteContainer target, SpriteArea brush)
        {
            Vector2Int centerCoords = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(point), target.Sprite).Rounded();


            List<Vector2Int> points = new List<Vector2Int>(brush.Points.Count);

            float ratio = target.Area.PixelsPerUnit / brush.PixelsPerUnit;
            float scaleX = brush.Scale.x * ratio;
            float scaleY = brush.Scale.y * ratio;

            foreach (Vector2Int coord in brush.Points)
            {
                Vector2Int transformed = (coord - brush.TextureCenter).Multilied(scaleX, scaleY) + centerCoords;
                if (transformed.x >= 0 && transformed.x < target.Area.Width && transformed.y >= 0 && transformed.y < target.Area.Height)
                {
                    points.Add(transformed);
                }
            }

            return points;
        }
    }

}