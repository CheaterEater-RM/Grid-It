using HarmonyLib;
using Verse;

namespace GridIt
{
    [StaticConstructorOnStartup]
    public static class GridIt_Init
    {
        static GridIt_Init()
        {
            var harmony = new Harmony("com.cheatereater.gridit");
            harmony.PatchAll();
            Log.Message("[Grid It] Harmony patches applied.");
        }
    }
}
