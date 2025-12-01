using System.Collections.Generic;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager.Models;

internal class Fallback(Rule Rule, bool Fixed)
{
    public Rule Rule = Rule;
    public bool Fixed = Fixed;
    public HashSet<TMP_FontAsset> Assets = [];
}