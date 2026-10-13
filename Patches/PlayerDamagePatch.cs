using System;
using HarmonyLib;
using Atomicrops.Core.DamageSystem;
using Atomicrops.Game.Data;
using Atomicrops.Game.Player;
using SharedLib;
using UnityEngine;

namespace MoreLives.Patches
{
    [HarmonyPatch(typeof(PlayerComp), "_dealDamage")]
    public static class PlayerDamagePatch
    {
        private static readonly object DeathToken = new object();
        private static bool _suppressedVanillaRes = false;

        public static void Prefix(PlayerComp __instance, DamageSourceType source, bool ignoreInvincible, DamageEventArg arg, bool ignoreArmor, out bool __state)
        {
            __state = false;
            _suppressedVanillaRes = false;

            if (__instance == null || __instance.IsDead())
                return;

            // Check if damage is fatal
            int effectiveHp = GameDataUtils.GetPlayerEffectiveHealth(ignoreArmor);
            if (effectiveHp > 1)
            {
                // Not fatal
                return;
            }

            // Damage would reduce effective HP to 0 or less
            bool hasLives = Plugin.InfiniteLives.Value || Plugin.RemainingLives > 0;
            if (!hasLives)
            {
                // No extra lives left; standard death occurs
                return;
            }

            // Check for vanilla ResUpgrade (Corpse-a-Copia flower)
            bool hasResUpgrade = SingletonScriptableObject<GameData>.I != null &&
                                 SingletonScriptableObject<GameData>.I.Upgrades != null &&
                                 SingletonScriptableObject<ConfigGame>.I != null &&
                                 SingletonScriptableObject<ConfigGame>.I.Player != null &&
                                 SingletonScriptableObject<ConfigGame>.I.Player.ResUpgrade != null &&
                                 SingletonScriptableObject<GameData>.I.Upgrades.GetStacks(SingletonScriptableObject<ConfigGame>.I.Player.ResUpgrade) > 0;

            if (hasResUpgrade && !Plugin.UseExtraLivesBeforeCorpseACopia.Value)
            {
                // User prefers to trigger vanilla resurrection flower first
                return;
            }

            if (hasResUpgrade && Plugin.UseExtraLivesBeforeCorpseACopia.Value)
            {
                // Temporarily remove ResUpgrade so vanilla _dealDamage doesn't trigger ResFlower sequence
                SingletonScriptableObject<GameData>.I.Upgrades.RemoveUpgrade(SingletonScriptableObject<ConfigGame>.I.Player.ResUpgrade);
                _suppressedVanillaRes = true;
            }

            // Protect player from SetToDead() and EndRunFailure() inside _dealDamage
            __instance.StartTrackableNoDeath(DeathToken);
            __state = true;
        }

        public static void Postfix(PlayerComp __instance, bool __state)
        {
            if (__instance == null)
                return;

            // Restore vanilla ResUpgrade if we temporarily removed it
            if (_suppressedVanillaRes)
            {
                if (SingletonScriptableObject<GameData>.I != null &&
                    SingletonScriptableObject<GameData>.I.Upgrades != null &&
                    SingletonScriptableObject<ConfigGame>.I != null &&
                    SingletonScriptableObject<ConfigGame>.I.Player != null &&
                    SingletonScriptableObject<ConfigGame>.I.Player.ResUpgrade != null)
                {
                    SingletonScriptableObject<GameData>.I.Upgrades.AddStacks(SingletonScriptableObject<ConfigGame>.I.Player.ResUpgrade, 1);
                }
                _suppressedVanillaRes = false;
            }

            if (!__state)
                return;

            // Remove the temporary no-death token
            __instance.StopTrackableNoDeath(DeathToken);

            // Check if player health was brought to 0
            int currentHealth = 0;
            if (SingletonScriptableObject<GameData>.I != null && SingletonScriptableObject<GameData>.I.PlayerHealth != null)
            {
                currentHealth = SingletonScriptableObject<GameData>.I.PlayerHealth.Get();
            }

            if (currentHealth <= 0)
            {
                Plugin.RevivePlayer(__instance);
            }
        }
    }
}
