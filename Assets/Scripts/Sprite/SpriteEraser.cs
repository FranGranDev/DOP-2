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
    [SerializeField] private SpriteContainer sprite;


    private Vector2 inputPoint;
    private Vector2 brushPoint;

    private bool isBrushing;


    private void BrushErase()
    {
        Profiler.BeginSample($"Find Collision");

        Vector2 coord = TexMath.PointToTextureCoord(transform.InverseTransformPoint(brushPoint), sprite.Sprite);
        int size = Mathf.RoundToInt(sprite.Sprite.pixelsPerUnit * brushSize);

        List<Vector2Int> coords = TexMath.OverlapCircle(coord, size, sprite.Sprite);
        Profiler.EndSample();

        Profiler.BeginSample($"Paint");
        sprite.ClearPixels(coords);
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
