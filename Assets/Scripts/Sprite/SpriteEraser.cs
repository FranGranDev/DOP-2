using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Game.Utils;
using System.Threading.Tasks;

public class SpriteEraser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float brushSize = 1f;
    [SerializeField, Range(0.1f, 1f)] private float brushSpeed = 1f;
    [Header("Components")]
    [SerializeField] private Transform brushObject;
    [SerializeField] private SpriteRenderer spriteRenderer;


    private Texture2D texture;
    private Sprite sprite;

    private Vector2 inputPoint;
    private Vector2 brushPoint;

    private bool isBrushing;

    private Color[] pixels;

    public Color GetPixel(int x, int y)
    {
        int index = GetPixelIndex(x, y);
        return pixels[index];
    }
    public void SetPixel(int x, int y, Color color)
    {
        int index = GetPixelIndex(x, y);
        if(pixels[index] != color)
        {
            pixels[index] = color;
            texture.SetPixel(x, y, color);
        }
    }
    private int GetPixelIndex(int x, int y)
    {
        return x + y * texture.width;
    }

    private void Awake()
    {
        CopySprite();

        pixels = texture.GetPixels();
    }


    private void CopySprite()
    {
        sprite = spriteRenderer.sprite.Copy();
        spriteRenderer.sprite = sprite;
        texture = sprite.texture;
    }

    private void BrushErase()
    {
        List<TextureHit> coords = new List<TextureHit>();

        float delta = 0.25f;
        for(float i = 0; i < 1f; i += delta)
        {
            Profiler.BeginSample($"Loop {i}");
            Vector3 point = Vector3.Lerp(brushPoint, inputPoint, i);

            Vector2 localPoint = transform.InverseTransformPoint(point);
            Vector2 textureCoord = TexMath.PointToTextureCoord(localPoint, sprite);

            int size = Mathf.RoundToInt(sprite.pixelsPerUnit * brushSize);

            coords.AddRange(TexMath.OverlapCircle(textureCoord, size, sprite));
            Profiler.EndSample();
        }

        Profiler.BeginSample($"Set Pixels");
        foreach (TextureHit coord in coords)
        {
            SetPixel(coord.X, coord.Y, Color.clear);
        }
        Profiler.EndSample();
        Profiler.BeginSample($"Apply");

        texture.Apply();

        Profiler.EndSample();
    }



    private void FixedUpdate()
    {
        if (isBrushing)
        {
            BrushErase();
        }
        brushPoint = Vector2.Lerp(brushObject.position, inputPoint, brushSpeed);

        brushObject.localScale = Vector2.Lerp(brushObject.localScale, Vector3.one * brushSize * 2, 0.1f);
        brushObject.position = brushPoint;
    }
    private void Update()
    {
        isBrushing = Input.GetKey(KeyCode.Mouse0);
        inputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
