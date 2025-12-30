using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Tanuki.Atlyss.Core.Plugins;
using Tanuki.Atlyss.FontAssetsManager.Managers;
using TMPro;
using UnityEngine.UI;

namespace Tanuki.Atlyss.FontAssetsManager;

[BepInPlugin(PluginInfo.GUID, "Tanuki.Atlyss.FontAssetsManager", PluginInfo.Version)]
[BepInDependency("9c00d52e-10b8-413f-9ee4-bfde81762442", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(NessieEasySettings.GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Main : Plugin
{
    internal static Main Instance;
    internal ManualLogSource ManualLogSource;
    private bool Reloaded = false;

    internal void Awake()
    {
        Instance = this;
        ManualLogSource = Logger;

        Harmony Harmony = new(PluginInfo.GUID);
        Harmony.PatchAll();

        Configuration.Initialize();
        Configuration.Instance.Load(Config);

        AssetBundles.Initialize();
        AssetBundles.Instance.OnAssetsRefreshFinished += AssetBundles_OnAssetsRefreshed;

        Replacements.Initialize();
        Fallbacks.Initialize();
        UnknownCharactersReplace.Initialize();

        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(NessieEasySettings.GUID))
            NessieEasySettings.Initialize();
    }

    protected override void Load()
    {
        if (Reloaded)
        {
            Config.Reload();
            Configuration.Instance.Load(Config);
        }

        if (!AssetBundles.Instance.Refreshing)
            AssetBundles.Instance.Refresh();

        if (Configuration.Instance.Debug.TMP_Text_OnEnable.Value)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
        }

        if (Configuration.Instance.Debug.Text_OnEnable.Value)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += Text_OnEnable_Log;

        Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += Text_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TextMeshPro_OnEnable_Prefix_OnInvoke;
        UnknownCharactersReplace.Instance.Load();
    }

    private void AssetBundles_OnAssetsRefreshed()
    {
        Replacements.Instance.Reload();
        Fallbacks.Instance.Reload();

        if (Configuration.Instance.General.UnloadUnusedAssets.Value)
            AssetBundles.Instance.DestroyUnusedAssets();
    }

    private void TMP_Text_OnEnable_Log(TMP_Text Instance) =>
        Logger.LogDebug(Translate("Debug.TMP_Text.OnEnable", Instance.name, Instance.font.name));

    private void Text_OnEnable_Log(Text Instance) =>
        Logger.LogDebug(Translate("Debug.Text.OnEnable", Instance.name, Instance.font.name));

    private void Text_OnEnable_Prefix_OnInvoke(Text Instance)
    {
        if (Instance.font is null)
            return;

        Replacements.Instance.Replace(Instance);
    }

    private void TextMeshProUGUI_OnEnable_Prefix_OnInvoke(TextMeshProUGUI Instance)
    {
        Replacements.Instance.Handle(Instance);
        Fallbacks.Instance.Handle(Instance);
    }

    private void TextMeshPro_OnEnable_Prefix_OnInvoke(TextMeshPro Instance)
    {
        Replacements.Instance.Handle(Instance);
        Fallbacks.Instance.Handle(Instance);
    }

    protected override void Unload()
    {
        Reloaded = true;

        if (Configuration.Instance.Debug.TMP_Text_OnEnable.Value)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
        }

        if (Configuration.Instance.Debug.Text_OnEnable.Value)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += Text_OnEnable_Log;

        Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke -= Text_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke -= TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke -= TextMeshPro_OnEnable_Prefix_OnInvoke;

        Replacements.Instance.Reset();
        Fallbacks.Instance.Reset();
        UnknownCharactersReplace.Instance.Unload();
    }
}