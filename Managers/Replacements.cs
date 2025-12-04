using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tanuki.Atlyss.FontAssetsManager.Models;
using TMPro;
using UnityEngine;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class Replacements
{
    private const string DirectoryName = "Replacements";
    public static Replacements Instance;

    private string Directory;
    public readonly List<Replacement<Font>> Assets;
    public readonly List<Replacement<TMP_FontAsset>> AssetsTMP;

    private Replacements()
    {
        Assets = [];
        AssetsTMP = [];
    }

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new()
        {
            Directory = Path.Combine(Paths.ConfigPath, Main.Instance.Name, DirectoryName)
        };
    }

    public void Load()
    {
        if (!System.IO.Directory.Exists(Directory))
            System.IO.Directory.CreateDirectory(Directory);

        List<Models.Configuration.Replacement> Configuration;

        Rule Rule;
        bool AddReplacement;
        foreach (string File in System.IO.Directory.GetFiles(Directory, "*.json"))
        {
            Configuration = JsonConvert.DeserializeObject<List<Models.Configuration.Replacement>>(System.IO.File.ReadAllText(File));

            foreach (Models.Configuration.Replacement Replacement in Configuration)
            {
                TMP_FontAsset TMP_FontAsset = AssetBundles.Instance.GetAssetObject<TMP_FontAsset>(Replacement.Asset.Bundle, Replacement.Asset.Object);
                Font Font = AssetBundles.Instance.GetAssetObject<Font>(Replacement.Asset.Bundle, Replacement.Asset.Object);

                if (Font is null && TMP_FontAsset is null)
                {
                    Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("Replacements.AssetNotFound", Replacement.Asset.Object, Replacement.Asset.Bundle, File));
                    continue;
                }

                try
                {
                    Rule = new(new Regex(Replacement.Rule.Object), new Regex(Replacement.Rule.Font));
                }
                catch
                {
                    Main.Instance.ManualLogSource.LogWarning(Main.Instance.Translate("Replacements.InvalidRegex", File));
                    continue;
                }

                if (TMP_FontAsset is not null)
                {
                    AddReplacement = true;

                    foreach (Replacement<TMP_FontAsset> OtherReplacement in AssetsTMP)
                    {
                        if (OtherReplacement.Rule.Equals(Rule))
                            continue;

                        AddReplacement = false;
                        AssetBundles.Instance.Unuse(OtherReplacement.Asset, true);
                        OtherReplacement.Asset = TMP_FontAsset;
                        break;
                    }

                    if (AddReplacement)
                        AssetsTMP.Add(new(Rule, TMP_FontAsset));

                    AssetBundles.Instance.Use(TMP_FontAsset);
                }

                if (Font is not null)
                {
                    AddReplacement = true;

                    foreach (Replacement<Font> OtherReplacement in Assets)
                    {
                        if (!OtherReplacement.Equals(Rule))
                            continue;

                        AddReplacement = false;
                        AssetBundles.Instance.Unuse(OtherReplacement.Asset, true);
                        OtherReplacement.Asset = Font;
                        break;
                    }

                    if (AddReplacement)
                        Assets.Add(new(Rule, Font));

                    AssetBundles.Instance.Use(Font);
                }
            }
        }
    }
    public void Handle(TMP_Text TMP_Text)
    {
        foreach (Replacement<TMP_FontAsset> Replacement in AssetsTMP)
        {
            if (TMP_Text.font == Replacement.Asset)
                break;

            if (!Replacement.Rule.IsMatch(TMP_Text.name, TMP_Text.font.name))
                continue;

            TMP_Text.font = Replacement.Asset;
            break;
        }
    }
    public void Replace(UnityEngine.UI.Text Text)
    {
        foreach (Replacement<Font> Replacement in Assets)
        {
            if (Text.font == Replacement.Asset)
                break;

            if (!Replacement.Rule.IsMatch(Text.name, Text.font.name))
                continue;

            Text.font = Replacement.Asset;
            break;
        }
    }
    public void Unload()
    {
        Assets.Clear();
        AssetsTMP.Clear();
    }
}