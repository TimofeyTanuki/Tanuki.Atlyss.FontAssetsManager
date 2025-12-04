using Newtonsoft.Json;
using System;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Rule
{
#pragma warning disable CS0649
    [JsonProperty("Object")]
    public string Object;

    [JsonProperty("Font")]
    public string Font;
#pragma warning restore CS0649
}