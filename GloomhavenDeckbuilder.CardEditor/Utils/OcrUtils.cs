using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GloomhavenDeckbuilder.CardEditor.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;

namespace GloomhavenDeckbuilder.CardEditor.Utils
{
    public class OcrUtils
    {
        private static ImageManipulation? Settings { get; set; }

        /// <summary>
        /// https://github.com/tesseract-ocr/tessdata
        /// https://stackoverflow.com/questions/10947399/how-to-implement-and-do-ocr-in-a-c-sharp-project
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string DoMagic(Bitmap image, bool scaleUp = false)
        {
            if (image is null) return string.Empty;

            return SendFile(PrepareForOcr(image, scaleUp));
        }

        private static string SendFile(Bitmap image)
        {
            HttpClient httpClient = new();
            httpClient.Timeout = new TimeSpan(0, 0, 30);

            MultipartFormDataContent form = new();
            form.Add(new StringContent("K86186512588957"), "apikey");
            form.Add(new StringContent("eng"), "language");
            form.Add(new StringContent("2"), "ocrengine");
            form.Add(new StringContent("true"), "scale");
            form.Add(new StringContent("png"), "filetype");
            form.Add(new StringContent("data:image/png;base64," + ImageUtils.BitmapToBase64(image, ImageFormat.Png)), "base64Image");

            HttpResponseMessage response = httpClient.PostAsync("https://api.ocr.space/Parse/Image", form).Result;
            string strContent = response.Content.ReadAsStringAsync().Result;

            RootObject? ocrResult = JsonConvert.DeserializeObject<RootObject>(strContent);

            if (ocrResult != null && ocrResult.OCRExitCode == 1)
                return ocrResult.ParsedResults?.FirstOrDefault()?.ParsedText ?? "";
            else
                return "";
        }

        public static Bitmap PrepareForOcr(Bitmap bitmap, bool scaleUp)
        {
            Settings = ConfigurationUtils.LoadConfiguration("ImageManipulation.json", new ImageManipulation()
            {
                Blur = 0,
                Brighten = true,
                BrightenRadius = 1,
                GlobalBrightness = -210,
                GrayScale = true
            }
            );

            if (scaleUp)
                bitmap = new Bitmap(bitmap, new Size(bitmap.Width * 2, bitmap.Height * 2));

            if (Settings is null) return bitmap;

            if (Settings.GrayScale)
                bitmap = MakeGrayscale3(bitmap);

            AdjustBrightness(bitmap, Settings.GlobalBrightness);

            if (Settings.Brighten)
                Brighten(bitmap);

            bitmap = new GaussianBlur(bitmap).Process(Settings.Blur);

            return bitmap;
        }

        public static void AdjustBrightness(Bitmap image, int value)
        {
            float FinalValue = value / 255.0f;
            ColorMatrix tempMatrix = new(new float[][]{
                       new float[] {1, 0, 0, 0, 0},
                       new float[] {0, 1, 0, 0, 0},
                       new float[] {0, 0, 1, 0, 0},
                       new float[] {0, 0, 0, 1, 0},
                       new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                   });

            ImageAttributes attributes = new();
            attributes.SetColorMatrix(tempMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            using Graphics g = Graphics.FromImage(image);
            g.DrawImage(
                image,
                new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel,
                attributes
                );
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using ImageAttributes attributes = new();

                //set the color matrix attribute
                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }

        public static Bitmap Brighten(Bitmap image)
        {
            Color closeToWhite = Color.FromArgb(254, 254, 254);

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);

                    if (!AreColorsEqual(pixel, closeToWhite) && !AreColorsEqual(pixel, Color.Black))
                    {
                        image.SetPixel(x, y, Color.White);

                        if (Settings == null) continue;
                        for (int x1 = -Settings.BrightenRadius; x1 < Settings.BrightenRadius; x1++)
                            for (int y1 = -Settings.BrightenRadius; y1 < Settings.BrightenRadius; y1++)
                            {
                                int xx = x + x1;
                                int yy = y + y1;

                                if (xx < image.Width - x1 && xx > 0 && yy < image.Height - y1 && yy > 0 && AreColorsEqual(image.GetPixel(xx, yy), Color.Black))
                                    image.SetPixel(xx, yy, closeToWhite);
                            }

                    }
                }

            return image;
        }

        public static bool AreColorsEqual(Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
        }
    }

    internal class GaussianBlur
    {
        private readonly int[] _alpha;
        private readonly int[] _red;
        private readonly int[] _green;
        private readonly int[] _blue;

        private readonly int _width;
        private readonly int _height;

        private readonly ParallelOptions _pOptions = new() { MaxDegreeOfParallelism = 16 };

        public GaussianBlur(Bitmap image)
        {
            var rct = new Rectangle(0, 0, image.Width, image.Height);
            var source = new int[rct.Width * rct.Height];
            var bits = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(bits.Scan0, source, 0, source.Length);
            image.UnlockBits(bits);

            _width = image.Width;
            _height = image.Height;

            _alpha = new int[_width * _height];
            _red = new int[_width * _height];
            _green = new int[_width * _height];
            _blue = new int[_width * _height];

            Parallel.For(0, source.Length, _pOptions, i =>
            {
                _alpha[i] = (int)((source[i] & 0xff000000) >> 24);
                _red[i] = (source[i] & 0xff0000) >> 16;
                _green[i] = (source[i] & 0x00ff00) >> 8;
                _blue[i] = (source[i] & 0x0000ff);
            });
        }

        public Bitmap Process(int radial)
        {
            var newAlpha = new int[_width * _height];
            var newRed = new int[_width * _height];
            var newGreen = new int[_width * _height];
            var newBlue = new int[_width * _height];
            var dest = new int[_width * _height];

            Parallel.Invoke(
                () => GaussBlur_4(_alpha, newAlpha, radial),
                () => GaussBlur_4(_red, newRed, radial),
                () => GaussBlur_4(_green, newGreen, radial),
                () => GaussBlur_4(_blue, newBlue, radial));

            Parallel.For(0, dest.Length, _pOptions, i =>
            {
                if (newAlpha[i] > 255) newAlpha[i] = 255;
                if (newRed[i] > 255) newRed[i] = 255;
                if (newGreen[i] > 255) newGreen[i] = 255;
                if (newBlue[i] > 255) newBlue[i] = 255;

                if (newAlpha[i] < 0) newAlpha[i] = 0;
                if (newRed[i] < 0) newRed[i] = 0;
                if (newGreen[i] < 0) newGreen[i] = 0;
                if (newBlue[i] < 0) newBlue[i] = 0;

                dest[i] = (int)((uint)(newAlpha[i] << 24) | (uint)(newRed[i] << 16) | (uint)(newGreen[i] << 8) | (uint)newBlue[i]);
            });

            var image = new Bitmap(_width, _height);
            var rct = new Rectangle(0, 0, image.Width, image.Height);
            var bits2 = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(dest, 0, bits2.Scan0, dest.Length);
            image.UnlockBits(bits2);
            return image;
        }

        private void GaussBlur_4(int[] source, int[] dest, int r)
        {
            var bxs = BoxesForGauss(r, 3);
            BoxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2);
            BoxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2);
            BoxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2);
        }

        private static int[] BoxesForGauss(int sigma, int n)
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new List<int>();
            for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
            return sizes.ToArray();
        }

        private void BoxBlur_4(int[] source, int[] dest, int w, int h, int r)
        {
            for (var i = 0; i < source.Length; i++) dest[i] = source[i];
            BoxBlurH_4(dest, source, w, h, r);
            BoxBlurT_4(source, dest, w, h, r);
        }

        private void BoxBlurH_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            Parallel.For(0, h, _pOptions, i =>
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - dest[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
            });
        }

        private void BoxBlurT_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            Parallel.For(0, w, _pOptions, i =>
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (int)Math.Round(val * iar);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ti += w;
                }
            });
        }
    }
}

