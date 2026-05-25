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

            bool show = GridOverlayRenderer.IsVisible(map);
            bool wasOn = show;

            row.ToggleableIcon(
                ref show,
                GridTex.GridToggle,
                "GridIt_ToggleGrid".Translate(),
                SoundDefOf.Mouseover_ButtonToggle);

            if (show != wasOn)
                GridOverlayRenderer.SetVisible(map, show);
        }
    }

    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate))]
    public static class Patch_MapInterface_DrawGridOverlay
    {
        static void Postfix()
        {
            GridOverlayRenderer.DrawCurrentMap();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Patch_Game_ClearGridOverlayState
    {
        static void Prefix()
        {
            GridOverlayRenderer.DisableAllOverlays();
        }
    }
}
