using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using NaughtyAttributes;
using System.Linq;
using System;
using UnityEngine.Profiling;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(SpriteContainer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class TextureCollider : MonoBehaviour
{
    [SerializeField] private bool isEnabled = true;
    [Space]
    [SerializeField, Min(1)] private int simplify;
    [SerializeField] private float minUpdateTime;

    private SpriteContainer spriteContainer;
    private PolygonCollider2D polygonCollider;

    private float prevUpdateTime = float.MinValue;
    private bool updateCalled;

    public SpriteContainer Attached
    {
        get => spriteContainer;
    }
    
    private void Awake()
    {
        spriteContainer = GetComponent<SpriteContainer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        spriteContainer.OnUpdated += CallUpdate;
    }
    private void Start()
    {
        if(isEnabled)
        {
            UpdateCollider();
        }
    }


    private void CallUpdate()
    {
        updateCalled = true;
    }
    private void UpdateCollider()
    {
        if (!isEnabled)
            return;

        Profiler.BeginSample("Islands");
        List<HashSet<Vector2Int>> pixelIslands = FindPixelIslands(spriteContainer.Pixels, spriteContainer.Size);

        Profiler.EndSample();
        polygonCollider.pathCount = pixelIslands.Count;

        int index = 0;

        foreach (HashSet<Vector2Int> island in pixelIslands)
        {
            UpdatePart(island, index);

            index++;
        }
    }
    private void UpdatePart(HashSet<Vector2Int> island, int index)
    {
        Vector2Int[] texVertex = FindPixelBoundary(island);

        CreateCollider(index, texVertex);
    }


    private void CreateCollider(int index, Vector2Int[] pixels)
    {
        Vector2[] vertex = new Vector2[pixels.Length];

        for(int i = 0; i < pixels.Length; i++)
        {
            Vector2 localPoint = TexMath.TextureCoordToPoint(pixels[i], spriteContainer.Sprite);
            Vector3 worldPoint = transform.TransformPoint(localPoint);

            vertex[i] = worldPoint;

        }

        polygonCollider.SetPath(index, vertex);
    }

    public Vector2Int[] FindPixelBoundary(HashSet<Vector2Int> pixels)
    {
        List<Vector2Int> border = new List<Vector2Int>();
        Vector2Int currentPixel = pixels.First();
        border.Add(currentPixel);
        Vector2Int nextPixel = currentPixel;
        Vector2Int previousDirection = new Vector2Int(0, 0);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, -1) * simplify,
            new Vector2Int(1, -1) * simplify,
            new Vector2Int(1, 0) * simplify,
            new Vector2Int(1, 1) * simplify,
            new Vector2Int(0, 1) * simplify,
            new Vector2Int(-1, 1) * simplify,
            new Vector2Int(-1, 0) * simplify,
            new Vector2Int(-1, -1) * simplify,
        };

        do
        {
            int directionIndex = Array.IndexOf(directions, previousDirection);

            for (int i = 1; i <= directions.Length; i++)
            {
                int index = (directionIndex + i) % directions.Length;
                nextPixel = currentPixel + directions[index];

                if (pixels.Contains(nextPixel))
                {
                    border.Add(nextPixel);
                    previousDirection = directions[(index + directions.Length - 2) % directions.Length];
                    break;
                }
            }

            currentPixel = nextPixel;
        }
        while (nextPixel != border[0]);

        return border.ToArray();
    }

    private List<HashSet<Vector2Int>> FindPixelIslands(Color[] pixels, Vector2Int size)
    {
        int width = size.x;
        int height = size.y;
        int[] labels = new int[width * height];
        int currentLabel = 1;
        List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();

        for (int y = 0; y < height; y += simplify)
        {
            for (int x = 0; x < width; x += simplify)
            {
                int index = x + y * width;

                if (pixels[index].a > 0.5f && labels[index] == 0)
                {
                    HashSet<Vector2Int> islandPixels = new HashSet<Vector2Int>();
                    LabelConnectedComponent(pixels, labels, width, height, x, y, currentLabel, islandPixels);
                    islands.Add(islandPixels);
                    currentLabel++;
                }
            }
        }

        return islands;
    }

    private void LabelConnectedComponent(Color[] pixels, int[] labels, int width, int height, int x, int y, int currentLabel, HashSet<Vector2Int> islandPixels)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int startPixel = new Vector2Int(x, y);
        queue.Enqueue(startPixel);
        labels[x + y * width] = currentLabel;
        islandPixels.Add(startPixel);

        while (queue.Count > 0)
        {
            Vector2Int pixel = queue.Dequeue();
            x = pixel.x;
            y = pixel.y;

            EnqueueIfValidPixel(queue, labels, pixels, width, height, x - simplify, y, currentLabel, islandPixels);
            EnqueueIfValidPixel(queue, labels, pixels, width, height, x + simplify, y, currentLabel, islandPixels);
            EnqueueIfValidPixel(queue, labels, pixels, width, height, x, y - simplify, currentLabel, islandPixels);
            EnqueueIfValidPixel(queue, labels, pixels, width, height, x, y + simplify, currentLabel, islandPixels);
        }
    }

    private void EnqueueIfValidPixel(Queue<Vector2Int> queue, int[] labels, Color[] pixels, int width, int height, int x, int y, int currentLabel, HashSet<Vector2Int> islandPixels)
    {
        if (x >= 0 && x < width && y >= 0 && y < height && pixels[x + y * width].a > 0.5f && labels[x + y * width] == 0)
        {
            Vector2Int pixel = new Vector2Int(x, y);
            queue.Enqueue(pixel);
            labels[x + y * width] = currentLabel;
            islandPixels.Add(pixel);
        }
    }



    private void Update()
    {
        UpdateCollider();

        if (updateCalled && Time.time > prevUpdateTime + minUpdateTime)
        {
            UpdateCollider();

            prevUpdateTime = Time.time;
            updateCalled = false;
        }
    }
}
