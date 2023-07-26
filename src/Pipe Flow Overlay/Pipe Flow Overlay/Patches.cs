using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

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

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnDestroy")]
        public class Game_OnDestroy_Patch
        {
            public static void Prefix()
            {
                PipeFlowOverlayMod.Instance.SaveSettings();
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Update")]
        public class Game_Update_Patch
        {
            public static void Postfix()
            {
                PipeFlowOverlayMod.Instance.Update();
            }
        }

        [HarmonyPatch(typeof(SimDebugView))]
        [HarmonyPatch("SetMode")]
        public class SimDebugView_SetMode_Patch
        {
            public static void Postfix(HashedString mode)
            {
                PipeFlowOverlayMod.Instance.ToggleOverlay(mode);
            }
        }

        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnSpawn")]
        public class Conduit_OnSpawn_Patch
        {
            public static void Postfix(Conduit __instance)
            {
                if (__instance.ConduitType == ConduitType.Liquid)
                {
                    PipeFlowOverlayMod.Instance.AddOrUpdateLiquidConduitFlowDirection(__instance.GameObject, ConduitFlow.FlowDirections.None);
                }
                else if (__instance.ConduitType == ConduitType.Gas)
                {
                    PipeFlowOverlayMod.Instance.AddOrUpdateGasConduitFlowDirection(__instance.GameObject, ConduitFlow.FlowDirections.None);
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduit))]
        [HarmonyPatch("OnSpawn")]
        public class SolidConduit_OnSpawn_Patch
        {
            public static void Postfix(SolidConduit __instance)
            {
                PipeFlowOverlayMod.Instance.AddOrUpdateSolidConduitFlowDirection(__instance.gameObject, SolidConduitFlow.FlowDirection.None);
            }
        }

        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnCleanUp")]
        public class Conduit_OnCleanUp_Patch
        {
            public static void Postfix(Conduit __instance)
            {
                if (__instance.ConduitType == ConduitType.Liquid)
                {
                    PipeFlowOverlayMod.Instance.RemoveLiquidConduitFlow(__instance.GameObject);
                }
                else if (__instance.ConduitType == ConduitType.Gas)
                {
                    PipeFlowOverlayMod.Instance.RemoveGasConduitFlow(__instance.GameObject);
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduit))]
        [HarmonyPatch("OnCleanUp")]
        public class SolidConduit_OnCleanUp_Patch
        {
            public static void Postfix(SolidConduit __instance)
            {
                PipeFlowOverlayMod.Instance.RemoveSolidConduitFlow(__instance.gameObject);

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
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("Clear")]
        public class SolidConduitFlow_SOAInfo_Clear_Patch
        {
            public static void Postfix()
            {
                PipeFlowOverlayMod.Instance.ClearSolidConduitFlowDirections();
            }
        }

        [HarmonyPatch(typeof(ConduitFlow.SOAInfo))]
        [HarmonyPatch("AddPermittedFlowDirections")]
        public class ConduitFlow_SOAInfo_AddPermittedFlowDirections_Patch
        {
            public static void Postfix(ConduitFlow.SOAInfo __instance, int idx, ConduitFlow.FlowDirections delta)
            {
                ConduitFlow manager = PipeFlowOverlayMod.Instance.GetConduitFlow(__instance);
                if (manager == null)
                {
                    return;
                }

                GameObject conduitGO = __instance.GetConduitGO(idx);

                if (manager.conduitType == ConduitType.Liquid)
                {
                    PipeFlowOverlayMod.Instance.AddOrUpdateLiquidConduitFlowDirection(conduitGO, delta);
                }
                else if (manager.conduitType == ConduitType.Gas)
                {
                    PipeFlowOverlayMod.Instance.AddOrUpdateGasConduitFlowDirection(conduitGO, delta);
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduitFlow.SOAInfo))]
        [HarmonyPatch("SetTargetFlowDirection")]
        public class SolidConduitFlow_SOAInfo_SetTargetFlowDirection_Patch
        {
            public static void Postfix(SolidConduitFlow.SOAInfo __instance, int idx, SolidConduitFlow.FlowDirection directions)
            {
                PipeFlowOverlayMod.Instance.AddOrUpdateSolidConduitFlowDirection(__instance.GetConduitGO(idx), directions);
            }
        }

        [HarmonyPatch(typeof(OverlayLegend))]
        [HarmonyPatch("SetLegend")]
        [HarmonyPatch(new System.Type[] { typeof(OverlayLegend.OverlayInfo) })]
        public class OverlayLegend_SetLegend_Patch
        {
            public static void Prefix(OverlayLegend.OverlayInfo overlayInfo)
            {
                
                PipeFlowOverlayMod.Instance.CreateCheckBox(overlayInfo);
            }
        }

        [HarmonyPatch(typeof(KToggle))]
        [HarmonyPatch("OnValueChanged")]
        public class KToggle_OnValueChanged_Patch
        {
            public static void Postfix(KToggle __instance, bool value)
            {
                PipeFlowOverlayMod.Instance.ToggleValueChanged(__instance, value);
            }
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("Initialize")]
        public class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                Translate(typeof(STRINGS));
            }

            public static void Translate(Type root)
            {
                Localization.RegisterForTranslation(root);

                LoadStrings();

                LocString.CreateLocStringKeys(root, null);

                Localization.GenerateStringsTemplate(root, GetTranslationDir());
            }

            private static string GetTranslationDir()
            {
                string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Translations");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }

            private static void LoadStrings()
            {
                string code = Localization.GetLocale()?.Code;
                if (string.IsNullOrEmpty(code))
                    code = Localization.GetCurrentLanguageCode();

                string path = Path.Combine(GetTranslationDir(), code + ".po");

                if (File.Exists(path))
                    Localization.OverloadStrings(Localization.LoadStringsFile(path, false));
                else
                    Debug.Log($"{code}.po not found, using default strings.");
            }
        }
    }
}
