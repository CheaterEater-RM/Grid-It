using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace GridIt
{
    public class GridIt_Settings : ModSettings
    {
        // Color (RGB 0–1, user-adjustable).
        public float ColorR = 0.5f;
        public float ColorG = 0.5f;
        public float ColorB = 0.5f;
        public float GridOpacity = 0.30f;

        // Border thickness 1–MaxBorderWidth, pixel width on the 64×64 cell texture.
        public int BorderThickness = 3;

        // Hide the toggle button from the play settings row.
        public bool HideToggleButton = false;

        // Defaults for reset.
        private const float DefaultR = 0.5f;
        private const float DefaultG = 0.5f;
        private const float DefaultB = 0.5f;
        private const float DefaultOpacity = 0.30f;
        private const int DefaultThickness = 3;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ColorR, "ColorR", DefaultR);
            Scribe_Values.Look(ref ColorG, "ColorG", DefaultG);
            Scribe_Values.Look(ref ColorB, "ColorB", DefaultB);
            Scribe_Values.Look(ref GridOpacity, "GridOpacity", DefaultOpacity);
            Scribe_Values.Look(ref BorderThickness, "BorderThickness", DefaultThickness);
            Scribe_Values.Look(ref HideToggleButton, "HideToggleButton", false);
        }

        /// <summary>Full RGBA color for the grid material.</summary>
        public Color GetGridColor()
        {
            return new Color(
                Mathf.Clamp01(ColorR),
                Mathf.Clamp01(ColorG),
                Mathf.Clamp01(ColorB),
                Mathf.Clamp01(GridOpacity));
        }

        /// <summary>RGB at full alpha, for the settings preview swatch.</summary>
        public Color GetSwatchColor()
        {
            return new Color(
                Mathf.Clamp01(ColorR),
                Mathf.Clamp01(ColorG),
                Mathf.Clamp01(ColorB),
                1f);
        }

        /// <summary>Set RGB from a preset, leave opacity and thickness alone.</summary>
        public void ApplyPreset(float r, float g, float b)
        {
            ColorR = r;
            ColorG = g;
            ColorB = b;
        }

        /// <summary>Get hex string from current RGB.</summary>
        public string GetHexColor()
        {
            int r = Mathf.RoundToInt(Mathf.Clamp01(ColorR) * 255f);
            int g = Mathf.RoundToInt(Mathf.Clamp01(ColorG) * 255f);
            int b = Mathf.RoundToInt(Mathf.Clamp01(ColorB) * 255f);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>Try to parse a hex color string (with or without #) and apply it.</summary>
        public bool TryApplyHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return false;
            hex = hex.TrimStart('#');
            if (hex.Length != 6) return false;

            if (int.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out int r)
             && int.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out int g)
             && int.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out int b))
            {
                ColorR = r / 255f;
                ColorG = g / 255f;
                ColorB = b / 255f;
                return true;
            }
            return false;
        }

        public void ResetToDefaults()
        {
            ColorR = DefaultR;
            ColorG = DefaultG;
            ColorB = DefaultB;
            GridOpacity = DefaultOpacity;
            BorderThickness = DefaultThickness;
            HideToggleButton = false;
        }
    }

    public class GridIt_Mod : Mod
    {
        public static GridIt_Settings Settings { get; private set; }

        // TextFieldNumeric buffers (IMGUI needs persistent string buffers).
        private string bufR, bufG, bufB, bufOpacity;
        private string hexBuffer;
        private static readonly Regex HexValidator = new Regex(@"^[0-9A-Fa-f#]*$");

        public GridIt_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<GridIt_Settings>();
            SyncBuffers();
        }

        private void SyncBuffers()
        {
            bufR = Settings.ColorR.ToString("F2");
            bufG = Settings.ColorG.ToString("F2");
            bufB = Settings.ColorB.ToString("F2");
            bufOpacity = Settings.GridOpacity.ToString("F2");
            hexBuffer = Settings.GetHexColor();
        }

        public override string SettingsCategory() => "Grid It";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            // --- Quick presets ---
            listing.Label("GridIt_Settings_Presets".Translate());
            listing.Gap(4f);

            var presetRect = listing.GetRect(28f);
            float buttonWidth = presetRect.width / 3f - 4f;

            if (Widgets.ButtonText(new Rect(presetRect.x, presetRect.y, buttonWidth, 28f),
                    "GridIt_Settings_Color_Gray".Translate()))
            {
                Settings.ApplyPreset(0.5f, 0.5f, 0.5f);
                SyncBuffers();
                GridOverlayRenderer.MarkMaterialDirty();
            }
            if (Widgets.ButtonText(new Rect(presetRect.x + buttonWidth + 6f, presetRect.y, buttonWidth, 28f),
                    "GridIt_Settings_Color_Black".Translate()))
            {
                Settings.ApplyPreset(0f, 0f, 0f);
                SyncBuffers();
                GridOverlayRenderer.MarkMaterialDirty();
            }
            if (Widgets.ButtonText(new Rect(presetRect.x + (buttonWidth + 6f) * 2f, presetRect.y, buttonWidth, 28f),
                    "GridIt_Settings_Color_White".Translate()))
            {
                Settings.ApplyPreset(1f, 1f, 1f);
                SyncBuffers();
                GridOverlayRenderer.MarkMaterialDirty();
            }

            listing.Gap(8f);

            // --- Compact RGB rows: [Label 24px] [Slider flex] [Input 50px] ---
            const float labelWidth = 24f;
            const float inputWidth = 50f;
            const float gap = 4f;
            const float rowHeight = 28f;

            bool colorChanged = false;
            colorChanged |= DrawColorRow(listing, "R", ref Settings.ColorR, ref bufR,
                                         labelWidth, inputWidth, gap, rowHeight);
            colorChanged |= DrawColorRow(listing, "G", ref Settings.ColorG, ref bufG,
                                         labelWidth, inputWidth, gap, rowHeight);
            colorChanged |= DrawColorRow(listing, "B", ref Settings.ColorB, ref bufB,
                                         labelWidth, inputWidth, gap, rowHeight);

            if (colorChanged)
            {
                hexBuffer = Settings.GetHexColor();
                GridOverlayRenderer.MarkMaterialDirty();
            }

            listing.Gap(4f);

            // --- Hex input + swatch on one row ---
            var hexRow = listing.GetRect(24f);
            // # label
            var hashRect = new Rect(hexRow.x, hexRow.y, 14f, hexRow.height);
            Widgets.Label(hashRect, "#");
            // Hex text field
            var hexFieldRect = new Rect(hashRect.xMax, hexRow.y, 60f, hexRow.height);

            string controlName = "GridIt_HexField";
            GUI.SetNextControlName(controlName);
            string newHex = Widgets.TextField(hexFieldRect, hexBuffer, 6, HexValidator);

            if (newHex != hexBuffer)
            {
                hexBuffer = newHex;
                if (hexBuffer.Length == 6 && Settings.TryApplyHex(hexBuffer))
                {
                    bufR = Settings.ColorR.ToString("F2");
                    bufG = Settings.ColorG.ToString("F2");
                    bufB = Settings.ColorB.ToString("F2");
                    GridOverlayRenderer.MarkMaterialDirty();
                }
            }

            // Swatch
            var swatchRect = new Rect(hexFieldRect.xMax + 8f, hexRow.y, 40f, hexRow.height);
            Widgets.DrawBoxSolid(swatchRect, Settings.GetSwatchColor());
            Widgets.DrawBox(swatchRect);

            listing.GapLine();

            // --- Opacity: same compact row ---
            DrawSliderRow(listing, "GridIt_Settings_Opacity_Label".Translate(),
                ref Settings.GridOpacity, ref bufOpacity,
                0f, 1f, 60f, inputWidth, gap, rowHeight,
                GridOverlayRenderer.MarkMaterialDirty);

            listing.GapLine();

            // --- Border thickness ---
            listing.Label("GridIt_Settings_Thickness".Translate(
                Settings.BorderThickness.ToString()));
            int newThickness = (int)listing.Slider(Settings.BorderThickness, 1f, GridTex.MaxBorderWidth);
            if (newThickness != Settings.BorderThickness)
            {
                Settings.BorderThickness = newThickness;
                GridOverlayRenderer.MarkTextureDirty();
            }

            listing.GapLine();

            // --- Hide toggle button ---
            bool hideToggleBefore = Settings.HideToggleButton;
            listing.CheckboxLabeled("GridIt_Settings_HideButton".Translate(),
                ref Settings.HideToggleButton,
                "GridIt_Settings_HideButton_Tooltip".Translate());
            if (Settings.HideToggleButton && !hideToggleBefore)
                GridOverlayRenderer.DisableAllOverlays();

            listing.GapLine();

            if (listing.ButtonText("GridIt_Settings_Reset".Translate()))
            {
                Settings.ResetToDefaults();
                SyncBuffers();
                GridOverlayRenderer.MarkTextureDirty();
            }

            listing.End();
        }

        /// <summary>
        /// Draw a single compact color channel row: [Label] [Slider] [NumericInput].
        /// The slider is authoritative while dragged; the text field is authoritative
        /// when the user types. We resolve the conflict by checking which one changed
        /// relative to the incoming value.
        /// </summary>
        private static bool DrawColorRow(Listing_Standard listing, string label,
            ref float value, ref string buffer,
            float labelWidth, float inputWidth, float gap, float rowHeight)
        {
            var row = listing.GetRect(rowHeight);
            float prev = value;

            // Label
            var labelRect = new Rect(row.x, row.y, labelWidth, row.height);
            Widgets.Label(labelRect, label);

            // Input field (right-aligned)
            var inputRect = new Rect(row.xMax - inputWidth, row.y, inputWidth, row.height);

            // Slider (fills the middle)
            var sliderRect = new Rect(labelRect.xMax + gap, row.y,
                                      row.width - labelWidth - inputWidth - gap * 2f,
                                      row.height);

            // Run slider first — it returns the new value.
            float sliderVal = Widgets.HorizontalSlider(sliderRect, value, 0f, 1f);

            // Run text field — it mutates a local copy via ref.
            float textVal = value;
            Widgets.TextFieldNumeric(inputRect, ref textVal, ref buffer, 0f, 1f);

            // Determine who changed: slider takes priority if it moved.
            if (sliderVal != value)
            {
                // Slider was dragged.
                value = sliderVal;
                buffer = value.ToString("F2");
            }
            else if (textVal != value)
            {
                // Text field was edited.
                value = textVal;
            }

            return value != prev;
        }

        /// <summary>
        /// Generic compact slider row: [Label] [Slider] [NumericInput].
        /// </summary>
        private static void DrawSliderRow(Listing_Standard listing, string label,
            ref float value, ref string buffer,
            float min, float max,
            float labelWidth, float inputWidth, float gap, float rowHeight,
            System.Action onChanged)
        {
            var row = listing.GetRect(rowHeight);
            float prev = value;

            var labelRect = new Rect(row.x, row.y, labelWidth, row.height);
            Widgets.Label(labelRect, label);

            var inputRect = new Rect(row.xMax - inputWidth, row.y, inputWidth, row.height);
            var sliderRect = new Rect(labelRect.xMax + gap, row.y,
                                      row.width - labelWidth - inputWidth - gap * 2f,
                                      row.height);

            float sliderVal = Widgets.HorizontalSlider(sliderRect, value, min, max);

            float textVal = value;
            Widgets.TextFieldNumeric(inputRect, ref textVal, ref buffer, min, max);

            if (sliderVal != value)
            {
                value = sliderVal;
                buffer = value.ToString("F2");
            }
            else if (textVal != value)
            {
                value = textVal;
            }

            if (value != prev)
                onChanged?.Invoke();
        }
    }
}
