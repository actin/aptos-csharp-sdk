using Newtonsoft.Json;

namespace Aptos.Rest.Model.Resources {
    [JsonObject]
    public interface IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }
}