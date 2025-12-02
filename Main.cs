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
    private const string
        Debug_ObjectLabel = "Object: ",
        Debug_FontLabel = "Font: ";

    internal static Main Instance;
    internal ManualLogSource ManualLogSource;
    private readonly Harmony Harmony = new(PluginInfo.GUID);

    internal void Awake()
    {
        Instance = this;
        ManualLogSource = Logger;

        Configuration.Initialize();
        AssetBundles.Initialize();
        Replacement.Initialize();
        Fallback.Initialize();
    }

    protected override void Load()
    {
        Configuration.Instance.Load(Config);
        AssetBundles.Instance.Load();

        Replacement.Instance.Load();
        Fallback.Instance.Load();

        if (Configuration.Instance.Debug.Log_TMP_Text_OnEnable.Value)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TMP_Text_OnEnable_Log;
        }

        if (Configuration.Instance.Debug.Log_Text_OnEnable.Value)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += Text_OnEnable_Log;

        if (AssetBundles.Instance.Assets.Count > 0)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += OnEnable_Prefix_OnInvoke;

        if (AssetBundles.Instance.AssetsTMP.Count > 0)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TextMeshPro_OnEnable_Prefix_OnInvoke;
        }

        if (Configuration.Instance.General.ReplaceUnknownCharactersWithCodes.Value)
            Patches.ChatBehaviour.UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix.OnInvoke += UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix_OnInvoke;

        Harmony.PatchAll();
    }
    private void TMP_Text_OnEnable_Log(TMP_Text Instance)
    {
        StringBuilder StringBuilder = new("TMP_Text\n");
        StringBuilder.Append(Debug_ObjectLabel);
        StringBuilder.AppendLine(Instance.name);
        StringBuilder.Append(Debug_FontLabel);
        StringBuilder.AppendLine(Instance.font.name);
        Logger.LogDebug(StringBuilder.ToString());
    }
    private void Text_OnEnable_Log(Text Instance)
    {
        StringBuilder StringBuilder = new("Text\n");
        StringBuilder.Append(Debug_ObjectLabel);
        StringBuilder.AppendLine(Instance.name);
        StringBuilder.Append(Debug_FontLabel);
        StringBuilder.AppendLine(Instance.font.name);
        Logger.LogDebug(StringBuilder.ToString());
    }

    private void OnEnable_Prefix_OnInvoke(Text Instance)
    {
        if (Instance.font is null)
            return;

        Replacement.Instance.Replace(Instance);
    }
    private void TextMeshProUGUI_OnEnable_Prefix_OnInvoke(TextMeshProUGUI Instance)
    {
        Replacement.Instance.Handle(Instance);
        Fallback.Instance.Handle(Instance);
    }
    private void TextMeshPro_OnEnable_Prefix_OnInvoke(TextMeshPro Instance)
    {
        Replacement.Instance.Handle(Instance);
        Fallback.Instance.Handle(Instance);
    }
    private void UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix_OnInvoke(ref string Message)
    {
        StringBuilder StringBuilder = new();

        foreach (char Character in Message)
        {
            if (ChatBehaviour._current._chatAssets._chatText.font.HasCharacter(Character, true))
            {
                StringBuilder.Append(Character);
                continue;
            }

            StringBuilder.Append($"\\u{Convert.ToInt32(Character)}");
        }

        Message = StringBuilder.ToString();
    }
    protected override void Unload()
    {
        if (AssetBundles.Instance.Assets.Count > 0)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke -= OnEnable_Prefix_OnInvoke;

        if (AssetBundles.Instance.AssetsTMP.Count > 0)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke -= TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke -= TextMeshPro_OnEnable_Prefix_OnInvoke;
        }

        if (Configuration.Instance.General.ReplaceUnknownCharactersWithCodes.Value)
            Patches.ChatBehaviour.UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix.OnInvoke -= UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix_OnInvoke;

        Replacement.Instance.Unload();
        Fallback.Instance.Unload();

        Harmony.UnpatchSelf();
    }
}