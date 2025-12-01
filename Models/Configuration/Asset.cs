using Newtonsoft.Json;
using System;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Asset
{
    [JsonProperty("AssetBundle")]
    public string AssetBundle;

    [JsonProperty("Name")]
    public string Name;
}