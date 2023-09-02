using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayPatches : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnSpawn")]
        public class Game_OnSpawn_Patch
        {
            public static void Postfix()
            {
                PipeFlowOverlayIconController.Initialize();
            }
        }

        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnSpawn")]
        public class Conduit_OnSpawn_Patch
        {
            public static void Postfix(Conduit __instance)
            {
                Util.KInstantiateUI(PipeFlowOverlayIconController.PipeFlowPrefab, GameScreenManager.Instance.worldSpaceCanvas)
                    .GetComponent<PipeFlowOverlayIconController>()
                    .SetConduit(__instance);
            }
        }
    }
}
