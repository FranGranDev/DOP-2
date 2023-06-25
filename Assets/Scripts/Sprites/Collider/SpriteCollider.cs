using Cysharp.Threading.Tasks;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game.Sprites
{
    [RequireComponent(typeof(SpriteContainer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SpriteCollider : MonoBehaviour
    {
        [SerializeField] private bool isEnabled = true;
        [Space]
        [SerializeField, Min(1)] private int simplify;
        [SerializeField, Min(1)] private int maxFrames;

        private SpriteContainer spriteContainer;
        private PolygonCollider2D polygonCollider;

        private bool updateCalled;
        private bool isUpdating;

        public SpriteContainer Attached
        {
            get => spriteContainer;
        }

        private void Awake()
        {
            spriteContainer = GetComponent<SpriteContainer>();
            polygonCollider = GetComponent<PolygonCollider2D>();

            spriteContainer.OnUpdated += CallUpdate;
            spriteContainer.OnInitialized += CallUpdate;
        }

        private void CallUpdate()
        {
            updateCalled = true;
        }
        private async void UpdateCollider()
        {
            isUpdating = true;

            List<HashSet<Vector2Int>> pixelIslands = await FindPixelIslands(spriteContainer.Pixels, spriteContainer.Size);

            polygonCollider.pathCount = pixelIslands.Count;

            int index = 0;

            foreach (HashSet<Vector2Int> island in pixelIslands)
            {
                UpdatePart(island, index);

                index++;
            }

            isUpdating = false;
        }
        private void UpdatePart(HashSet<Vector2Int> island, int index)
        {
            List<Vector2Int> texVertex = FindPixelBoundary(island);

            CreateCollider(index, texVertex);
        }


        private void CreateCollider(int index, List<Vector2Int> pixels)
        {
            Vector2[] vertex = new Vector2[pixels.Count];

            for (int i = 0; i < pixels.Count; i++)
            {
                vertex[i] = TexMath.TextureCoordToPoint(pixels[i], spriteContainer.Sprite);
            }

            polygonCollider.SetPath(index, vertex);
        }

        public List<Vector2Int> FindPixelBoundary(HashSet<Vector2Int> pixels)
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

            return border;
        }

        private async UniTask<List<HashSet<Vector2Int>>> FindPixelIslands(Color32[] pixels, Vector2Int size)
        {
            int width = size.x;
            int height = size.y;
            int[] labels = new int[width * height];
            int currentLabel = 1;
            List<HashSet<Vector2Int>> islands = new List<HashSet<Vector2Int>>();


            int delay = Mathf.RoundToInt((float)height / (float)(maxFrames * simplify));


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

                if (y % delay == 0)
                {
                    await UniTask.Yield();
                }
            }

            return islands;
        }
        private void LabelConnectedComponent(Color32[] pixels, int[] labels, int width, int height, int x, int y, int currentLabel, HashSet<Vector2Int> islandPixels)
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
        private void EnqueueIfValidPixel(Queue<Vector2Int> queue, int[] labels, Color32[] pixels, int width, int height, int x, int y, int currentLabel, HashSet<Vector2Int> islandPixels)
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
            if (isEnabled && !isUpdating && updateCalled)
            {
                UpdateCollider();

                updateCalled = false;
            }
        }
    }
}