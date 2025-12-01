using HarmonyLib;

namespace Tanuki.Atlyss.FontAssetsManager.Patches.UnityEngine.UI.Text;

[HarmonyPriority(int.MaxValue)]
[HarmonyPatch(typeof(global::UnityEngine.UI.Text), "OnEnable", MethodType.Normal)]
public static class OnEnable_Prefix
{
    public delegate void EventHandler(global::UnityEngine.UI.Text Instance);
    public static event EventHandler OnInvoke;

#pragma warning disable IDE0051
    private static void Postfix(global::UnityEngine.UI.Text __instance) => OnInvoke?.Invoke(__instance);
#pragma warning restore IDE0051
}