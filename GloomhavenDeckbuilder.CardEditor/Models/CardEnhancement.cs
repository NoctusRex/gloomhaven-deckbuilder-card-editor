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
        public int X { get; set; }
        public int Y { get; set; }

        public AbilityLine AbilityLine { get; set; } = AbilityLine.Main;
        public bool IsNumeric { get; set; } = false;
        public bool CanTargetEnemies { get; set; } = false;
        public bool CanTargetAllies { get; set; } = false;
        public bool IsMovement { get; set; } = false;
    }
}
