using BepInEx;
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
    public event AssetsRefreshed OnAssetsRefreshFinished;

    public delegate void BeforeAssetsRefresh();
    public event BeforeAssetsRefresh OnBeforeAssetsRefresh;

    private string AssetBundlesPath;
    private readonly Dictionary<ulong, Asset> Assets;
    private readonly Dictionary<Object, ulong> AssetHashes;
    private ushort PendingRefreshes = 0;

    public bool Refreshing => PendingRefreshes > 0;

    private AssetBundles()
    {
        Assets = [];
        AssetHashes = [];
    }

    public static void Initialize()
    {
        if (Instance is not null)
            return;


        Instance = new()
        {
            AssetBundlesPath = Path.Combine(Paths.ConfigPath, Main.Instance.Name, AssetBundlesDirectory)
        };
    }
    internal void Refresh()
    {
        if (Assets.Count > 0)
            return;

        if (!Directory.Exists(Instance.AssetBundlesPath))
            Directory.CreateDirectory(Instance.AssetBundlesPath);

        string[] Files = Directory.GetFiles(Instance.AssetBundlesPath, "*.assetbundle");
        PendingRefreshes = (ushort)Files.Length;

        OnBeforeAssetsRefresh?.Invoke();

        AssetBundleCreateRequest AssetBundleCreateRequest;
        foreach (string File in Files)
        {
            AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(File);
            AssetBundleCreateRequest.completed += OnAssetBundleLoaded;
        }
    }
    private void OnAssetBundleLoaded(AsyncOperation AsyncOperation)
    {
        AssetBundleCreateRequest AssetBundleCreateRequest = AsyncOperation as AssetBundleCreateRequest;
        AssetBundle AssetBundle = AssetBundleCreateRequest.assetBundle;

        if (!AssetBundle)
        {
            OnAssetBundleProcessFinished();
            return;
        }

        ulong Hash;

        foreach (TMP_FontAsset TMP_FontAsset in AssetBundle.LoadAllAssets<TMP_FontAsset>())
        {
            Hash = GetAssetHash(AssetBundle.name, TMP_FontAsset.name);
            if (Assets.ContainsKey(Hash))
            {
                Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("AssetBundle.Duplicate", TMP_FontAsset.name, nameof(TMP_FontAsset.name), AssetBundle.name));
                continue;
            }

            Assets.Add(Hash, new(TMP_FontAsset));
            AssetHashes.Add(TMP_FontAsset, Hash);
        }

        foreach (Font Font in AssetBundle.LoadAllAssets<Font>())
        {
            Hash = GetAssetHash(AssetBundle.name, Font.name);
            if (Assets.ContainsKey(Hash))
            {
                Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("AssetBundle.Duplicate", Font.name, nameof(Font.name), AssetBundle.name));
                continue;
            }

            Assets.Add(Hash, new(Font));
            AssetHashes.Add(Font, Hash);
        }

        AssetBundleUnloadOperation AssetBundleUnloadOperation = AssetBundle.UnloadAsync(false);
        AssetBundleUnloadOperation.completed += OnAssetBundleUnloaded;
    }
    private void OnAssetBundleUnloaded(AsyncOperation AsyncOperation) =>
        OnAssetBundleProcessFinished();
    private void OnAssetBundleProcessFinished()
    {
        PendingRefreshes--;

        if (PendingRefreshes > 0)
            return;

        OnAssetsRefreshFinished?.Invoke();
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
    public void Use(Object Object)
    {
        if (!AssetHashes.TryGetValue(Object, out ulong Hash))
            return;

        Assets[Hash].Uses++;
    }
    public void Unuse(Object Object, bool PreventUnload)
    {
        if (!AssetHashes.TryGetValue(Object, out ulong Hash))
            return;

        Asset Asset = Assets[Hash];

        if (Asset.Uses > 0)
            Asset.Uses--;

        if (Asset.Uses > 0 || PreventUnload)
            return;

        Assets.Remove(Hash);
        AssetHashes.Remove(Object);
        Object.Destroy(Object);
    }
    public void DestroyUnusedAssets()
    {
        List<ulong> Unused = [];
        foreach (KeyValuePair<ulong, Asset> Asset in Assets)
        {
            if (Asset.Value.Uses > 0)
                continue;

            Unused.Add(Asset.Key);
        }

        Object Object;
        foreach (ulong Hash in Unused)
        {
            Object = Assets[Hash].Object;
            Assets.Remove(Hash);
            AssetHashes.Remove(Object);
            UnityEngine.Object.Destroy(Object);
        }
    }
}