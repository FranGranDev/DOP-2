using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Utils
{
    public static class TexMath
    {
        public static Vector2 PointToTextureCoord(Vector2 localPoint, Sprite sprite)
        {
            localPoint *= sprite.pixelsPerUnit;

            Vector2 texurePivot = new Vector2(sprite.rect.x, sprite.rect.y) + sprite.pivot;
            Vector2 texureCoord = texurePivot + localPoint;

            return texureCoord;
        }
        public static Vector2 TextureCoordToPoint(Vector2 coords, Sprite sprite)
        {
            Vector2 texurePivot = new Vector2(sprite.rect.x, sprite.rect.y) + sprite.pivot;

            return (coords - texurePivot) / sprite.pixelsPerUnit;
        }
        public static List<Vector2Int> OverlapCircle(Vector2 coords, int radius, Sprite sprite)
        {
            int sqrRadius = radius * radius;


            int minX = Mathf.Max(Mathf.FloorToInt(coords.x - radius), 0);
            int maxX = Mathf.Min(Mathf.CeilToInt(coords.x + radius), sprite.texture.width);
            int minY = Mathf.Max(Mathf.FloorToInt(coords.y - radius), 0);
            int maxY = Mathf.Min(Mathf.CeilToInt(coords.y + radius), sprite.texture.height);

            int capacity = Mathf.Abs((maxX - minX) * (maxY - minY));
            List<Vector2Int> reuslt = new List<Vector2Int>(capacity);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    float sqrDistance = (x - coords.x) * (x - coords.x) + (y - coords.y) * (y - coords.y);

                    if (sqrDistance < sqrRadius)
                    {
                        reuslt.Add(new Vector2Int(x, y));
                    }
                }
            }

            return reuslt;
        }
        public static List<Vector2Int> OverlapCircleOld(Vector2 coords, int radius, Sprite sprite)
        {
            int width = sprite.texture.width;
            int height = sprite.texture.height;

            List<Vector2Int> pixelCircle = new List<Vector2Int>();
            for(int x = -radius; x <= radius; x++)
            {
                for(int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        pixelCircle.Add(new Vector2Int(x, y));
                    }
                }
            }
            List<Vector2Int> pixelCoords = new List<Vector2Int>();

            Vector2Int centerCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));

            foreach (Vector2Int pixel in pixelCircle)
            {
                Vector2Int result = centerCoords + pixel;
                if (result.x < 0 || result.x >= width || result.y < 0 || result.y >= height)
                    continue;

                pixelCoords.Add(result);
            }

            return pixelCoords;
        }


        public static Vector2Int Multilied(this Vector2Int self, float value)
        {
            return new Vector2Int(Mathf.RoundToInt(self.x * value), Mathf.RoundToInt(self.y * value));
        }
        public static Vector2Int Multilied(this Vector2Int self, float x, float y)
        {
            return new Vector2Int(Mathf.RoundToInt(self.x * x), Mathf.RoundToInt(self.y * y));
        }
        public static Vector2Int Rounded(this Vector2 self)
        {
            return new Vector2Int(Mathf.RoundToInt(self.x), Mathf.RoundToInt(self.y));
        }
        public static Vector2Int Rounded(this Vector3 self)
        {
            return new Vector2Int(Mathf.RoundToInt(self.x), Mathf.RoundToInt(self.y));
        }
        public static Vector3 ToVector3(this Vector2Int self)
        {
            return new Vector3(self.x, self.y, 0);
        }


        public static HashSet<Vector2Int> GetCoords(this Sprite self)
        {
            int width = self.texture.width;
            int height = self.texture.height;

            HashSet<Vector2Int> points = new HashSet<Vector2Int>(width * height);
            Color[] pixels = self.texture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;
                    if(pixels[index].a > 0.5f)
                    {
                        points.Add(new Vector2Int(x, y));
                    }
                }
            }

            return points;
        }


        public static Sprite Copy(this Sprite self)
        {            
            return Sprite.Create(self.texture.Copy(), self.rect, Vector2.one / 2, self.pixelsPerUnit);
        }
        public static Texture2D Copy(this Texture2D self)
        {
            Texture2D copiedTexture = new Texture2D(self.width, self.height, self.format, self.mipmapCount > 1);
            copiedTexture.SetPixels(self.GetPixels());

            copiedTexture.wrapMode = self.wrapMode;
            copiedTexture.filterMode = self.filterMode;
            copiedTexture.anisoLevel = self.anisoLevel;

            copiedTexture.Apply(true);

            return copiedTexture;
        }
    }
}
