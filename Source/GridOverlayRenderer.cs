using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GridIt
{
    /// <summary>
    /// Runtime-only grid overlay renderer. This intentionally does not use a
    /// MapComponent, so Grid It does not add mod-owned objects to save files.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class GridOverlayRenderer
    {
        // Intentionally empty: its presence forces the [StaticConstructorOnStartup]
        // attribute to be honored (turns off .beforefieldinit) and silences the
        // main-thread asset-load warning. The material is still built lazily in
        // RebuildMaterial() on the render thread.
        static GridOverlayRenderer() { }

        private static Material gridMat;
        private static bool materialDirty = true;
        private static bool textureDirty = false;
        private static readonly HashSet<int> VisibleMapIds = new HashSet<int>();

        private const int MaxBatchSize = 1023;
        private static readonly Matrix4x4[] BatchBuffer = new Matrix4x4[MaxBatchSize];

        /// <summary>
        /// Force material rebuild on next frame (color/opacity changed).
        /// </summary>
        public static void MarkMaterialDirty()
        {
            materialDirty = true;
        }

        /// <summary>
        /// Force texture regeneration and material rebuild (thickness changed).
        /// </summary>
        public static void MarkTextureDirty()
        {
            textureDirty = true;
            materialDirty = true;
        }

        public static bool IsVisible(Map map)
        {
            return map != null && VisibleMapIds.Contains(map.uniqueID);
        }

        public static void SetVisible(Map map, bool visible)
        {
            if (map == null) return;

            if (visible)
                VisibleMapIds.Add(map.uniqueID);
            else
                VisibleMapIds.Remove(map.uniqueID);
        }

        public static void ToggleVisible(Map map)
        {
            SetVisible(map, !IsVisible(map));
        }

        public static void HandleHotkey()
        {
            if (!WorldRendererUtility.DrawingMap) return;
            if (Find.CurrentMap == null) return;
            if (!GridItDefOf.GridIt_ToggleGridOverlay.KeyDownEvent) return;

            ToggleVisible(Find.CurrentMap);
            Event.current.Use();
        }

        public static void DrawCurrentMap()
        {
            if (!WorldRendererUtility.DrawingMap) return;

            var map = Find.CurrentMap;
            if (map == null) return;

            if (!IsVisible(map)) return;

            if (textureDirty)
            {
                GridTex.RegenerateBorder(GridIt_Mod.Settings.BorderThickness);
                textureDirty = false;
            }

            if (gridMat == null || materialDirty)
                RebuildMaterial();

            DrawGrid();
        }

        public static void DisableAllOverlays()
        {
            VisibleMapIds.Clear();
        }

        private static void RebuildMaterial()
        {
            // Destroy old material to avoid leak from slider adjustments.
            if (gridMat != null)
                Object.Destroy(gridMat);

            Color color = GridIt_Mod.Settings.GetGridColor();
            gridMat = new Material(ShaderDatabase.MetaOverlay);
            gridMat.mainTexture = GridTex.CellBorder;
            gridMat.color = color;
            gridMat.enableInstancing = true;
            materialDirty = false;
        }

        private static void DrawGrid()
        {
            var map = Find.CurrentMap;
            if (map == null) return;

            CellRect visible = Find.CameraDriver.CurrentViewRect;
            visible = visible.ClipInsideMap(map);

            if (visible.Area <= 0) return;

            float y = AltitudeLayer.MetaOverlays.AltitudeFor();
            int batchCount = 0;

            for (int z = visible.minZ; z <= visible.maxZ; z++)
            {
                for (int x = visible.minX; x <= visible.maxX; x++)
                {
                    Vector3 pos = new Vector3(x + 0.5f, y, z + 0.5f);
                    BatchBuffer[batchCount] = Matrix4x4.TRS(
                        pos, Quaternion.identity, Vector3.one);

                    batchCount++;
                    if (batchCount == MaxBatchSize)
                    {
                        Graphics.DrawMeshInstanced(
                            MeshPool.plane10, 0, gridMat, BatchBuffer, batchCount);
                        batchCount = 0;
                    }
                }
            }

            if (batchCount > 0)
            {
                Graphics.DrawMeshInstanced(
                    MeshPool.plane10, 0, gridMat, BatchBuffer, batchCount);
            }
        }
    }
}
