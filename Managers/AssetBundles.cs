using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;
using UnityEngine;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class AssetBundles
{
    private const string AssetBundlesDirectory = "AssetBundles";
    public static AssetBundles Instance;

    public delegate void AssetsRefreshed();
    public event AssetsRefreshed OnAssetsRefreshed;

    private string AssetBundlesPath;
    private readonly Dictionary<ulong, Asset> Assets;
    private readonly Dictionary<UnityEngine.Object, ulong> ObjectHashes;

    private AssetBundles()
    {
        Assets = [];
        ObjectHashes = [];
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
    public void Refresh()
    {
        if (!Directory.Exists(Instance.AssetBundlesPath))
            Directory.CreateDirectory(Instance.AssetBundlesPath);

        AssetBundle AssetBundle;
        ulong Hash;
        string TypeName;
        foreach (string File in Directory.GetFiles(Instance.AssetBundlesPath, "*.assetbundle"))
        {
            try
            {
                AssetBundle = UnityEngine.AssetBundle.LoadFromFile(File);
            }
            catch (Exception Exception)
            {
                Main.Instance.ManualLogSource.LogError(Main.Instance.Translate("AssetBundles.FailedToLoad", File, Exception));
                continue;
            }

            TypeName = typeof(TMP_FontAsset).Name;
            foreach (TMP_FontAsset TMP_FontAsset in AssetBundle.LoadAllAssets<TMP_FontAsset>())
            {
                Hash = GetAssetHash(AssetBundle.name, TMP_FontAsset.name);
                if (Assets.ContainsKey(Hash))
                {
                    Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("AssetBundles.Duplicate", TMP_FontAsset.name, TypeName, File));
                    continue;
                }

                Assets.Add(Hash, new(TMP_FontAsset));
                ObjectHashes.Add(TMP_FontAsset, Hash);
            }

            TypeName = typeof(Font).Name;
            foreach (Font Font in AssetBundle.LoadAllAssets<Font>())
            {
                Hash = GetAssetHash(AssetBundle.name, Font.name);
                if (Assets.ContainsKey(Hash))
                {
                    Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("AssetBundles.Duplicate", Font.name, TypeName, File));
                    continue;
                }

                Assets.Add(Hash, new(Font));
                ObjectHashes.Add(Font, Hash);
            }
        }

        OnAssetsRefreshed?.Invoke();
    }
    private ulong GetAssetHash(string AssetBundle, string AssetName) =>
        ((ulong)AssetBundle.GetHashCode() << 32) | (uint)AssetName.GetHashCode();
    public T GetAssetObject<T>(string AssetBundle, string AssetName) where T : class
    {
        Assets.TryGetValue(GetAssetHash(AssetBundle, AssetName), out Asset Asset);

        if (Asset is null)
            return default;

        if (Asset.Object is not T)
            return default;

        return Asset.Object as T;
    }
    public void Use(UnityEngine.Object Object)
    {
        if (!ObjectHashes.TryGetValue(Object, out ulong Hash))
            return;

        Assets[Hash].Uses++;
    }
    public void Unuse(UnityEngine.Object Object, bool PreventUnload)
    {
        if (!ObjectHashes.TryGetValue(Object, out ulong Hash))
            return;

        Asset Asset = Assets[Hash];

        if (Asset.Uses > 0)
            Asset.Uses--;

        if (Asset.Uses > 0 || PreventUnload)
            return;

        Assets.Remove(Hash);
        ObjectHashes.Remove(Object);
        UnityEngine.Object.Destroy(Object);
    }
    public void UnloadUnusedAssets()
    {
        List<ulong> Unused = [];
        foreach (KeyValuePair<ulong, Asset> Asset in Assets)
        {
            if (Asset.Value.Uses > 0)
                continue;

            Unused.Add(Asset.Key);
        }

        UnityEngine.Object Object;
        foreach (ulong Hash in Unused)
        {
            Object = Assets[Hash].Object;
            Assets.Remove(Hash);
            ObjectHashes.Remove(Object);
            UnityEngine.Object.Destroy(Object);
        }
    }
}