using BepInEx.Configuration;
using Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager;

internal class Configuration
{
    public static Configuration Instance;

    public General General;
    public Debug Debug;

    private Configuration() { }
    public static void Initialize() => Instance ??= new();

    public void Load(ConfigFile ConfigFile)
    {
        General = new(ConfigFile);
        Debug = new(ConfigFile);
    }
}