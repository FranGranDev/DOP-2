using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;


public class SpriteArea
{
    public SpriteArea(SpriteRenderer spriteRenderer, int simplify = 1)
    {
        this.spriteRenderer = spriteRenderer;

        Width = spriteRenderer.sprite.texture.width;
        Height = spriteRenderer.sprite.texture.height;

        TextureCenter = TexMath.PointToTextureCoord(Vector3.zero, spriteRenderer.sprite).Rounded();

        SetPoints();
    }

    private SpriteRenderer spriteRenderer;

    public List<Vector2Int> Points
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
    public int Width { get; }
    public int Height { get; }


    public void SetPoints()
    {
        Points = spriteRenderer.sprite.GetCoords();
    }

    public bool Inside(Vector2Int point)
    {
        return point.x >= 0 && point.x < Width && point.y >= 0 && point.y < Height;
    }
}
