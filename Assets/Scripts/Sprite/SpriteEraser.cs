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
    [SerializeField, Range(0.1f, 1f)] private float brushSmooth = 1f;
    [SerializeField, Min(0)] private float brushMaxSpeed = 5f;
    [Header("Components")]
    [SerializeField] private Brush brush;
    [SerializeField] private SpriteContainer targetSprite;


    private Vector2 inputPoint;
    private Vector2 brushPoint;

    private bool isBrushing;


    private void BrushEraseOld()
    {
        Profiler.BeginSample($"Find Collision");

        Vector2 coord = TexMath.PointToTextureCoord(transform.InverseTransformPoint(brushPoint), targetSprite.Sprite);
        int size = Mathf.RoundToInt(targetSprite.Sprite.pixelsPerUnit * brushSize);

        List<Vector2Int> coords = TexMath.OverlapCircle(coord, size, targetSprite.Sprite);
        Profiler.EndSample();

        Profiler.BeginSample($"Paint");
        targetSprite.ClearPixels(coords);
        Profiler.EndSample();
    }
    private void BrushErase()
    {      
        Vector2Int targetCoord = TexMath.PointToTextureCoord(transform.InverseTransformPoint(brushPoint), targetSprite.Sprite).Rounded();

        HashSet<Vector2Int> points = new HashSet<Vector2Int>();

        float ratio = targetSprite.Area.PixelsPerUnit / brush.Area.PixelsPerUnit;
        float scaleX = brush.Area.Scale.x * ratio;
        float scaleY = brush.Area.Scale.y * ratio;

        Profiler.BeginSample($"Calculate");

        foreach (Vector2Int point in brush.Area.Points)
        {
            points.Add((point - brush.Area.TextureCenter).Multilied(scaleX, scaleY) + targetCoord);
        }

        IEnumerable<Vector2Int> result = targetSprite.Area.Intercept(points);


        Profiler.EndSample();

        Profiler.BeginSample($"Set");

        targetSprite.ClearPixels(result);

        Profiler.EndSample();
    }
    private void MoveBrush()
    {
        if (isBrushing)
        {
            BrushErase();
        }

        Vector2 prev = brushPoint;
        brushPoint = Vector2.Lerp(brush.Position, inputPoint, brushSmooth);
        Vector2 velocity = (brushPoint - prev) / Time.fixedDeltaTime;
        if(velocity.magnitude > brushMaxSpeed)
        {
            brushPoint = prev + velocity.normalized * brushMaxSpeed * Time.fixedDeltaTime;
        }

        brush.Position = brushPoint;
    }


    public void UpdateBrush(Vector2 point, bool isBrushing)
    {
        this.isBrushing = isBrushing;

        inputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }


    private void FixedUpdate()
    {
        MoveBrush();
    }
    private void Update()
    {
        UpdateBrush(Input.mousePosition, Input.GetKey(KeyCode.Mouse0));
    }
}
