using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Friskzips.patch;
using static Friskzips.PluginInfo;
using BepInEx.Configuration;
using UnityEngine.UIElements;
using System;

namespace Friskzips;

[BepInPlugin(PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; set; }

    public static ConfigEntry<configHudPosition> position { get; private set; }
    public static ConfigEntry<bool> oldTexture { get; private set; }

    public static ConfigEntry<bool> alignToShipRadar { get; private set; }

    public enum configHudPosition
    {
        Bottom,
        Top
    }


    public static ManualLogSource Log => Instance.Logger;

    private readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);


    public Plugin()
    {
        Instance = this;
    }

    private void Awake()
    {
        

        Log.LogInfo($"Applying patches...");
        ApplyPluginPatch();
        Log.LogInfo($"Patches applied!");

        Log.LogInfo($"Loading assets...");
        ACompass.loadAssets();

        //Config
        Log.LogInfo($"Loading config...");

        position = Config.Bind(
            new ConfigDefinition("Hud", "Position"),
            configHudPosition.Bottom,
            new ConfigDescription("The compass position it can be Bottom or Top.\nDefault: Bottom")
            );

        oldTexture = Config.Bind(
           new ConfigDefinition("Hud", "Old texture"),
           false,
           new ConfigDescription("If you want the old texture of the original mod.\nDefault: False")
           );

        alignToShipRadar = Config.Bind(
           new ConfigDefinition("Hud", "Align to the ship monitor"),
           true,
           new ConfigDescription("If you want the compass to align to the ship monitor.\nDefault: True")
           );

        Log.LogInfo($"Config loaded!");
        //End config

        Log.LogDebug($"Looking for lethal config...");
        if (LethalConfigCompatibility.enabled)
        {
            Log.LogInfo($"LethalConfig found!");
            LethalConfigCompatibility.initLethalConfig();

        }

        Log.LogInfo($"ACompass revived loaded!");
        Log.LogInfo($"Original mod by alekso56");
    }

    /// <summary>
    /// Applies the patch to the game.
    /// </summary>
    private void ApplyPluginPatch()
    {
        _harmony.PatchAll(typeof(ACompass));
    }
}
