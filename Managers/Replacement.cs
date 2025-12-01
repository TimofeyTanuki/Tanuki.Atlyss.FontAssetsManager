using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;
using UnityEngine;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

internal class Replacement
{
    private const string DirectoryName = "Replacements";
    public static Replacement Instance;

    private string ReplacementConfigurationsPath;
    public List<Replacement<TMP_FontAsset>> ReplacementsTMP;
    public List<Replacement<Font>> Replacements;

    private Replacement() { }

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
            ReplacementsTMP = [],
            Replacements = [],
            ReplacementConfigurationsPath = Path.Combine(Paths.ConfigPath, Main.Instance.Name, DirectoryName)
        };
    }

    public void Load()
    {
        if (!Directory.Exists(ReplacementConfigurationsPath))
            Directory.CreateDirectory(ReplacementConfigurationsPath);

        List<Models.Configuration.Replacement> Configuration;

        Rule Rule;
        bool AddReplacement;
        foreach (string File in Directory.GetFiles(ReplacementConfigurationsPath, "*.json"))
        {
            Configuration = JsonConvert.DeserializeObject<List<Models.Configuration.Replacement>>(System.IO.File.ReadAllText(File));

            foreach (Models.Configuration.Replacement Replacement in Configuration)
            {
                TMP_FontAsset TMP_FontAsset = AssetBundles.Instance.GetFontAssetTMP(Replacement.Asset.AssetBundle, Replacement.Asset.Name);
                Font Font = AssetBundles.Instance.GetFontAsset(Replacement.Asset.AssetBundle, Replacement.Asset.Name);

                if (Font is null && TMP_FontAsset is null)
                {
                    Main.Instance.ManualLogSource.LogWarning($"Rule {File}\nAsset {Replacement.Asset.AssetBundle}, {Replacement.Asset.Name} not found");
                    continue;
                }

                foreach (Models.Configuration.Rule ReplacementRule in Replacement.Rules)
                {
                    try
                    {
                        Rule = new(new Regex(ReplacementRule.ObjectName), new Regex(ReplacementRule.FontName));
                    }
                    catch
                    {
                        Main.Instance.ManualLogSource.LogWarning("Invalid regex rule");
                        continue;
                    }

                    if (TMP_FontAsset is not null)
                    {
                        AddReplacement = true;

                        foreach (Replacement<TMP_FontAsset> OtherReplacement in ReplacementsTMP)
                        {
                            if (OtherReplacement.Rule.Equals(Rule))
                                continue;

                            AddReplacement = false;
                            OtherReplacement.Asset = TMP_FontAsset;
                            break;
                        }

                        if (AddReplacement)
                            ReplacementsTMP.Add(new(Rule, TMP_FontAsset));
                    }

                    if (Font is not null)
                    {
                        AddReplacement = true;

                        foreach (Replacement<Font> OtherReplacement in Replacements)
                        {
                            if (!OtherReplacement.Equals(Rule))
                                continue;

                            AddReplacement = false;
                            OtherReplacement.Asset = Font;
                            break;
                        }

                        if (AddReplacement)
                            Replacements.Add(new(Rule, Font));
                    }
                }
            }
        }

        Main.Instance.ManualLogSource.LogDebug($"Rule rules (Fonts) (x{Replacements.Count})");
        Main.Instance.ManualLogSource.LogDebug($"Rule rules (Fonts_TMP) (x{ReplacementsTMP.Count})");
    }
    public void Handle(TMP_Text TMP_Text)
    {
        foreach (Replacement<TMP_FontAsset> Replacement in ReplacementsTMP)
        {
            if (TMP_Text.font == Replacement.Asset)
                break;

            if (!Replacement.Rule.IsMatch(TMP_Text.name, TMP_Text.font.name))
                continue;

            Main.Instance.ManualLogSource.LogDebug($"RPL Obj:{TMP_Text.transform.name}|Font:{TMP_Text.font.name}");
            TMP_Text.font = Replacement.Asset;

            break;
        }
    }
    public void Replace(UnityEngine.UI.Text Text)
    {
        foreach (Replacement<Font> Replacement in Replacements)
        {
            if (Text.font == Replacement.Asset)
                break;

            if (!Replacement.Rule.IsMatch(Text.name, Text.font.name))
                continue;

            Main.Instance.ManualLogSource.LogDebug($"RPL Obj:{Text.name}|Font:{Text.font.name}");
            Text.font = Replacement.Asset;

            break;
        }
    }
}