using System;
using System.Text.RegularExpressions;

namespace Tanuki.Atlyss.FontAssetsManager.Models;

internal class Rule(Regex ObjectName, Regex FontName) : IEquatable<Rule>
{
    private readonly Regex
        ObjectName = ObjectName,
        FontName = FontName;

    public bool IsMatch(string ObjectName, string FontName) =>
        this.ObjectName.IsMatch(ObjectName) && this.FontName.IsMatch(FontName);

    public bool Equals(Rule Other) =>
        ObjectName.Equals(Other.ObjectName) && FontName.Equals(Other.FontName);
}