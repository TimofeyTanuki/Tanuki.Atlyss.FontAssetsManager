namespace Tanuki.Atlyss.FontAssetsManager.Models;

internal class Replacement<T>(Rule Rule, T Asset)
{
    public Rule Rule = Rule;
    public T Asset = Asset;
}