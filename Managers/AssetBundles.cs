using BepInEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using TMPro;
using UnityEngine;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class AssetBundles
{
    private const string AssetBundlesDirectory = "AssetBundles";
    public static AssetBundles Instance;

    private string AssetBundlesPath;
    public readonly Dictionary<ulong, TMP_FontAsset> AssetsTMP;
    public readonly Dictionary<ulong, Font> Assets;

    private AssetBundles()
    {
        AssetsTMP = [];
        Assets = [];
    }

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
            AssetBundlesPath = Path.Combine(Paths.PluginPath, Main.Instance.Name, AssetBundlesDirectory)
        };
    }
    public void Load()
    {
        if (!Directory.Exists(Instance.AssetBundlesPath))
            Directory.CreateDirectory(Instance.AssetBundlesPath);

        Main.Instance.ManualLogSource.LogInfo($"assets path: {Instance.AssetBundlesPath}");
        AssetBundle AssetBundle;

        ulong Hash;
        foreach (string AssetBundlePath in Directory.GetFiles(Instance.AssetBundlesPath, "*.assetbundle"))
        {
            try
            {
                AssetBundle = UnityEngine.AssetBundle.LoadFromFile(AssetBundlePath);
            }
            catch (Exception Exception)
            {
                Main.Instance.ManualLogSource.LogError($"Failed to load:\n{Exception}");
                continue;
            }

            foreach (TMP_FontAsset TMP_FontAsset in AssetBundle.LoadAllAssets<TMP_FontAsset>())
            {
                Hash = GetAssetHash(AssetBundle.name, TMP_FontAsset.name);
                Main.Instance.ManualLogSource.LogDebug($"Found Font (TMP): \"{TMP_FontAsset.name}\" hash: {Hash}");

                if (AssetsTMP.ContainsKey(Hash))
                {
                    Main.Instance.ManualLogSource.LogWarning($"Duplicate AssetsTMP: {TMP_FontAsset.name}");
                    continue;
                }

                AssetsTMP.Add(Hash, TMP_FontAsset);
                Main.Instance.ManualLogSource.LogDebug($"Added Font (TMP): asset \"{TMP_FontAsset.name}\" with font \"{TMP_FontAsset.faceInfo.familyName}\" hash: {Hash}");
            }

            foreach (Font Font in AssetBundle.LoadAllAssets<Font>())
            {
                Hash = GetAssetHash(AssetBundle.name, Font.name);
                Main.Instance.ManualLogSource.LogDebug($"Found Font: \"{Font.name}\" hash: {Hash}");

                if (Assets.ContainsKey(Hash))
                {
                    Main.Instance.ManualLogSource.LogWarning($"Duplicate Assets: {Font.name}");
                    continue;
                }

                Assets.Add(Hash, Font);
                Main.Instance.ManualLogSource.LogDebug($"Added Font: \"{Font.name}\" hash: {Hash}");
            }
        }
    }
    public void Unload()
    {
        AssetsTMP.Clear();
        Assets.Clear();
    }

    private ulong GetAssetHash(string AssetBundle, string AssetName) =>
        ((ulong)AssetBundle.GetHashCode() << 32) | (uint)AssetName.GetHashCode();

    public TMP_FontAsset GetFontAssetTMP(string AssetBundle, string AssetName)
    {
        AssetsTMP.TryGetValue(GetAssetHash(AssetBundle, AssetName), out TMP_FontAsset TMP_FontAsset);
        return TMP_FontAsset;
    }
    public Font GetFontAsset(string AssetBundle, string AssetName)
    {
        Assets.TryGetValue(GetAssetHash(AssetBundle, AssetName), out Font TMP_FontAsset);
        return TMP_FontAsset;
    }
}