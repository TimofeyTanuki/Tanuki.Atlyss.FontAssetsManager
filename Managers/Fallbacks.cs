using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class Fallbacks
{
    private const string DirectoryName = "Fallbacks";
    public static Fallbacks Instance;

    private string Directory;
    public readonly List<Fallback> Assets;

    private Fallbacks() =>
        Assets = [];

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
            Directory = Path.Combine(Paths.ConfigPath, Main.Instance.Name, DirectoryName)
        };
    }

    public void Reload()
    {
        if (!System.IO.Directory.Exists(Directory))
            System.IO.Directory.CreateDirectory(Directory);

        List<Models.Configuration.Fallback> Configuration;

        Rule Rule;
        TMP_FontAsset TMP_FontAsset;
        Fallback CurrentFallback;
        HashSet<TMP_FontAsset> Fallbacks = [];
        foreach (string File in System.IO.Directory.GetFiles(Directory, "*.json"))
        {
            Configuration = JsonConvert.DeserializeObject<List<Models.Configuration.Fallback>>(System.IO.File.ReadAllText(File));

            foreach (Models.Configuration.Fallback Fallback in Configuration)
            {
                try
                {
                    Rule = new(new Regex(Fallback.Rule.Object), new Regex(Fallback.Rule.Font));
                }
                catch
                {
                    Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("Fallbacks.InvalidRegex", File));
                    continue;
                }

                Fallbacks.Clear();
                foreach (Models.Configuration.Asset Asset in Fallback.Assets)
                {
                    TMP_FontAsset = AssetBundles.Instance.GetAssetObject<TMP_FontAsset>(Asset.Bundle, Asset.Object);
                    if (TMP_FontAsset is null)
                    {
                        Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("Fallbacks.AssetNotFound", Asset.Object, Asset.Bundle, File));
                        continue;
                    }

                    Fallbacks.Add(TMP_FontAsset);
                }

                if (Fallbacks.Count == 0)
                    continue;

                CurrentFallback = null;
                foreach (Fallback OtherFallback in Assets)
                {
                    Main.Instance.ManualLogSource.LogInfo($"isnull? {Rule.Equals(OtherFallback.Rule)}");
                    Main.Instance.ManualLogSource.LogInfo($"eq? {Rule.Equals(OtherFallback.Rule)}");
                    if (!Rule.Equals(OtherFallback.Rule))
                        continue;

                    CurrentFallback = OtherFallback;
                    break;
                }

                if (CurrentFallback is null)
                {
                    CurrentFallback = new(Rule, Fallback.Fixed);
                    Assets.Add(CurrentFallback);
                }
                else
                {
                    if (CurrentFallback.Fixed || Fallback.Fixed)
                    {
                        foreach (TMP_FontAsset Asset in CurrentFallback.Assets)
                            AssetBundles.Instance.Unuse(Asset, true);

                        CurrentFallback.Assets.Clear();
                        Main.Instance.ManualLogSource.LogInfo($"Unused {CurrentFallback.Assets.Count}");
                    }

                    CurrentFallback.Fixed = Fallback.Fixed;
                }

                foreach (TMP_FontAsset Asset in Fallbacks)
                {
                    CurrentFallback.Assets.Add(Asset);
                    AssetBundles.Instance.Use(Asset);
                }
            }
        }
    }
    public void Reset() => Assets.Clear();
    public void Handle(TMP_Text TMP_Text)
    {
        foreach (Fallback Fallback in Assets)
        {
            if (!Fallback.Rule.IsMatch(TMP_Text.name, TMP_Text.font.name))
                continue;

            TMP_Text.font.fallbackFontAssetTable ??= [];

            foreach (TMP_FontAsset TMP_FontAsset in Fallback.Assets)
            {
                if (TMP_Text.font == TMP_FontAsset)
                    continue;

                if (TMP_Text.font.fallbackFontAssetTable.Contains(TMP_FontAsset))
                    continue;

                TMP_Text.font.fallbackFontAssetTable.Add(TMP_FontAsset);
            }

            break;
        }
    }
}