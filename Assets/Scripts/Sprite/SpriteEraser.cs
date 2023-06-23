using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using System.Threading.Tasks;

public class SpriteEraser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float brushSize = 1f;
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;


    private Texture2D texture;
    private Sprite sprite;
    private MeshCollider meshCollider;



    private void Awake()
    {
        CopySprite();
    }


    private void CopySprite()
    {
        sprite = spriteRenderer.sprite.Copy();
        spriteRenderer.sprite = sprite;
        texture = sprite.texture;
    }

    private void BrushErase(Vector2 point)
    {
        Vector2 localPoint = transform.InverseTransformPoint(point);
        Vector2 textureCoord = TexMath.PointToTextureCoord(localPoint, sprite);

        int size = Mathf.RoundToInt(sprite.pixelsPerUnit * brushSize);

        List<TextureHit> coords = TexMath.OverlapCircle(textureCoord, size, sprite);

        foreach(TextureHit coord in coords)
        {
            texture.SetPixel(coord.X, coord.Y, Color.clear);
        }

        texture.Apply();
    }



    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collider = Physics2D.OverlapCircle(point, brushSize);

            if (collider)
            {
                BrushErase(point);
            }
        }
    }

}
