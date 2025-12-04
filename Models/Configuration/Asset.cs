using Newtonsoft.Json;
using System;

namespace Tanuki.Atlyss.FontAssetsManager.Models.Configuration;

[Serializable]
internal struct Asset
{
#pragma warning disable CS0649
    [JsonProperty("Bundle")]
    public string Bundle;

    [JsonProperty("Object")]
    public string Object;
#pragma warning restore CS0649
}