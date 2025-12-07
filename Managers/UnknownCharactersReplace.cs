using System;
using System.Text;
using TMPro;

namespace Tanuki.Atlyss.FontAssetsManager.Managers;

public class UnknownCharactersReplace
{
    public static UnknownCharactersReplace Instance;

    private UnknownCharactersReplace() { }

    public static void Initialize()
    {
        if (Instance is not null)
            return;

        Instance = new();
    }
    private void TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke(TMP_Text __instance, ref string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (!__instance.font)
            return;

        StringBuilder StringBuilder = new();

        foreach (char Character in value)
        {
            if (__instance.font.HasCharacter(Character, true))
            {
                StringBuilder.Append(Character);
                continue;
            }

            StringBuilder.Append($"\\u{Convert.ToInt32(Character)}");
        }

        value = StringBuilder.ToString();
    }
    public void Load()
    {
        if (!Configuration.Instance.General.ReplaceUnknownCharactersWithCodes.Value)
            return;

        Patches.TMPro.TMP_Text.Text_Setter_Prefix.OnInvoke += TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke;
    }
    public void Unload()
    {
        Patches.TMPro.TMP_Text.Text_Setter_Prefix.OnInvoke -= TMPro_TMP_Text_Text_Setter_Prefix_OnInvoke;
    }
    public void Reload()
    {
        Unload();
        Load();
    }
}