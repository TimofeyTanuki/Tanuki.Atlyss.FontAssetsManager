using BepInEx.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

internal class Debug(ConfigFile ConfigFile)
{
    private const string Section = "Debug";
    public ConfigEntry<bool> TMP_Text_OnEnable = ConfigFile.Bind(Section, "TMP_Text_OnEnable", false);
    public ConfigEntry<bool> Text_OnEnable = ConfigFile.Bind(Section, "Text_OnEnable", false);
}