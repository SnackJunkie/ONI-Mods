using HarmonyLib;
using Klei.AI;
using STRINGS;
using System;
using System.IO;
using System.Reflection;

namespace Reworked_Incubator
{
    public class Patches
    {
        [HarmonyPatch(typeof(ModifierSet))]
        [HarmonyPatch("Initialize")]
        public class ModifierSet_Initialize_Patch
        {
            public static void Postfix(ModifierSet __instance)
            {
                Effect effect = __instance.effects.Get("EggSong");
                if (effect != null)
                    effect.duration = 1f;
            }
        }

        [HarmonyPatch(typeof(EggIncubator))]
        [HarmonyPatch("EggNeedsAttention")]
        public class EggIncubator_EggNeedsAttention_Patch
        {
            public static void Postfix(EggIncubator __instance, ref bool __result)
            {
                __result = false;
                IncubationMonitor.Instance smi = __instance.Occupant?.GetSMI<IncubationMonitor.Instance>();
                smi?.ApplySongBuff();
            }
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("Initialize")]
        public class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                Translate(typeof(STRINGS));
                CREATURES.MODIFIERS.INCUBATOR_SONG.NAME = STRINGS.BUFF_NAME;
                CREATURES.MODIFIERS.INCUBATOR_SONG.TOOLTIP = STRINGS.BUFF_TOOLTIP;
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
