using BepInEx.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

internal class Debug(ConfigFile ConfigFile)
{
    private const string Section = "Debug";
    public ConfigEntry<bool> Log_TextMeshProUGUI_OnEnable = ConfigFile.Bind(Section, "Log_TextMeshProUGUI_OnEnable", true);
}