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
    [SerializeField, Min(1)] private int simplify;

    private SpriteContainer spriteContainer;
    private PolygonCollider2D polygonCollider;

    
    private void Awake()
    {
        spriteContainer = GetComponent<SpriteContainer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        spriteContainer.OnUpdated += UpdateCollider;
    }
    private void Start()
    {
        if(isEnabled)
        {
            UpdateCollider();
        }
    }

    private void UpdateCollider()
    {
        if (!isEnabled)
            return;

        List<HashSet<Vector2Int>> pixelIslands = FindPixelIslands(spriteContainer.Pixels, spriteContainer.Size);

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
        bool[] visited = new bool[size.x * size.y];
        List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();

        int width = size.x;
        int height = size.y;

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

                        // Проверяем соседние пиксели только если текущий пиксель не находится на границе изображения
                        if (currentX > 0)
                            stack.Push(currentIndex - simplify);
                        if (currentX < width - 1)
                            stack.Push(currentIndex + simplify);
                        if (currentY > 0)
                            stack.Push(currentIndex - width * simplify);
                        if (currentY < height - 1)
                            stack.Push(currentIndex + width * simplify);
                    }

                    islands.Add(islandPixels);
                }
            }
        }

        return islands;
    }

    private void a()
    {
        //bool[] visited = new bool[size.x * size.y];
        //List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();

        //for (int y = 0; y < size.y; y += simplify)
        //{
        //    for (int x = 0; x < size.x; x += simplify)
        //    {
        //        int index = x + y * size.x;

        //        if (!visited[index] && pixels[index].a > 0.5f)
        //        {
        //            HashSet<Vector2Int> islandPixels = new HashSet<Vector2Int>();

        //            Stack<int> stack = new Stack<int>();
        //            stack.Push(index);

        //            while (stack.Count > 0)
        //            {
        //                int currentIndex = stack.Pop();

        //                int currentX = currentIndex % size.x;
        //                int currentY = currentIndex / size.x;

        //                if (currentX < 0 || currentX >= size.x || currentY < 0 || currentY >= size.y || visited[currentIndex] || pixels[currentIndex].a <= 0f)
        //                {
        //                    continue;
        //                }

        //                visited[currentIndex] = true;
        //                islandPixels.Add(new Vector2Int(currentX, currentY));


        //                stack.Push(currentIndex + simplify);
        //                stack.Push(currentIndex - simplify);
        //                stack.Push(currentIndex + size.x * simplify);
        //                stack.Push(currentIndex - size.x * simplify);
        //            }


        //            islands.Add(islandPixels);
        //        }
        //    }
        //}

        //return islands;
    }
}
