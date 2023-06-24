using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine.Profiling;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteContainer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Sprite sprite;
    private Texture2D texture;

    private Color32[] pixelsBuffer;

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
        pixelsBuffer = texture.GetPixels32();
    }

    public void ClearPixels(IEnumerable<Vector2Int> coords)
    {
        Profiler.BeginSample("WTF");

        int width = texture.width;
        Color32 color = new Color32(0, 0, 0, 0);
        foreach (Vector2Int coord in coords)
        {
            pixelsBuffer[coord.x + coord.y * width] = color;
            i++;
        }

        Profiler.EndSample();


        texture.SetPixels32(pixelsBuffer);
        texture.Apply();
        //Area.Remove(coords);

        OnUpdated?.Invoke();
    }
}
