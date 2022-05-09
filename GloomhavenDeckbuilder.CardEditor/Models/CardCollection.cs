using Newtonsoft.Json;
using System.Collections.Generic;

namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class CardCollection
    {
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;
        [JsonProperty("cards")]
        public List<Card> Cards { get; set; } = new();
    }
}
