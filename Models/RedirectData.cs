using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP_Components.Models
{
    public class RedirectData
    {

        //Properties
        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("targetUrl")]
        public string? TargetUrl { get; set; }

        [JsonPropertyName("redirectType")]
        public int RedirectType { get; set; }

        [JsonPropertyName("useRelative")]
        public bool UseRelative { get; set; }


        //Methods
        public override string ToString() => JsonSerializer.Serialize<RedirectData>(this);

    }
}
