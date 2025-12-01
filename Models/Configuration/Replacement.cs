using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Replacement
{
    [JsonProperty("Asset")]
    public Asset Asset;

    [JsonProperty("Rules")]
    public List<Rule> Rules;
}