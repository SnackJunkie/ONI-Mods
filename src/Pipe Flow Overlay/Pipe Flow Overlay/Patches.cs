using HarmonyLib;

namespace Pipe_Flow_Overlay
{
    public class Patches
    {
        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnSpawn")]
        public class Game_OnSpawn_Patch
        {
            public static void Postfix()
            {
                PipeFlowOverlayMod.Instance.Initialize();
            }
        }

        [HarmonyPatch(typeof(SimDebugView))]
        [HarmonyPatch("SetMode")]
        public class OverlayMenu_ActivateOverlay_Patch
        {
            public static void Postfix(HashedString mode)
            {
                PipeFlowOverlayMod.Instance.OverlayMode = mode;

                if (mode == OverlayModes.LiquidConduits.ID)
                {
                    PipeFlowOverlayMod.Instance.ShowLiquidConduitOverlay();
                }
                else if (mode == OverlayModes.GasConduits.ID)
                {
                    PipeFlowOverlayMod.Instance.ShowGasConduitOverlay();
                }
                else if (mode == OverlayModes.SolidConveyor.ID)
                {
                    PipeFlowOverlayMod.Instance.ShowSolidConduitOverlay();
                }
                else
                {
                    PipeFlowOverlayMod.Instance.ClearFlowOverlay();
                }
            }
        }

        [HarmonyPatch(typeof(ConduitFlow.SOAInfo))]
        [HarmonyPatch("Clear")]
        public class ConduitFlow_SOAInfo_Clear_Patch
        {
            public static void Postfix(ConduitFlow.SOAInfo __instance, ConduitFlow manager)
            {
                PipeFlowOverlayMod.Instance.RegisterConduitFlowManager(__instance, manager);

                if (manager.conduitType == ConduitType.Liquid)
                {
                    PipeFlowOverlayMod.Instance.ClearLiquidConduitFlowDirections();
                }
                else if (manager.conduitType == ConduitType.Gas)
                {
                    PipeFlowOverlayMod.Instance.ClearGasConduitFlowDirections();
                }
                else if (manager.conduitType == ConduitType.Solid)
                {
                    PipeFlowOverlayMod.Instance.ClearSolidConduitFlowDirections();
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("Clear")]
        public class SolidConduitFlow_SOAInfo_Clear_Patch
        {
            public static void Postfix(SolidConduitFlow.SOAInfo __instance, SolidConduitFlow manager)
            {
                PipeFlowOverlayMod.Instance.ClearSolidConduitFlowDirections();
            }
        }

        [HarmonyPatch(typeof(ConduitFlow.SOAInfo))]
        [HarmonyPatch("AddPermittedFlowDirections")]
        public class ConduitFlow_SOAInfo_AddPermittedFlowDirections_Patch
        {
            public static void Prefix(ConduitFlow.SOAInfo __instance, int idx, ConduitFlow.FlowDirections delta)
            {
                ConduitFlow manager = PipeFlowOverlayMod.Instance.GetConduitFlow(__instance);
                if (manager == null)
                {
                    return;
                }

                if (manager.conduitType == ConduitType.Liquid)
                {
                    PipeFlowOverlayMod.Instance.AddLiquidConduitFlowDirection(idx, delta);
                }
                else if (manager.conduitType == ConduitType.Gas)
                {
                    PipeFlowOverlayMod.Instance.AddGasConduitFlowDirection(idx, delta);
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("SetTargetFlowDirection")]
        public class SolidConduitFlow_SOAInfo_SetTargetFlowDirection_Patch
        {
            public static void Prefix(int idx, SolidConduitFlow.FlowDirection directions)
            {
                PipeFlowOverlayMod.Instance.SetSolidConduitFlowDirection(idx, directions);
            }
        }
    }
}
