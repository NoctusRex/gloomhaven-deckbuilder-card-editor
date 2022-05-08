namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class Card
    {
        public string Title { get; set; } = string.Empty;
        public string ImgName { get; set; } = string.Empty;
        public int? Counter { get; set; } = null;
        public bool Losable { get; set; } = false;
        public int? Level { get; set; } = null;
        public int? Initiative { get; set; } = null;
    }
}
