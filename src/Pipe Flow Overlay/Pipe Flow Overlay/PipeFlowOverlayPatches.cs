using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayPatches : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            PipeFlowOverlayIconController.LoadSprites();
        }

        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnSpawn")]
        public class Conduit_OnSpawn_Patch
        {
            public static void Postfix(Conduit __instance)
            {
                PUIUtils.AddPinkOverlay(__instance.GameObject);
                GameObject pipeFlow = PUIElements.CreateUI(__instance.GameObject, "PipeFlow");
                pipeFlow.AddComponent<PipeFlowOverlayIconController>();
                pipeFlow.SetActive(true);
            }
        }
    }
}
