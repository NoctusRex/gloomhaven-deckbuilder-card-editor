﻿using Newtonsoft.Json;

namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class Card
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("imgName")]
        public string ImgName { get; set; } = string.Empty;

        [JsonProperty("counter")]
        public int? Counter { get; set; } = null;

        [JsonProperty("losable")]
        public bool Losable { get; set; } = false;

        [JsonProperty("level")]
        public int? Level { get; set; } = null;

        [JsonProperty("initiative")]
        public int? Initiative { get; set; } = null;

        [JsonProperty("permanent")]
        public bool Permanent { get; set; } = false;
    }
}
