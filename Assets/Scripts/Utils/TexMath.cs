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


        public static Vector2Int Devided(this Vector2Int self, float value)
        {
            return new Vector2Int(Mathf.RoundToInt(self.x / value), Mathf.RoundToInt(self.y / value));
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


        public static List<Vector2Int> GetCoords(this Sprite self)
        {
            int width = self.texture.width;
            int height = self.texture.height;

            List<Vector2Int> points = new List<Vector2Int>(width * height);
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
