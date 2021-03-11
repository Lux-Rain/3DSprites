using UnityEditor;
using UnityEngine;

namespace DiazTeo.Editor.Utils
{
    public static class EditorSpriteGUIUtility
    {
        public enum FitMode
        {
            BestFit,
            FitHorizontal,
            FitVertical,
            Fill,
            Tiled
        }

        private static Material s_SpriteMaterial;
        public static Material spriteMaterial
        {
            get
            {
                if (s_SpriteMaterial == null)
                {
                    s_SpriteMaterial = new Material(Shader.Find("Hidden/InternalSpritesInspector"));
                    s_SpriteMaterial.hideFlags = HideFlags.DontSave;
                }
                s_SpriteMaterial.SetFloat("_AdjustLinearForGamma", PlayerSettings.colorSpace == ColorSpace.Linear ? 1.0f : 0.0f);
                return s_SpriteMaterial;
            }
        }

        public static Texture GetOriginalSpriteTexture(Sprite sprite)
        {
            return UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite, false);
        }

        public static Vector2[] GetOriginalSpriteUvs(Sprite sprite)
        {
            return UnityEditor.Sprites.SpriteUtility.GetSpriteUVs(sprite, false);
        }

        public static Texture2D GetTextureFromSingleSprite(Sprite sprite)
        {
            Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                    (int)sprite.textureRect.y,
                                                    (int)sprite.textureRect.width,
                                                    (int)sprite.textureRect.height);

            croppedTexture.alphaIsTransparency = sprite.texture.alphaIsTransparency;
            croppedTexture.filterMode = sprite.texture.filterMode;
            croppedTexture.anisoLevel = sprite.texture.anisoLevel;
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }

        public static Texture2D FlipTexture(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;


            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
            flipped.alphaIsTransparency = original.alphaIsTransparency;
            flipped.filterMode = original.filterMode;
            flipped.anisoLevel = original.anisoLevel;
            flipped.Apply();

            return flipped;
        }


    }
}