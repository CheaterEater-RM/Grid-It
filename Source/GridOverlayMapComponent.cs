using UnityEngine;
using Verse;

namespace GridIt
{
    /// <summary>
    /// MapComponent that draws a grid overlay using instanced mesh rendering.
    /// Each visible cell gets a border-textured quad drawn via
    /// Graphics.DrawMeshInstanced for minimal draw-call overhead.
    /// </summary>
    public class GridOverlayMapComponent : MapComponent
    {
        public bool ShowGrid;

        private static Material gridMat;
        private static bool materialDirty = true;
        private static bool textureDirty = false;

        private const int MaxBatchSize = 1023;
        private static readonly Matrix4x4[] BatchBuffer = new Matrix4x4[MaxBatchSize];

        public GridOverlayMapComponent(Map map) : base(map) { }

        /// <summary>Persist the toggle state across save/load.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShowGrid, "ShowGrid", false);
        }

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

        /// <summary>
        /// MapComponentDraw runs during the render phase and only for the
        /// currently-viewed map, so there is no cross-map drawing.
        /// </summary>
        public override void MapComponentDraw()
        {
            if (!ShowGrid) return;
            if (Find.CurrentMap != map) return;

            if (textureDirty)
            {
                GridTex.RegenerateBorder(GridIt_Mod.Settings.BorderThickness);
                textureDirty = false;
            }

            if (gridMat == null || materialDirty)
                RebuildMaterial();

            DrawGrid();
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

        private void DrawGrid()
        {
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
