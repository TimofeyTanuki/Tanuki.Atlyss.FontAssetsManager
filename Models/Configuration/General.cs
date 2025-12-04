using BepInEx.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

internal class General(ConfigFile ConfigFile)
{
    private const string Section = "General";
    public ConfigEntry<bool> ReplaceUnknownCharactersWithCodes = ConfigFile.Bind(Section, "ReplaceUnknownCharactersWithCodes", true);
    public ConfigEntry<bool> UnloadUnusedAssets = ConfigFile.Bind(Section, "UnloadUnusedAssets", true);
}