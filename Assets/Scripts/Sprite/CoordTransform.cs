using Cysharp.Threading.Tasks;
using Game.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;


public interface ICoordTransform
{
    HashSet<Vector2Int> Execute(Vector3 prev, Vector3 point, int iterations, SpriteContainer target, SpriteArea brush);
}

public class CoordTransform : ICoordTransform
{
    public HashSet<Vector2Int> Execute(Vector3 prev, Vector3 point, int iterations, SpriteContainer target, SpriteArea brush)
    {
        Vector2Int targetCoordStart = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(prev), target.Sprite).Rounded();
        Vector2Int targetCoordEnd = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(point), target.Sprite).Rounded();

        Vector2Int path = (targetCoordEnd - targetCoordStart);
        Vector2Int step = path.Devided(iterations);

        HashSet<Vector2Int> points = new HashSet<Vector2Int>();

        float ratio = target.Area.PixelsPerUnit / brush.PixelsPerUnit;
        float scaleX = brush.Scale.x * ratio;
        float scaleY = brush.Scale.y * ratio;

        foreach (Vector2Int coord in brush.Points)
        {
            for (int i = 0; i < iterations; i++)
            {
                Vector2Int transformed = (coord - brush.TextureCenter).Multilied(scaleX, scaleY) + targetCoordStart + step * i;
                if (!target.Area.Inside(transformed))
                    continue;

                points.Add(transformed);

            }
        }

        return points;
    }
}

public class FastCoordTransform : ICoordTransform
{
    public FastCoordTransform(SpriteArea brush)
    {
        brushPoints = new NativeArray<Vector2Int>(brush.Points.ToArray(), Allocator.Persistent);
        for(int i = 0; i < brush.Points.Count; i++)
        {
            brushPoints[i] = brush.Points[i] - brush.TextureCenter;
        }
    }

    private NativeArray<Vector2Int> brushPoints;


    ~FastCoordTransform()
    {
        brushPoints.Dispose();
    }

    public HashSet<Vector2Int> Execute(Vector3 prev, Vector3 point, int iterations, SpriteContainer target, SpriteArea brush)
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>(brushPoints.Length * iterations);


        Vector2Int targetCoordStart = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(prev), target.Sprite).Rounded();
        Vector2Int targetCoordEnd = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(point), target.Sprite).Rounded();

        Vector2Int path = (targetCoordEnd - targetCoordStart);
        Vector2Int step = path.Devided(iterations);


        float ratio = target.Area.PixelsPerUnit / brush.PixelsPerUnit;
        float scaleX = brush.Scale.x * ratio;
        float scaleY = brush.Scale.y * ratio;


        NativeArray<Vector2Int> output = new NativeArray<Vector2Int>(brushPoints.Length, Allocator.TempJob);

        for (int i = 0; i < iterations; i++)
        {

            PixelsCalculation calculation = new PixelsCalculation()
            {
                coords = targetCoordStart + step * i,

                points = brushPoints,

                width = target.Area.Width,
                height = target.Area.Height,

                scaleX = scaleX,
                scaleY = scaleY,

                result = output,
            };

            JobHandle jobHandle = calculation.Schedule(brushPoints.Length, 64);
            jobHandle.Complete();


            foreach(Vector2Int res in output)
            {
                result.Add(res);
            }
        }
        output.Dispose();

        return result;
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
        }
    }
}
