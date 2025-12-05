using HarmonyLib;

namespace Tanuki.Atlyss.FontAssetsManager.Patches.TMPro.TMP_Text;

[HarmonyPatch(typeof(global::TMPro.TMP_Text), nameof(global::TMPro.TMP_Text.text), MethodType.Setter)]
public static class Text_Setter_Prefix
{
    public delegate void EventHandler(global::TMPro.TMP_Text __instance, ref string value);
    public static event EventHandler OnInvoke;

#pragma warning disable IDE0051
    private static void Prefix(global::TMPro.TMP_Text __instance, ref string value)
    {
        if (__instance.text == value)
            return;

        OnInvoke?.Invoke(__instance, ref value);
    }
#pragma warning restore IDE0051
}