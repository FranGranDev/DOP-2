using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteContainer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Sprite sprite;
    private Texture2D texture;

    private Color[] pixelsBuffer;

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
    public Color[] Pixels
    {
        get
        {
            return texture.GetPixels();
        }
    }
    public SpriteArea Area
    {
        get;
        private set;
    }


    public event System.Action OnUpdated;


    private void Awake()
    {
        CopySprite();
    }

    private void CopySprite()
    {
        sprite = spriteRenderer.sprite.Copy();
        spriteRenderer.sprite = sprite;
        texture = sprite.texture;

        Area = new SpriteArea(spriteRenderer);
        pixelsBuffer = Pixels;
    }

    public void ClearPixels(IEnumerable<Vector2Int> coords)
    {
        foreach (Vector2Int coord in coords)
        {
            texture.SetPixel(coord.x, coord.y, Color.clear);
        }

        texture.Apply();
        //Area.Remove(coords);

        OnUpdated?.Invoke();
    }
}
