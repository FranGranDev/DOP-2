# DOP 2 Game
## Технологии
- Unity 2021.3.20f1
- UniTask
- C# Jobs
- DOTween
## Механика стирания
Механика стирания основывается на трансформации точек пикселей текстуры кисти в пространство пикселей стираемой текстуры и выставление найденным пикселям прозрачного цвета.

**Нахождения стираемых объектов  | [SpriteEraser.cs](Assets/Scripts/Sprites/Eraser/SpriteEraser.cs)**
```csharp
private void TryBrushErase()
{
    Collider2D[] colliders = Physics2D.OverlapBoxAll(brushPoint, brush.Size, 0);
    foreach(Collider2D collider in colliders)
    {
        if (collider.TryGetComponent(out SpriteContainer sprite))
        {
            BrushErase(sprite);

            if (!visitedSprites.Contains(sprite))
            {
                visitedSprites.Push(sprite);
            }
        }
    }
}
```

**Стирание объекта | [SpriteEraser.cs](Assets/Scripts/Sprites/Eraser/SpriteEraser.cs)**
<br>
Для стиранию применяется итеративных подход для избегания пробелов и срубов при быстром перемещении кисти. Стирание происходит в области пикселей кисти от предыдущей позиции кисти к текущей

```csharp
private void BrushErase(SpriteContainer sprite)
{
    float distance = (brushPoint - brushPrev).magnitude;
    int iterations = Mathf.Clamp(Mathf.RoundToInt(distance / iterationThreshold), 1, maxIterations);


    for (int i = 0; i < iterations; i++)
    {
        Vector3 point = Vector3.Lerp(brushPrev, brushPoint, (float)i / (float)maxIterations);

        IEnumerable<Vector2Int> points = coordTransform.Execute(point, sprite, brush.Area);

        sprite.ClearPixels(points);
    }
}
```

**Преобразование позиций пикселей | [CoordTransform.cs](Assets/Scripts/Sprites/Calculation/CoordTransform.cs)** 
<br>
Для быстрого преобразования применяется ``C# Jobs``. Массив ``NativeArray<Vector2Int> points`` представляет массив точек кисти, а ``NativeArray<Vector2Int> result`` результирующий массив точек, переведенных в пространство точек текстуры стираемого объекта.

```csharp
public IEnumerable<Vector2Int> Execute(Vector3 point, SpriteContainer target, SpriteArea brush)
{
    Vector2Int targetCoordStart = TexMath.PointToTextureCoord(target.transform.InverseTransformPoint(point), target.Sprite).Rounded();

    float ratio = target.Area.PixelsPerUnit / brush.PixelsPerUnit;
    float scaleX = brush.Scale.x * ratio;
    float scaleY = brush.Scale.y * ratio;


    PixelsCalculation calculation = new PixelsCalculation()
    {
        coords = targetCoordStart,

        points = brushPoints,

        width = target.Area.Width,
        height = target.Area.Height,

        scaleX = scaleX,
        scaleY = scaleY,

        result = output,
    };

    JobHandle jobHandle = calculation.Schedule(brushPoints.Length, 64);
    jobHandle.Complete();

    return output;
}

private struct PixelsCalculation : IJobParallelFor
{
    [ReadOnly] public Vector2Int coords;

    [ReadOnly] public NativeArray<Vector2Int> points;


    [ReadOnly] public int width;
    [ReadOnly] public int height;

    [ReadOnly] public float scaleX;
    [ReadOnly] public float scaleY;

    [WriteOnly] public NativeArray<Vector2Int> result;

    public void Execute(int index)
    {
        Vector2Int transformed = points[index].Multilied(scaleX, scaleY) + coords;
        if (transformed.x >= 0 && transformed.x < width && transformed.y >= 0 && transformed.y < height)
        {
            result[index] = transformed;
        }
        else
        {
            result[index] = default;
        }
    }
}
```

**Стирание найденных пикселей | [SpriteContainer.cs](Assets/Scripts/Sprites/Sprite/SpriteContainer.cs)**
```csharp
public void ClearPixels(IEnumerable<Vector2Int> coords)
{
    int width = texture.width;
    Color32 color = new Color32(0, 0, 0, 0);
    foreach (Vector2Int coord in coords)
    {
        currantPixels[coord.x + coord.y * width] = color;
    }

    texture.SetPixels32(currantPixels);
    texture.Apply();

    OnUpdated?.Invoke();
}
```

## Механика создания коллайдера
Коллайдер генерируется по границе пикселей текстуры объекта, граница определяется прозрачностью пикселей. Для нахождения отдельных остравков пикселей используется алгоритм *Deep First Search*.

**Нахождение остравков пикселей | [SpriteCollider.cs](Assets/Scripts/Sprites/Collider/SpriteCollider.cs)**
<br>
Так как алгоритм довольно требовательный, рассчет идет на протяжении `maxFrames` кадров. `Simplify` используется для уровня качества генерируемого коллайдера. Если `Simplify = 1`. Коллайдер генерируется по каждому пикселю текстуры, если `Simplify = 2`, то по каждому второму и тд.
```csharp
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
```
**Нахождение границы каждого остравка  | [SpriteCollider.cs](Assets/Scripts/Sprites/Collider/SpriteCollider.cs)**
<br>
Для нахождения границы каждого найденного остравка из `FindPixelIslands(Color32[] pixels, Vector2Int size)` используется алгоритм *Boundary Tracing*. Он находит стартовый пиксель и идет вдоль границы, проверяя находится ли пиксель в текущем направлении. Если пиксель не найден, напрвление смещается в правую сторону. Цикл продолжается, пока снова не будет найдена первая найденная точка, то есть граница замкнулась.

```csharp
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
```

**Создание коллайдера  | [SpriteCollider.cs](Assets/Scripts/Sprites/Collider/SpriteCollider.cs)**
```csharp
private async void UpdateCollider()
{
    isUpdating = true;

    List<HashSet<Vector2Int>> pixelIslands = await FindPixelIslands(spriteContainer.Pixels, spriteContainer.Size);

    polygonCollider.pathCount = pixelIslands.Count;

    int index = 0;

    foreach (HashSet<Vector2Int> island in pixelIslands)
    {
        UpdateIsland(island, index);

        index++;
    }

    isUpdating = false;
}

private void UpdateIsland(HashSet<Vector2Int> island, int index)
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
```

## Производительность

Стирание объекта кистью при `MaxIteration = 5` занимает примерно `4-5 ms` на кадр. При `MaxIteration = 1` занимает меньше `1ms`.

Обновление коллайдера на текстуре `512x512` при `Simplify = 3` и `maxFrames = 10` стабильно работает и занимает примерно `5-6 ms` на каждом кадре.
