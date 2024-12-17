using HarmonyLib;
using Klei.AI;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using STRINGS;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace ReworkedIncubator
{
    public class ReworkedIncubatorPatches : UserMod2
    {
        private static ReworkedIncubatorSettings _settings;
        private static readonly ConcurrentDictionary<EggIncubator, LogicPorts> _incubatorLogicPortMapping = new ConcurrentDictionary<EggIncubator, LogicPorts>();
        private static readonly ConcurrentDictionary<LogicPorts, EggIncubator> _logicPortIncubatorMapping = new ConcurrentDictionary<LogicPorts, EggIncubator>();
        private static readonly ConcurrentDictionary<LogicPorts, bool> _logicPortEnabled = new ConcurrentDictionary<LogicPorts, bool>();
        private static Chore.Precondition IsAutomationEnabled;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            LocString.CreateLocStringKeys(typeof(ReworkedIncubatorStrings.UI));
            PUtil.InitLibrary();
            new PLocalization().Register();
            new POptions().RegisterOptions(this, typeof(ReworkedIncubatorSettings));
            Localization.GenerateStringsTemplate(typeof(ReworkedIncubatorSettings), Path.Combine(path, PLocalization.TRANSLATIONS_DIR));

            IsAutomationEnabled = new Chore.Precondition
            {
                id = nameof(IsAutomationEnabled),
                description = ReworkedIncubatorStrings.UI.TOOLTIPS.REWORKEDINCUBATOR.AUTOMATIONPRECONDITION,
                fn = CheckPrecondition
            };
        }

        private static ReworkedIncubatorSettings Settings => _settings ?? (_settings = POptions.ReadSettings<ReworkedIncubatorSettings>() ?? new ReworkedIncubatorSettings());

        private static bool CheckPrecondition(ref Chore.Precondition.Context context, object data)
        {
            return !(data is EggIncubator incubator)
                || !_incubatorLogicPortMapping.TryGetValue(incubator, out LogicPorts port)
                || !_logicPortEnabled.TryGetValue(port, out bool enabled)
                || enabled;
        }

        [HarmonyPatch(typeof(ModifierSet))]
        [HarmonyPatch(nameof(ModifierSet.Initialize))]
        public class ModifierSet_Initialize_Patch
        {
            public static void Postfix(ModifierSet __instance)
            {
                Effect effect = __instance.effects.Get("EggSong");
                if (effect == null)
                    return;

                effect.duration = 1f;
                AttributeModifier modifier = effect.SelfModifiers.FirstOrDefault();
                if (modifier == null)
                    return;

                modifier.SetValue(Settings.IncubationRate / 100f);
            }
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch(nameof(Localization.Initialize))]
        public class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                CREATURES.MODIFIERS.INCUBATOR_SONG.NAME = ReworkedIncubatorStrings.CREATURES.MODIFIERS.INCUBATOR_SONG.NAME;
                CREATURES.MODIFIERS.INCUBATOR_SONG.TOOLTIP = ReworkedIncubatorStrings.CREATURES.MODIFIERS.INCUBATOR_SONG.TOOLTIP;
            }
        }

        [HarmonyPatch(typeof(EggIncubatorConfig))]
        [HarmonyPatch(nameof(EggIncubatorConfig.CreateBuildingDef))]
        public class EggIncubatorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = Settings.Power;
                __result.SelfHeatKilowattsWhenActive = Settings.SelfHeat / 1000f;
                __result.ExhaustKilowattsWhenActive = Settings.ExhaustHeat / 1000f;
            }
        }

        [HarmonyPatch(typeof(EggIncubator))]
        [HarmonyPatch("OnSpawn")]
        public class EggIncubator_OnSpawn_Patch
        {
            public static void Postfix(EggIncubator __instance)
            {
                if (!Settings.FetchAutomation
                    || !__instance.TryGetComponent(out LogicPorts port))
                    return;

                _incubatorLogicPortMapping.TryAdd(__instance, port);
                _logicPortIncubatorMapping.TryAdd(port, __instance);
            }
        }

        [HarmonyPatch(typeof(EggIncubator))]
        [HarmonyPatch("OnCleanUp")]
        public class EggIncubator_OnCleanUp_Patch
        {
            public static void Postfix(EggIncubator __instance)
            {
                if (_incubatorLogicPortMapping.TryRemove(__instance, out LogicPorts port))
                {
                    _logicPortIncubatorMapping.TryRemove(port, out _);
                    _logicPortEnabled.TryRemove(port, out _);
                }
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

        [HarmonyPatch(typeof(SingleEntityReceptacle))]
        [HarmonyPatch("CreateFetchChore")]
        public class SingleEntityReceptacle_CreateFetchChore_Patch
        {
            public static void Postfix(SingleEntityReceptacle __instance)
            {
                if (Settings.FetchAutomation
                    && __instance is EggIncubator incubator
                    && incubator.GetActiveRequest is FetchChore chore)
                    chore.AddPrecondition(IsAutomationEnabled, incubator);
            }
        }

        [HarmonyPatch(typeof(LogicPorts))]
        [HarmonyPatch("OnLogicValueChanged")]
        public class LogicPorts_OnLogicValueChanged_Patch
        {
            public static void Postfix(LogicPorts __instance, int new_value)
            {
                if (!Settings.FetchAutomation)
                    return;

                _logicPortEnabled.AddOrUpdate(__instance, new_value > 0, (_, __) => new_value > 0);
                if (!_logicPortIncubatorMapping.TryGetValue(__instance, out EggIncubator incubator)
                    || incubator.GetActiveRequest == null)
                    return;

                // Force refresh the chore conditions
                Tag requestedEntityTag = incubator.requestedEntityTag;
                Tag requestedEntityAdditionalFilterTag = incubator.requestedEntityAdditionalFilterTag;
                incubator.CancelActiveRequest();
                incubator.CreateOrder(requestedEntityTag, requestedEntityAdditionalFilterTag);
            }
        }

        [HarmonyPatch(typeof(Workable))]
        [HarmonyPatch(nameof(Workable.GetSkillExperienceMultiplier))]
        public class Workable_GetSkillExperienceMultiplier_Patch
        {
            public static void Postfix(ref float __result, string ___skillExperienceSkillGroup)
            {
                if (___skillExperienceSkillGroup == Db.Get().SkillGroups.Ranching.Id)
                    __result *= Settings.XPMultiplier;
            }
        }

        [HarmonyPatch(typeof(Workable))]
        [HarmonyPatch(nameof(Workable.GetAttributeExperienceMultiplier))]
        public class Workable_GetAttributeExperienceMultiplier_Patch
        {
            public static void Postfix(ref float __result, string ___skillExperienceSkillGroup)
            {
                if (___skillExperienceSkillGroup == Db.Get().SkillGroups.Ranching.Id)
                    __result *= Settings.XPMultiplier;
            }
        }
    }
}
