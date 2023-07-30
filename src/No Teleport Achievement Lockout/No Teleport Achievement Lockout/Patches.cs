using Database;
using HarmonyLib;
using Klei.CustomSettings;

namespace No_Teleport_Achievement_Lockout
{
    public class Patches
    {
        private static bool _overrideTeleportCriteria;

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnSpawn")]
        public class Game_OnSpawn_Patch
        {
            public static void Postfix()
            {
                _overrideTeleportCriteria = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.Teleporters).id != "Enabled";
                Debug.Log($"No Teleport Achievement Lockout enabled: {_overrideTeleportCriteria}");
            }
        }

        [HarmonyPatch(typeof(TeleportDuplicant))]
        [HarmonyPatch("Success")]
        public class TeleportDuplicant_Success_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (_overrideTeleportCriteria)
                    __result = true;
            }
        }
    }
}
