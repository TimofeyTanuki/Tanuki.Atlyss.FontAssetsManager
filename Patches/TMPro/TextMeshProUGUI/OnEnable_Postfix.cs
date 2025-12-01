using HarmonyLib;

namespace Tanuki.Atlyss.FontAssetsManager.Patches.TMPro.TextMeshProUGUI;

[HarmonyPriority(int.MaxValue)]
[HarmonyPatch(typeof(global::TMPro.TextMeshProUGUI), "OnEnable", MethodType.Normal)]
public static class OnEnable_Prefix
{
    public delegate void EventHandler(global::TMPro.TextMeshProUGUI Instance);
    public static event EventHandler OnInvoke;

#pragma warning disable IDE0051
    private static void Postfix(global::TMPro.TextMeshProUGUI __instance) => OnInvoke?.Invoke(__instance);
#pragma warning restore IDE0051
}