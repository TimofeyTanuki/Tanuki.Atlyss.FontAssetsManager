using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Text;
using Tanuki.Atlyss.Core.Plugins;
using Tanuki.Atlyss.FontAssetsManager.Managers;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager;

[BepInPlugin(PluginInfo.GUID, "Tanuki.Atlyss.FontAssetsManager", PluginInfo.Version)]
[BepInDependency("9c00d52e-10b8-413f-9ee4-bfde81762442", BepInDependency.DependencyFlags.HardDependency)]
public class Main : Plugin
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
        Replacement.Initialize();
        Fallback.Initialize();
    }

    protected override void Load()
    {
        Configuration.Instance.Load(Config);

        if (Configuration.Instance.Debug.Log_TextMeshProUGUI_OnEnable.Value)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TMPro_TextMeshProUGUI_OnEnable_Prefix_Log;
        }

        AssetBundles.Instance.Load();
        Replacement.Instance.Load();
        Fallback.Instance.Load();

        Harmony.PatchAll();

        if (AssetBundles.Instance.Assets.Count > 0)
            Patches.UnityEngine.UI.Text.OnEnable_Prefix.OnInvoke += OnEnable_Prefix_OnInvoke;

        if (AssetBundles.Instance.AssetsTMP.Count > 0)
        {
            Patches.TMPro.TextMeshProUGUI.OnEnable_Prefix.OnInvoke += TextMeshProUGUI_OnEnable_Prefix_OnInvoke;
            Patches.TMPro.TextMeshPro.OnEnable_Prefix.OnInvoke += TextMeshPro_OnEnable_Prefix_OnInvoke;
        }

        Patches.ChatBehaviour.UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix.OnInvoke += UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix_OnInvoke;
    }

    private void TMPro_TextMeshProUGUI_OnEnable_Prefix_Log(TextMeshProUGUI Instance)
    {
        StringBuilder StringBuilder = new("TextMeshProUGUI\n");
        StringBuilder.Append("name: ");
        StringBuilder.AppendLine(Instance.name);
        StringBuilder.Append("transform.name: ");
        StringBuilder.AppendLine(Instance.transform.name);
        StringBuilder.Append("font.faceInfo.familyName: ");
        StringBuilder.AppendLine(Instance.font.faceInfo.familyName);
        StringBuilder.Append("font.faceInfo.styleName: ");
        StringBuilder.AppendLine(Instance.font.faceInfo.styleName);
        StringBuilder.Append("Instance.font.name: ");
        StringBuilder.AppendLine(Instance.font.name);
        Logger.LogDebug(StringBuilder.ToString());
    }

    private void OnEnable_Prefix_OnInvoke(UnityEngine.UI.Text Instance)
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
        Logger.LogInfo($"Message (Before): {Message}");

        StringBuilder StringBuilder = new();

        Logger.LogInfo($"ChatFont:{ChatBehaviour._current._chatAssets._chatText.font.faceInfo.familyName}");
        Logger.LogInfo($"ChatFontFallbacks:{string.Join(", ", ChatBehaviour._current._chatAssets._chatText.font.fallbackFontAssetTable.Select(x => x.faceInfo.familyName))}");

        foreach (char Character in Message)
        {
            if (ChatBehaviour._current._chatAssets._chatText.font.HasCharacter(Character, true))
            {
                //string[] prikoli1 = ["", "<b>", "<i>"];
                //string[] prikoli2 = ["", "</b>", "</i>"];
                //int prikol = Random.Range(0, prikoli1.Length);

                //StringBuilder.Append($"{prikoli1[prikol]}{Character}{prikoli2[prikol]}");
                StringBuilder.Append(Character);
                continue;
            }

            StringBuilder.Append($"\\u{Convert.ToInt32(Character)}");
        }

        Message = StringBuilder.ToString();
        Logger.LogInfo($"Message (After): {Message}");
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

        Patches.ChatBehaviour.UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix.OnInvoke -= UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix_OnInvoke;

        Harmony.UnpatchSelf();
    }
}