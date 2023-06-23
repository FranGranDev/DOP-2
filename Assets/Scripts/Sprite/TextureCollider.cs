using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utils;
using NaughtyAttributes;
using System.Linq;
using System;
using UnityEngine.Profiling;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class TextureCollider : MonoBehaviour
{
    [SerializeField, Min(1)] private int simplify;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;

    private Texture2D texture;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        texture = spriteRenderer.sprite.texture;
    }
    private void Start()
    {
        //AdjustCollider();
    }


    private void UpdateCollider()
    {
        List<HashSet<Vector2Int>> pixelIslands = FindPixelIslands(texture);

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
        Profiler.BeginSample(nameof(CreateCollider));


        Vector2[] vertex = new Vector2[pixels.Length];

        for(int i = 0; i < pixels.Length; i++)
        {
            Vector2 localPoint = TexMath.TextureCoordToPoint(pixels[i], spriteRenderer.sprite);
            Vector3 worldPoint = transform.TransformPoint(localPoint);

            vertex[i] = worldPoint;

        }

        polygonCollider.SetPath(index, vertex);

        Profiler.EndSample();
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
    private List<HashSet<Vector2Int>> FindPixelIslands(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        Color[] pixels = texture.GetPixels();

        bool[] visited = new bool[width * height];
        List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();

        for (int y = 0; y < height; y += simplify)
        {
            for (int x = 0; x < width; x += simplify)
            {
                int index = x + y * width;

                if (!visited[index] && pixels[index].a > 0.5f)
                {
                    HashSet<Vector2Int> islandPixels = new HashSet<Vector2Int>();

                    Stack<int> stack = new Stack<int>();
                    stack.Push(index);

                    while (stack.Count > 0)
                    {
                        int currentIndex = stack.Pop();

                        int currentX = currentIndex % width;
                        int currentY = currentIndex / width;

                        if (currentX < 0 || currentX >= width || currentY < 0 || currentY >= height || visited[currentIndex] || pixels[currentIndex].a <= 0f)
                        {
                            continue;
                        }

                        visited[currentIndex] = true;
                        islandPixels.Add(new Vector2Int(currentX, currentY));


                        // Помещаем соседние пиксели на стек для обработки
                        stack.Push(currentIndex + simplify);     // Правый пиксель
                        stack.Push(currentIndex - simplify);     // Левый пиксель
                        stack.Push(currentIndex + width * simplify); // Верхний пиксель
                        stack.Push(currentIndex - width * simplify); // Нижний пиксель
                    }


                    islands.Add(islandPixels);
                }
            }
        }

        return islands;
    }


    private void FixedUpdate()
    {
        UpdateCollider();
    }
}
