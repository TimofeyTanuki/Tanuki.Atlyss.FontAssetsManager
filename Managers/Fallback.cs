using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class Fallback
{
    private const string DirectoryName = "Fallbacks";
    public static Fallback Instance;

    private string ReplacementConfigurationsPath;
    public readonly List<Models.Fallback> Assets;

    private Fallback() =>
        Assets = [];

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
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
        HashSet<TMP_FontAsset> Fallbacks = [];
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

                Fallbacks.Clear();
                foreach (Models.Configuration.Asset Asset in Fallback.Fallbacks)
                {
                    TMP_FontAsset = AssetBundles.Instance.GetFontAssetTMP(Asset.AssetBundle, Asset.Name);
                    if (TMP_FontAsset is null)
                    {
                        Main.Instance.ManualLogSource.LogWarning("fallback not found =(");
                        continue;
                    }

                    Fallbacks.Add(TMP_FontAsset);
                }

                if (Fallbacks.Count == 0)
                    continue;

                CurrentFallback = null;
                foreach (Models.Fallback OtherFallback in Assets)
                {
                    if (!OtherFallback.Rule.Equals(Fallback.Rule))
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
                    CurrentFallback.Fixed = Fallback.Fixed;

                    if (CurrentFallback.Fixed)
                        CurrentFallback.Assets.Clear();
                }
                Main.Instance.ManualLogSource.LogDebug("D6");
                foreach (TMP_FontAsset Asset in Fallbacks)
                    CurrentFallback.Assets.Add(Asset);
            }
        }

        Main.Instance.ManualLogSource.LogDebug($"Fallback rules (x{this.Assets.Count})");
    }
    public void Handle(TMP_Text TMP_Text)
    {
        foreach (Models.Fallback Fallback in Assets)
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

    public void Unload() => Assets.Clear();
}