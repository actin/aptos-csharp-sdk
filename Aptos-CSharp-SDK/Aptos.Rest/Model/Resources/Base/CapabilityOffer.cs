using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aptos.Rest.Model.Resources
{
    [JsonObject]
    public class CapabilityOffer
    {
        [JsonProperty("for", Required = Required.Always)]
        public For For { get; set; }
    }

    [JsonObject]
    public class For
    {
        [JsonProperty("vec", Required = Required.Always)]
        public List<string> Vec { get; set; }
    }
}