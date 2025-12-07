using Nessie.ATLYSS.EasySettings;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

internal class NessieEasySettings
{
    public const string GUID = "EasySettings";

    internal static NessieEasySettings Instance;

    private NessieEasySettings()
    {
        Settings.OnInitialized.AddListener(NessieEasySettings_OnInitialize);
        Settings.OnApplySettings.AddListener(NessieEasySettings_OnApplySettings);
    }

    public static void Initialize() => Instance ??= new();
    private void NessieEasySettings_OnInitialize()
    {
        SettingsTab SettingsTab = Settings.ModTab;

        SettingsTab.AddHeader("- TA FontAssetsManager -");

        SettingsTab.AddToggle("Replace unknown characters with codes", Configuration.Instance.General.ReplaceUnknownCharactersWithCodes);

        SettingsTab.AddSpace();
    }
    private void NessieEasySettings_OnApplySettings()
    {
        Configuration.Instance.Load(Main.Instance.Config);

        UnknownCharactersReplace.Instance.Reload();
    }
}