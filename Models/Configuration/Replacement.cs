using Newtonsoft.Json;
using System;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Replacement
{
#pragma warning disable CS0649
    [JsonProperty("Asset")]
    public Asset Asset;

    [JsonProperty("Rule")]
    public Rule Rule;
#pragma warning restore CS0649
}