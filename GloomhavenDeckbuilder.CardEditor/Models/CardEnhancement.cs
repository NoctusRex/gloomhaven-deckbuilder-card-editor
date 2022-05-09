using Newtonsoft.Json;

namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public enum AbilityLine
    {
        Main,
        Hex,
        Summon
    }

    /// <summary>
    /// https://online.flippingbook.com/view/598058/46/
    /// </summary>
    public class CardEnhancement
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("abilityLine")]
        public AbilityLine AbilityLine { get; set; } = AbilityLine.Main;

        [JsonProperty("isNumeric")]
        public bool IsNumeric { get; set; } = false;

        [JsonProperty("canTargetEnemies")]
        public bool CanTargetEnemies { get; set; } = false;

        [JsonProperty("canTargetAllies")]
        public bool CanTargetAllies { get; set; } = false;

        [JsonProperty("isMovement")]
        public bool IsMovement { get; set; } = false;
    }
}
