using HarmonyLib;
using RimWorld;
using Verse;

namespace GridIt
{
    /// <summary>
    /// Adds the grid toggle button to the bottom-right play settings row,
    /// right alongside the vanilla beauty/roof/fertility overlay toggles.
    /// </summary>
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings_GridToggle
    {
        static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView || GridIt_Mod.Settings.HideToggleButton) return;

            var map = Find.CurrentMap;
            if (map == null) return;

            var comp = map.GetComponent<GridOverlayMapComponent>();
            if (comp == null) return;

            bool show = comp.ShowGrid;
            bool wasOn = show;

            row.ToggleableIcon(
                ref show,
                GridTex.GridToggle,
                "GridIt_ToggleGrid".Translate(),
                SoundDefOf.Mouseover_ButtonToggle);

            if (show != wasOn)
                comp.ShowGrid = show;
        }
    }
}
