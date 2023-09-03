using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.UI;
using PipeFlowOverlay.Wrappers;
using System.Collections.Generic;
using UnityEngine;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayPatches : UserMod2
    {
        internal static event System.Action ConduitsRebuilt;
        internal static event System.Action OverlayModeChanged;
        internal static ConduitType OverlayMode { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            new PLocalization().Register();
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

        [HarmonyPatch(typeof(SimDebugView))]
        [HarmonyPatch("SetMode")]
        public class SimDebugView_SetMode_Patch
        {
            public static void Postfix(HashedString mode)
            {
                if (mode == OverlayModes.LiquidConduits.ID)
                    OverlayMode = ConduitType.Liquid;
                else if (mode == OverlayModes.GasConduits.ID)
                    OverlayMode = ConduitType.Gas;
                else if (mode == OverlayModes.SolidConveyor.ID)
                    OverlayMode = ConduitType.Solid;
                else
                    OverlayMode = ConduitType.None;

                OverlayModeChanged?.Invoke();
            }
        }

        [HarmonyPatch(typeof(OverlayLegend))]
        [HarmonyPatch("OnSpawn")]
        public class OverlayLegend_OnSpawn_Patch
        {
            public static void Prefix(List<OverlayLegend.OverlayInfo> ___overlayInfoList)
            {
                GameObject checkBoxPrefab = new PCheckBox("PipeFlowOverlayCheckBox")
                {
                    Text = PipeFlowOverlayStrings.UI.PIPEFLOWOVERLAY.CHECKBOXTEXT
                }.Build();
                checkBoxPrefab.AddComponent<PipeFlowOverlayCheckBoxController>();

                foreach (OverlayLegend.OverlayInfo overlayInfo in ___overlayInfoList)
                {
                    if (overlayInfo.mode == OverlayModes.LiquidConduits.ID
                        || overlayInfo.mode == OverlayModes.GasConduits.ID
                        || overlayInfo.mode == OverlayModes.SolidConveyor.ID)
                        overlayInfo.diagrams.Add(checkBoxPrefab);
                }
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
                    .SetConduit(new ConduitWrapper(__instance));
            }
        }

        [HarmonyPatch(typeof(SolidConduit))]
        [HarmonyPatch("OnSpawn")]
        public class SolidConduit_OnSpawn_Patch
        {
            public static void Postfix(SolidConduit __instance)
            {
                Util.KInstantiateUI(PipeFlowOverlayIconController.PipeFlowPrefab, GameScreenManager.Instance.worldSpaceCanvas)
                    .GetComponent<PipeFlowOverlayIconController>()
                    .SetConduit(new SolidConduitWrapper(__instance));
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("Clear")]
        public class SolidConduitFlow_SOAInfo_Clear_Patch
        {
            public static void Postfix()
            {
                SolidConduitFlowWrapper.FlowDirections.Clear();
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("SetTargetFlowDirection")]
        public class SolidConduitFlow_SOAInfo_SetTargetFlowDirection_Patch
        {
            public static void Postfix(int idx, SolidConduitFlow.FlowDirection directions)
            {
                ConduitFlow.FlowDirections flowDirections = SolidConduitFlowWrapper.GetFlowDirections(directions);
                SolidConduitFlowWrapper.FlowDirections.AddOrUpdate(idx, flowDirections, (_, current) => current | flowDirections);
            }
        }
    }
}
