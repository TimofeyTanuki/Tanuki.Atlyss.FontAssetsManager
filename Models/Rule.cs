using System;
using System.Text.RegularExpressions;

namespace Tanuki.Atlyss.FontAssetsManager.Models;

public class Rule(Regex ObjectName, Regex FontName) : IEquatable<Rule>
{
    private readonly Regex
        ObjectName = ObjectName,
        FontName = FontName;

    public bool IsMatch(string ObjectName, string FontName) =>
        this.ObjectName.IsMatch(ObjectName) && this.FontName.IsMatch(FontName);

    public bool Equals(Rule Other) =>
        ObjectName.ToString() == Other.ObjectName.ToString() && FontName.ToString() == Other.FontName.ToString();
}