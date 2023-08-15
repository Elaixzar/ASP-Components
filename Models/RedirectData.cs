using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP_Components.Models
{

    /// <summary>
    /// Represents a data structure that defines redirect information.
    /// </summary>
    public class RedirectData
    {
        /// <summary>
        /// Gets or sets the URL to which the middleware should redirect.
        /// </summary>
        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the target URL to which the redirect should occur.
        /// </summary>
        [JsonPropertyName("targetUrl")]
        public string? TargetUrl { get; set; }

        /// <summary>
        /// Gets or sets the type of redirect, Typically 301: Moved Permanently or 302: Found.
        /// </summary>
        [JsonPropertyName("redirectType")]
        public int RedirectType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the redirect URL is relative to the application's base URL.  If this is true, the middleware will swap the Target and Redirect url, saving the child path of the original target.
        /// </summary>
        [JsonPropertyName("useRelative")]
        public bool UseRelative { get; set; }

        /// <summary>
        /// Converts the current <see cref="RedirectData"/> instance to its JSON representation as a string.
        /// </summary>
        /// <returns>A JSON string representing the current object.</returns>
        public override string ToString() => JsonSerializer.Serialize<RedirectData>(this);
    }

}
