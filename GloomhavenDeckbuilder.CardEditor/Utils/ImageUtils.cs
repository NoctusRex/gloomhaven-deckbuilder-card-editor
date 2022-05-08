using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Rectangle = System.Drawing.Rectangle;

namespace GloomhavenDeckbuilder.CardEditor.Utils
{
    public static class ImageUtils
    {
        /// <summary>
        /// https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            using Bitmap bitmap = new(outStream);

            return new Bitmap(bitmap);
        }

        public static Bitmap CaptureArea(int x, int y, int width, int height, BitmapImage source)
        {
            Rectangle area = new(x, y, width, height);
            using Bitmap img = BitmapImage2Bitmap(source);

            return img.Clone(area, img.PixelFormat);
        }


        public static string BitmapToBase64(Bitmap image, System.Drawing.Imaging.ImageFormat format)
        {
            Bitmap bImage = image;
            MemoryStream ms = new();
            bImage.Save(ms, format);
            byte[] byteImage = ms.ToArray();
            return Convert.ToBase64String(byteImage);
        }

    }
}
