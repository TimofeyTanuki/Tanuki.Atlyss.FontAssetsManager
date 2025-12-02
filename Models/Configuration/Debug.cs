using BepInEx.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

internal class Debug(ConfigFile ConfigFile)
{
    private const string Section = "Debug";
    public ConfigEntry<bool> Log_TMP_Text_OnEnable = ConfigFile.Bind(Section, "Log_TMP_Text_OnEnable", false);
    public ConfigEntry<bool> Log_Text_OnEnable = ConfigFile.Bind(Section, "Log_Text_OnEnable", false);
}