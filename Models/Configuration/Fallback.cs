using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Fallback
{
    [JsonProperty("Rule")]
    public Rule Rule;

    [JsonProperty("Fixed")]
    public bool Fixed;

    [JsonProperty("Fallbacks")]
    public List<Asset> Fallbacks;
}