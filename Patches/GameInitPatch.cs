using HarmonyLib;
using Atomicrops.Game.Data;
using Atomicrops.Game.Player;

namespace MoreLives.Patches
{
    [HarmonyPatch(typeof(GameDataPresets), "NewGameData")]
    public static class NewGameDataPatch
    {
        public static void Postfix(GameData data)
        {
            if (data != null)
            {
                Plugin.OnNewGameData(data);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerComp), "AwakeSub")]
    public static class PlayerCompAwakePatch
    {
        public static void Postfix(PlayerComp __instance)
        {
            if (__instance != null)
            {
                Plugin.OnPlayerCompAwake(__instance);
            }
        }
    }
}
