using UnityEngine;
using Verse;

namespace GridIt
{
    [StaticConstructorOnStartup]
    internal static class GridTex
    {
        /// <summary>Button icon for the PlaySettings toggle row.</summary>
        public static readonly Texture2D GridToggle =
            ContentFinder<Texture2D>.Get("UI/ToggleGridOverlay");

        /// <summary>
        /// Runtime-generated cell border texture.
        /// White border pixels on a transparent interior.
        /// Regenerated when the user changes border thickness.
        /// </summary>
        public static Texture2D CellBorder { get; private set; }

        private const int TexSize = 64;

        /// <summary>Max border width in pixels on the 64×64 texture.
        /// Must match the slider max in Settings.cs.</summary>
        public const int MaxBorderWidth = 20;

        static GridTex()
        {
            int thickness = GridIt_Mod.Settings?.BorderThickness ?? 3;
            CellBorder = GenerateBorderTexture(thickness);
        }

        /// <summary>
        /// Regenerate the border texture with a new pixel width.
        /// Called from GridOverlayRenderer when settings change.
        /// </summary>
        public static void RegenerateBorder(int borderWidth)
        {
            // Destroy the old texture to avoid a GPU leak.
            if (CellBorder != null)
                Object.Destroy(CellBorder);

            CellBorder = GenerateBorderTexture(borderWidth);
        }

        private static Texture2D GenerateBorderTexture(int borderWidth)
        {
            borderWidth = Mathf.Clamp(borderWidth, 1, MaxBorderWidth);

            var tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[TexSize * TexSize];
            var borderPixel = new Color32(255, 255, 255, 255);
            var clearPixel = new Color32(0, 0, 0, 0);

            for (int y = 0; y < TexSize; y++)
            {
                for (int x = 0; x < TexSize; x++)
                {
                    bool isBorder = x < borderWidth || x >= TexSize - borderWidth
                                 || y < borderWidth || y >= TexSize - borderWidth;
                    pixels[y * TexSize + x] = isBorder ? borderPixel : clearPixel;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return tex;
        }
    }
}
