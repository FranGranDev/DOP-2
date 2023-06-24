using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Game.Utils;
using System.Threading.Tasks;

public class SpriteEraser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(1f, 10)] private int iterations = 1;
    [Header("Brush Settings")]
    [SerializeField, Range(0.1f, 1f)] private float brushSmooth = 1f;
    [SerializeField, Min(0)] private float brushMaxSpeed = 5f;
    [Header("Components")]
    [SerializeField] private Brush brush;
    [SerializeField] private SpriteContainer targetSprite;


    private ICoordTransform coordTransform;


    private Vector2 inputPoint;
    private Vector2 brushPoint;
    private Vector2 brushPrev;

    private bool isBrushing;


    private void BrushErase()
    {
        HashSet<Vector2Int> points = coordTransform.Execute(brushPrev, brushPoint, iterations, targetSprite, brush.Area);

        targetSprite.ClearPixels(points);
    }
    private void MoveBrush()
    {
        brushPrev = brushPoint;
        brushPoint = Vector2.Lerp(brush.Position, inputPoint, brushSmooth);
        Vector2 velocity = (brushPoint - brushPrev) / Time.fixedDeltaTime;
        if(velocity.magnitude > brushMaxSpeed)
        {
            brushPoint = brushPrev + velocity.normalized * brushMaxSpeed * Time.fixedDeltaTime;
        }

        brush.Position = brushPoint;

        if (isBrushing)
        {
            BrushErase();
        }
    }

    public void UpdateBrush(Vector2 point, bool isBrushing)
    {
        this.isBrushing = isBrushing;

        inputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }


    private void Start()
    {
        coordTransform = new FastCoordTransform(brush.Area);
        //coordTransform = new CoordTransform();
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
