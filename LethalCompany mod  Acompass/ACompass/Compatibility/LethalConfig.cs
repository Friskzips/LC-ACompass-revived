using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;

namespace Friskzips
{
    public class LethalConfigCompatibility
    {
        private static bool? _enabled;
        

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig");
                }
                return (bool)_enabled;
            }
        }


        internal static void initLethalConfig()
        {
            var positionEnum = new EnumDropDownConfigItem<Plugin.configHudPosition>(Plugin.position, new EnumDropDownOptions
            {

                RequiresRestart = false
            });

            var oldTextureCheckbox = new BoolCheckBoxConfigItem(Plugin.oldTexture, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });

            var alignToShipRadarCheckbox = new BoolCheckBoxConfigItem(Plugin.alignToShipRadar, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });

            LethalConfigManager.AddConfigItem(positionEnum);
            LethalConfigManager.AddConfigItem(oldTextureCheckbox);
            LethalConfigManager.AddConfigItem(alignToShipRadarCheckbox);
            LethalConfigManager.SetModDescription("Original mod by alekso56");
            LethalConfigManager.SkipAutoGen();
        }
        

    }
}