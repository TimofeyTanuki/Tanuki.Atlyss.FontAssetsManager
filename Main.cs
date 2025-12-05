using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Text;
using Tanuki.Atlyss.Core.Plugins;
using Tanuki.Atlyss.FontAssetsManager.Managers;
using TMPro;
using UnityEngine.UI;

namespace Tanuki.Atlyss.FontAssetsManager;

[BepInPlugin(PluginInfo.GUID, "Tanuki.Atlyss.FontAssetsManager", PluginInfo.Version)]
[BepInDependency("9c00d52e-10b8-413f-9ee4-bfde81762442", BepInDependency.DependencyFlags.HardDependency)]
internal class Main : Plugin
{
    internal static Main Instance;
    internal ManualLogSource ManualLogSource;
    private readonly Harmony Harmony = new(PluginInfo.GUID);

    internal void Awake()
    {
        Instance = this;
        ManualLogSource = Logger;

        Configuration.Initialize();
        AssetBundles.Initialize();
        Replacements.Initialize();
        Fallbacks.Initialize();
    }

    protected override void Load()
    {
        Configuration.Instance.Load(Config);

        AssetBundles.Instance.OnAssetsRefreshed += AssetBundles_OnAssetsRefreshed;
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

        if (Configuration.Instance.General.ReplaceUnknownCharactersWithCodes.Value)
            Patches.TMPro.TMP_Text.Text_Setter_Prefix.OnInvoke += TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke;

        Harmony.PatchAll();
    }

    private void TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke(TMP_Text __instance, ref string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (!__instance.font)
            return;

        StringBuilder StringBuilder = new();

        foreach (char Character in value)
        {
            if (__instance.font.HasCharacter(Character, true))
            {
                StringBuilder.Append(Character);
                continue;
            }

            StringBuilder.Append($"\\u{Convert.ToInt32(Character)}");
        }

        value = StringBuilder.ToString();
    }
    private void AssetBundles_OnAssetsRefreshed()
    {
        Replacements.Instance.Reload();
        Fallbacks.Instance.Reload();

        if (Configuration.Instance.General.UnloadUnusedAssets.Value)
            AssetBundles.Instance.UnloadUnusedAssets();
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
        if (Configuration.Instance.General.ReplaceUnknownCharactersWithCodes.Value)
            Patches.TMPro.TMP_Text.Text_Setter_Prefix.OnInvoke -= TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke;

        Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke -= Text_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke -= TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
        Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke -= TextMeshPro_OnEnable_Prefix_OnInvoke;

        AssetBundles.Instance.OnAssetsRefreshed -= AssetBundles_OnAssetsRefreshed;

        Replacements.Instance.Reset();
        Fallbacks.Instance.Reset();

        Harmony.UnpatchSelf();
    }
}