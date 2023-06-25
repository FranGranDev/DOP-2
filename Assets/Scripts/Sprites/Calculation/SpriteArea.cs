using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;


public class SpriteArea
{
    public SpriteArea(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;

        Width = spriteRenderer.sprite.texture.width;
        Height = spriteRenderer.sprite.texture.height;

        TextureCenter = TexMath.PointToTextureCoord(Vector3.zero, spriteRenderer.sprite).Rounded();
        Points = spriteRenderer.sprite.GetCoords();
    }

    private SpriteRenderer spriteRenderer;

    public List<Vector2Int> Points
    {
        get;
    }
    public Vector2Int TextureCenter
    {
        get;
    }
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
}
