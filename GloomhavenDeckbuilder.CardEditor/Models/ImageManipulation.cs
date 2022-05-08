namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class ImageManipulation
    {
        public bool GrayScale { get; set; }
        public int GlobalBrightness { get; set; }
        public bool Brighten { get; set; }
        public int BrightenRadius { get; set; }
        public int Blur { get; set; }
    }
}
