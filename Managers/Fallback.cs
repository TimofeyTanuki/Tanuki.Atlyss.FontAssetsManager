using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

internal class Fallback
{
    private const string DirectoryName = "Fallbacks";
    public static Fallback Instance;

    private string ReplacementConfigurationsPath;
    public List<Models.Fallback> Fallbacks;

    private Fallback() { }

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
            Fallbacks = [],
            ReplacementConfigurationsPath = Path.Combine(Paths.ConfigPath, Main.Instance.Name, DirectoryName)
        };
    }

    public void Load()
    {
        if (!Directory.Exists(ReplacementConfigurationsPath))
            Directory.CreateDirectory(ReplacementConfigurationsPath);

        List<Models.Configuration.Fallback> Configuration;

        Rule Rule;
        TMP_FontAsset TMP_FontAsset;
        Models.Fallback CurrentFallback;
        HashSet<TMP_FontAsset> Assets = [];
        foreach (string File in Directory.GetFiles(ReplacementConfigurationsPath, "*.json"))
        {
            Configuration = JsonConvert.DeserializeObject<List<Models.Configuration.Fallback>>(System.IO.File.ReadAllText(File));

            foreach (Models.Configuration.Fallback Fallback in Configuration)
            {
                try
                {
                    Rule = new(new Regex(Fallback.Rule.ObjectName), new Regex(Fallback.Rule.FontName));
                }
                catch
                {
                    Main.Instance.ManualLogSource.LogWarning("Invalid regex rule");
                    continue;
                }

                CurrentFallback = null;
                foreach (Models.Fallback OtherFallback in Fallbacks)
                {
                    if (!OtherFallback.Rule.Equals(Fallback.Rule))
                        continue;

                    if (Fallback.Fixed)
                        OtherFallback.Assets.Clear();

                    CurrentFallback = OtherFallback;
                    break;
                }

                Assets.Clear();
                foreach (Models.Configuration.Asset Asset in Fallback.Fallbacks)
                {
                    TMP_FontAsset = AssetBundles.Instance.GetFontAssetTMP(Asset.AssetBundle, Asset.Name);
                    if (TMP_FontAsset is null)
                    {
                        Main.Instance.ManualLogSource.LogWarning("fallback not found =(");
                        continue;
                    }

                    Assets.Add(TMP_FontAsset);
                }

                if (Assets.Count == 0)
                    continue;

                if (CurrentFallback is not null)
                {
                    if (Fallback.Fixed)
                        CurrentFallback.Assets.Clear();
                }
                else
                {
                    CurrentFallback = new(Rule, Fallback.Fixed);
                    Fallbacks.Add(CurrentFallback);
                }

                foreach (TMP_FontAsset Asset in Assets)
                    CurrentFallback.Assets.Add(Asset);
            }
        }

    }
    public void Handle(TMP_Text TMP_Text)
    {
        foreach (Models.Fallback Fallback in Fallbacks)
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

            Main.Instance.ManualLogSource.LogDebug($"FLB Obj:{TMP_Text.name}|Font:{TMP_Text.font.name}");

            break;
        }
    }
}