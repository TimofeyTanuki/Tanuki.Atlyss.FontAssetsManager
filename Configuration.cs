using BepInEx.Configuration;

namespace Tanuki.Atlyss.FontAssetsManager;

internal class Configuration
{
    public static Configuration Instance;

    public Models.Configuration.Debug Debug;

    private Configuration() { }
    public static void Initialize() => Instance ??= new();

    public void Load(ConfigFile ConfigFile)
    {
        Debug = new(ConfigFile);
    }
}