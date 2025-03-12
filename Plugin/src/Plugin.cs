using System.Reflection;
using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using BepInEx.Logging;
using System.IO;
using BoiledOne.Configuration;

namespace BoiledOne {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)] 
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger = null!;
        internal static PluginConfig BoundConfig { get; private set; } = null!;
        public static AssetBundle? ModAssets;

        private void Awake() {
            Logger = base.Logger;
            BoundConfig = new PluginConfig(base.Config);
            InitializeNetworkBehaviours();
            var bundleName = "modassets";
            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), bundleName));
            if (ModAssets == null) {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }

            var BoiledOne = ModAssets.LoadAsset<EnemyType>("BoiledOne");
            var BoiledOneTN = ModAssets.LoadAsset<TerminalNode>("BoiledOneTN");
            var BoiledOneTK = ModAssets.LoadAsset<TerminalKeyword>("BoiledOneTK");

            NetworkPrefabs.RegisterNetworkPrefab(BoiledOne.enemyPrefab);
            Enemies.RegisterEnemy(BoiledOne, BoundConfig.SpawnWeight.Value, Levels.LevelTypes.All, BoiledOneTN, BoiledOneTK);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void InitializeNetworkBehaviours() {
            // See https://github.com/EvaisaDev/UnityNetcodePatcher?tab=readme-ov-file#preparing-mods-for-patching
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        } 
    }
}