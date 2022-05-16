using Newtonsoft.Json;

namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class CardCounter
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
