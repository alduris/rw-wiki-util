using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WikiUtil.BuiltIn
{
    internal static class IconsToolHelper
    {
        public static Texture2D CreatureIcon(string type, int data = 0)
        {
            var icon = new IconSymbol.IconSymbolData { critType = new CreatureTemplate.Type(type, false), intData = data };
            var sprite = new FSprite(CreatureSymbol.SpriteNameOfCreature(icon));
            var color = CreatureSymbol.ColorOfCreature(icon);
            var tex = GetSpriteFromAtlas(sprite);
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] *= color;
            }
            tex.SetPixels(pixels);
            Iconify(tex);
            tex.Apply();
            return tex;
        }

        public static Texture2D ItemIcon(string type, int data = 0)
        {
            var aoType = new AbstractPhysicalObject.AbstractObjectType(type, false);
            var sprite = new FSprite(ItemSymbol.SpriteNameForItem(aoType, data));
            var color = ItemSymbol.ColorForItem(aoType, data);
            var tex = GetSpriteFromAtlas(sprite);
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] *= color;
            }
            tex.SetPixels(pixels);
            Iconify(tex);
            tex.Apply();
            return tex;
        }

        public static Texture2D GetSpriteFromAtlas(FSprite sprite)
        {
            return GetElementFromAtlas(sprite.element);
        }

        public static Texture2D GetElementFromAtlas(FAtlasElement element)
        {
            Texture2D atlasTex = element.atlas.texture as Texture2D;
            if (element.atlas.texture.name != "")
            {
                var oldRT = RenderTexture.active;

                var rt = new RenderTexture(atlasTex.width, atlasTex.height, 32, RenderTextureFormat.ARGB32);
                Graphics.Blit(atlasTex, rt);
                RenderTexture.active = rt;
                atlasTex = new Texture2D(atlasTex.width, atlasTex.height, TextureFormat.ARGB32, false);
                atlasTex.ReadPixels(new Rect(0, 0, atlasTex.width, atlasTex.height), 0, 0);

                RenderTexture.active = oldRT;
            }

            // Get sprite pos and size
            var pos = element.uvRect.position * element.atlas.textureSize;
            var size = element.sourceRect.size;

            // Fix size issues
            if (pos.x + size.x > atlasTex.width) size = new Vector2(atlasTex.width - pos.x, size.y);
            if (pos.y + size.y > atlasTex.height) size = new Vector2(size.x, atlasTex.height - pos.y);

            // Get the texture
            var tex = new Texture2D((int)size.x, (int)size.y, atlasTex.format, 1, false);
            Graphics.CopyTexture(atlasTex, 0, 0, (int)pos.x, (int)pos.y, (int)size.x, (int)size.y, tex, 0, 0, 0, 0);
            return tex;
        }

        public static void CenterTextureInRect(Texture2D texture, int width, int height)
        {
            // Get old pixels
            var pixels = texture.GetPixels();
            var (oldW, oldH) = (texture.width, texture.height);

            // Resize and clear from invalid color
            texture.Resize(width, height);
            Color[] clear = new Color[width * height];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0f, 0f, 0f, 0f);
            texture.SetPixels(clear);

            // Put old image back
            texture.SetPixels(width / 2 - oldW / 2, height / 2 - oldH / 2, oldW, oldH, pixels);
        }

        public static void Iconify(Texture2D texture)
        {
            var w = texture.width + 4;
            var h = texture.height + 4;
            CenterTextureInRect(texture, w, h);

            // Add outline
            Color[] pixels = texture.GetPixels();
            bool[] colored = [.. pixels.Select(x => x.a > 0f)];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (colored[i + j * w])
                    {
                        var col = pixels[i + j * w];
                        pixels[i + j * w] = Color.Lerp(Color.black, new Color(col.r, col.g, col.b), col.a);
                    }
                    else
                    {
                        // Check all 8 neighboring tiles if can do outline
                        bool tl = i > 0 && j > 0 && colored[i - 1 + (j - 1) * w];
                        bool tm = i > 0 && colored[i - 1 + j * w];
                        bool tr = i > 0 && j < h - 1 && colored[i - 1 + (j + 1) * w];
                        bool ml = j > 0 && colored[i + (j - 1) * w];
                        bool mr = j < h - 1 && colored[i + (j + 1) * w];
                        bool bl = i < w - 1 && j > 0 && colored[i + 1 + (j - 1) * w];
                        bool bm = i < w - 1 && colored[i + 1 + j * w];
                        bool br = i < w - 1 && j < h - 1 && colored[i + 1 + (j + 1) * w];
                        if (tl || tm || tr || ml || mr || bl || bm || br)
                        {
                            pixels[i + j * w] = Color.black;
                        }
                    }

                }
            }

            // Fill in holes
            bool[,] floodfill = new bool[w, h];
            bool[,] ffChecked = new bool[w, h];
            Stack<(int x, int y)> toCheck = [];
            toCheck.Push((0, 0)); // always transparent
            while (toCheck.Count > 0)
            {
                var (x, y) = toCheck.Pop();
                if (x < 0 || x >= w || y < 0 || y >= h) continue;
                if (ffChecked[x, y]) continue;

                ffChecked[x, y] = true;
                if (pixels[x + y * w].a == 0f)
                {
                    floodfill[x, y] = true;
                    toCheck.Push((x + 1, y));
                    toCheck.Push((x - 1, y));
                    toCheck.Push((x, y + 1));
                    toCheck.Push((x, y - 1));
                }
            }

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (pixels[i + j * w].a == 0f && !floodfill[i, j])
                    {
                        pixels[i + j * w] = Color.black;
                    }
                }
            }

            // Overwrite texture
            texture.SetPixels(pixels);
        }
    }
}
