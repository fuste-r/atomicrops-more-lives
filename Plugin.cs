using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Atomicrops.Game.Data;
using Atomicrops.Game.GameState;
using Atomicrops.Game.Player;
using SharedLib;
using UnityEngine;

namespace MoreLives
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.fuster.atomicrops.morelives";
        public const string PLUGIN_NAME = "More Lives";
        public const string PLUGIN_VERSION = "1.0.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        // Configuration Entries
        public static ConfigEntry<int> StartingLives;
        public static ConfigEntry<int> BonusMaxHealth;
        public static ConfigEntry<float> ReviveHealthPercent;
        public static ConfigEntry<float> InvincibilityDuration;
        public static ConfigEntry<bool> InfiniteLives;
        public static ConfigEntry<bool> UseExtraLivesBeforeCorpseACopia;

        // UI Configuration Entries
        public static ConfigEntry<bool> ShowLivesHUD;
        public static ConfigEntry<float> HUDPositionX;
        public static ConfigEntry<float> HUDPositionY;
        public static ConfigEntry<int> HUDFontSize;

        // Hotkey Configuration Entries
        public static ConfigEntry<KeyCode> AddLifeHotkey;
        public static ConfigEntry<KeyCode> RestoreHealthHotkey;
        public static ConfigEntry<KeyCode> ToggleInfiniteHotkey;

        // Runtime State
        public static int RemainingLives = 3;
        public static string CurrentRunId = "";

        // GUI Style Cache
        private GUIStyle _hudBoxStyle;
        private GUIStyle _hudTextStyle;
        private Texture2D _boxBackgroundTexture;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // Bind Configurations
            StartingLives = Config.Bind(
                "Lives",
                "StartingLives",
                3,
                "Number of extra revive lives the player starts each run with."
            );

            BonusMaxHealth = Config.Bind(
                "Health",
                "BonusMaxHealth",
                0,
                "Additional max health (hearts) given to player at run start. 0 means vanilla default (4 hearts)."
            );

            ReviveHealthPercent = Config.Bind(
                "Revive",
                "ReviveHealthPercent",
                100f,
                "Percentage of max health restored upon reviving with an extra life (10 to 100)."
            );

            InvincibilityDuration = Config.Bind(
                "Revive",
                "InvincibilityDuration",
                3.0f,
                "Seconds of invincibility and blinking granted after reviving with an extra life."
            );

            InfiniteLives = Config.Bind(
                "Cheats",
                "InfiniteLives",
                false,
                "If true, player has infinite extra lives (cannot die)."
            );

            UseExtraLivesBeforeCorpseACopia = Config.Bind(
                "Revive",
                "UseExtraLivesBeforeCorpseACopia",
                true,
                "If true, extra lives trigger before the vanilla Corpse-a-Copia flower (preserving max HP). If false, Corpse-a-Copia triggers first."
            );

            ShowLivesHUD = Config.Bind(
                "HUD",
                "ShowLivesHUD",
                true,
                "Display the remaining extra lives counter on the in-game HUD."
            );

            HUDPositionX = Config.Bind(
                "HUD",
                "HUDPositionX",
                20f,
                "HUD X position in pixels from the top-left corner of the screen."
            );

            HUDPositionY = Config.Bind(
                "HUD",
                "HUDPositionY",
                85f,
                "HUD Y position in pixels from the top-left corner of the screen (just below health hearts)."
            );

            HUDFontSize = Config.Bind(
                "HUD",
                "HUDFontSize",
                16,
                "Font size for the lives HUD display."
            );

            AddLifeHotkey = Config.Bind(
                "Hotkeys",
                "AddLifeHotkey",
                KeyCode.F7,
                "Press during gameplay to add +1 extra life."
            );

            RestoreHealthHotkey = Config.Bind(
                "Hotkeys",
                "RestoreHealthHotkey",
                KeyCode.F8,
                "Press during gameplay to restore health to full."
            );

            ToggleInfiniteHotkey = Config.Bind(
                "Hotkeys",
                "ToggleInfiniteHotkey",
                KeyCode.F9,
                "Press during gameplay to toggle infinite lives on/off."
            );

            RemainingLives = StartingLives.Value;

            // Apply Harmony Patches
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} initialized successfully! Starting lives: {StartingLives.Value}, Bonus health: {BonusMaxHealth.Value}");
        }

        private void Update()
        {
            if (SingletonSceneScope<PlayerComp>.I == null || SingletonSceneScope<PlayerComp>.I.IsDead())
                return;

            // Hotkey: Add +1 Life
            if (AddLifeHotkey.Value != KeyCode.None && Input.GetKeyDown(AddLifeHotkey.Value))
            {
                RemainingLives++;
                PlayHeartSound();
                InGameText.FloatUpFromPlayer($"<color=#39FF14>+1 EXTRA LIFE! ({RemainingLives} total)</color>");
                Log.LogInfo($"Added extra life! Remaining: {RemainingLives}");
            }

            // Hotkey: Full Heal
            if (RestoreHealthHotkey.Value != KeyCode.None && Input.GetKeyDown(RestoreHealthHotkey.Value))
            {
                if (SingletonScriptableObject<GameData>.I != null && SingletonScriptableObject<GameData>.I.PlayerMaxHealth != null)
                {
                    int maxHp = SingletonScriptableObject<GameData>.I.PlayerMaxHealth.Get();
                    SingletonScriptableObject<GameData>.I.PlayerHealth.Set(maxHp);
                    PlayHeartSound();
                    InGameText.FloatUpFromPlayer("<color=#FF69B4>HEALTH FULLY RESTORED!</color>");
                    Log.LogInfo("Restored health to maximum.");
                }
            }

            // Hotkey: Toggle Infinite Lives
            if (ToggleInfiniteHotkey.Value != KeyCode.None && Input.GetKeyDown(ToggleInfiniteHotkey.Value))
            {
                InfiniteLives.Value = !InfiniteLives.Value;
                string status = InfiniteLives.Value ? "ENABLED" : "DISABLED";
                string color = InfiniteLives.Value ? "#FFD700" : "#AAAAAA";
                PlayHeartSound();
                InGameText.FloatUpFromPlayer($"<color={color}>INFINITE LIVES {status}</color>");
                Log.LogInfo($"Toggled InfiniteLives: {InfiniteLives.Value}");
            }
        }

        public static void OnNewGameData(GameData data)
        {
            RemainingLives = StartingLives.Value;
            CurrentRunId = data.RunId;

            if (BonusMaxHealth.Value > 0)
            {
                data.PlayerMaxHealth.Add(BonusMaxHealth.Value);
                data.PlayerHealth.Add(BonusMaxHealth.Value);
                Log.LogInfo($"Applied {BonusMaxHealth.Value} bonus max health hearts to new run.");
            }

            Log.LogInfo($"New run started (RunId: {CurrentRunId}). Extra lives reset to {RemainingLives}.");
        }

        public static void OnPlayerCompAwake(PlayerComp player)
        {
            if (SingletonScriptableObject<GameData>.I != null)
            {
                string runId = SingletonScriptableObject<GameData>.I.RunId;
                if (CurrentRunId != runId)
                {
                    CurrentRunId = runId;
                    RemainingLives = StartingLives.Value;
                    Log.LogInfo($"PlayerComp initialized for RunId {runId}. Extra lives set to {RemainingLives}.");
                }
            }
        }

        public static void RevivePlayer(PlayerComp player)
        {
            if (player == null) return;

            if (!InfiniteLives.Value)
            {
                RemainingLives--;
            }

            int livesLeft = RemainingLives;
            string lifeDisplay = InfiniteLives.Value ? "∞" : livesLeft.ToString();

            // Calculate restored health
            int maxHp = 4;
            if (SingletonScriptableObject<GameData>.I != null && SingletonScriptableObject<GameData>.I.PlayerMaxHealth != null)
            {
                maxHp = SingletonScriptableObject<GameData>.I.PlayerMaxHealth.Get();
            }

            int healAmount = Mathf.Max(1, Mathf.RoundToInt(maxHp * (Mathf.Clamp(ReviveHealthPercent.Value, 10f, 100f) / 100f)));

            if (SingletonScriptableObject<GameData>.I != null && SingletonScriptableObject<GameData>.I.PlayerHealth != null)
            {
                SingletonScriptableObject<GameData>.I.PlayerHealth.Set(healAmount);
            }

            // Invulnerability and visual blinking
            float invDur = Mathf.Max(0.5f, InvincibilityDuration.Value);
            player.StartTimedInvicibility(invDur);
            player.DoBlink(invDur);

            // Knock enemies back and destroy nearby bullets
            try
            {
                PlayerUtils.BreakBulletsAndPushEnemies();
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Exception in BreakBulletsAndPushEnemies: {ex.Message}");
            }

            // Visual notification above player
            InGameText.FloatUpFromPlayer($"<color=#39FF14>★ EXTRA LIFE USED! ({lifeDisplay} remaining) ★</color>");

            // Audio notification
            PlayHeartSound();

            Log.LogInfo($"Player revived using an extra life! Restored {healAmount}/{maxHp} HP. Remaining lives: {lifeDisplay}");
        }

        public static void PlayHeartSound()
        {
            try
            {
                if (SingletonScriptableObject<ConfigGame>.I != null &&
                    SingletonScriptableObject<ConfigGame>.I.Crops != null &&
                    SingletonScriptableObject<ConfigGame>.I.Crops.HeartBeetGainedSound != null &&
                    SingletonSceneScope<PlayerComp>.I != null)
                {
                    SingletonScriptableObject<ConfigGame>.I.Crops.HeartBeetGainedSound.PlayOneShot(SingletonSceneScope<PlayerComp>.I.Anchors.Center);
                }
            }
            catch
            {
                // Ignore audio errors gracefully
            }
        }

        private void InitGUIStyles()
        {
            if (_hudBoxStyle == null)
            {
                _boxBackgroundTexture = new Texture2D(1, 1);
                _boxBackgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.65f));
                _boxBackgroundTexture.Apply();

                _hudBoxStyle = new GUIStyle(GUI.skin.box);
                _hudBoxStyle.normal.background = _boxBackgroundTexture;
                _hudBoxStyle.padding = new RectOffset(8, 8, 4, 4);
            }

            if (_hudTextStyle == null)
            {
                _hudTextStyle = new GUIStyle(GUI.skin.label);
                _hudTextStyle.fontSize = HUDFontSize.Value;
                _hudTextStyle.fontStyle = FontStyle.Bold;
                _hudTextStyle.normal.textColor = new Color(1f, 0.25f, 0.35f, 1f); // Heart pink-red
                _hudTextStyle.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                _hudTextStyle.fontSize = HUDFontSize.Value;
            }
        }

        private void OnGUI()
        {
            if (!ShowLivesHUD.Value)
                return;

            if (SingletonSceneScope<PlayerComp>.I == null || SingletonSceneScope<PlayerComp>.I.IsDead())
                return;

            if (SingletonScriptableObject<GameData>.I == null)
                return;

            InitGUIStyles();

            string text = InfiniteLives.Value
                ? "<color=#FFD700>❤ Lives: ∞</color>"
                : (RemainingLives > 0
                    ? $"<color=#00FF88>❤ Lives: {RemainingLives}</color>"
                    : "<color=#FF4444>❤ Lives: 0</color>");

            GUIContent content = new GUIContent(text);
            Vector2 size = _hudTextStyle.CalcSize(content);
            float width = Mathf.Max(size.x + 16f, 120f);
            float height = Mathf.Max(size.y + 8f, 28f);

            Rect rect = new Rect(HUDPositionX.Value, HUDPositionY.Value, width, height);

            GUI.Box(rect, GUIContent.none, _hudBoxStyle);

            Rect textRect = new Rect(rect.x + 8f, rect.y + 4f, rect.width - 16f, rect.height - 8f);
            GUI.Label(textRect, content, _hudTextStyle);
        }

        private void OnDestroy()
        {
            if (_boxBackgroundTexture != null)
            {
                Destroy(_boxBackgroundTexture);
            }
        }
    }
}
