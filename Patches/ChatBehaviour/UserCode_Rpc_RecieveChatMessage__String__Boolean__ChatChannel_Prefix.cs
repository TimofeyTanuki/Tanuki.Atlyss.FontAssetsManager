using HarmonyLib;

namespace Tanuki.Atlyss.FontAssetsManager.Patches.ChatBehaviour;

[HarmonyPatch(typeof(global::ChatBehaviour), "UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel", MethodType.Normal)]
public static class UserCode_Rpc_RecieveChatMessage__String__Boolean__ChatChannel_Prefix
{
    public delegate void EventHandler(ref string Message);
    public static event EventHandler OnInvoke;

#pragma warning disable IDE0051
    private static void Prefix(ref string message) => OnInvoke?.Invoke(ref message);
#pragma warning restore IDE0051
}