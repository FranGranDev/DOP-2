using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;


public class SpriteArea
{
    public SpriteArea(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;

        TextureCenter = TexMath.PointToTextureCoord(Vector3.zero, spriteRenderer.sprite).Rounded();

        SetPoints();
    }

    private SpriteRenderer spriteRenderer;

    public HashSet<Vector2Int> Points
    {
        get;
        private set;
    }
    public Vector2Int TextureCenter { get; }
    public float PixelsPerUnit
    {
        get => spriteRenderer.sprite.pixelsPerUnit;
    }
    public Vector2 Scale
    {
        get => spriteRenderer.transform.localScale;
    }


    public void SetPoints()
    {
        Points = spriteRenderer.sprite.GetCoords();
    }
    public void Remove(Vector2Int point)
    {
        Points.Remove(point);
    }
    public void Remove(IEnumerable<Vector2Int> points)
    {
        foreach(Vector2Int point in points)
        {
            Points.Remove(point);
        }
    }




    public IEnumerable<Vector2Int> Intercept(HashSet<Vector2Int> points)
    {
        return Points.Intersect(points);
    }
}
