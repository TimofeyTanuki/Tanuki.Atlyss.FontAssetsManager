using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Fallback
{
#pragma warning disable CS0649
    [JsonProperty("Rule")]
    public Rule Rule;

    [JsonProperty("Fixed")]
    public bool Fixed;

    [JsonProperty("Assets")]
    public List<Asset> Assets;
#pragma warning restore CS0649
}