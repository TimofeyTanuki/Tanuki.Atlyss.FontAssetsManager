using Newtonsoft.Json;
using System;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Rule
{
    [JsonProperty("ObjectName")]
    public string ObjectName;

    [JsonProperty("FontName")]
    public string FontName;
}