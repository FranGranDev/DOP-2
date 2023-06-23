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
        public static List<TextureHit> OverlapCircle(Vector2 coords, float radius, Sprite sprite)
        {
            float sqrRadius = radius * radius;

            List<TextureHit> points = new List<TextureHit>();

            int minX = Mathf.Max(Mathf.FloorToInt(coords.x - radius), 0);
            int maxX = Mathf.Min(Mathf.CeilToInt(coords.x + radius), sprite.texture.width);
            int minY = Mathf.Max(Mathf.FloorToInt(coords.y - radius), 0);
            int maxY = Mathf.Min(Mathf.CeilToInt(coords.y + radius), sprite.texture.height);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    float sqrDistance = (x - coords.x) * (x - coords.x) + (y - coords.y) * (y - coords.y);

                    if (sqrDistance < sqrRadius)
                    {
                        points.Add(new TextureHit(Mathf.Sqrt(sqrDistance), x, y));
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


    public struct TextureHit
    {
        public TextureHit(float distance, int x, int y)
        {
            Distance = distance;
            X = x;
            Y = y;
        }

        public float Distance { get; }
        public int X { get; }
        public int Y { get; }

        public bool Equals(TextureHit other)
        {
            return X == other.X && Y == other.Y;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }
    }
}
