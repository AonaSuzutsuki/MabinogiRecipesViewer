using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CookInformationViewer.Models.Converter
{
    public class ImageLoader
    {
        private static readonly Dictionary<string, BitmapImage> Cache = new Dictionary<string, BitmapImage>();

        public static BitmapImage? LoadFromResource(string key)
        {
            if (Cache.ContainsKey(key))
                return Cache[key];

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(key);
            if (stream == null)
                return null;

            var bitmapImage = CreateBitmapImage(stream, 80, 80);

            Cache.Add(key, bitmapImage);

            return bitmapImage;
        }

        public static BitmapImage CreateBitmapImage(Stream stream, int width = 50, int height = 50)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.DecodePixelWidth = width;
            bitmapImage.DecodePixelHeight = height;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static byte[]? CreateByteArray(BitmapImage? image)
        {
            if (image == null)
                return null;

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            var data = ms.ToArray();

            return data;
        }

        public static BitmapImage? GetNoImage()
        {
            return LoadFromResource("CookInformationViewer.Resources.no-image.png");
        }

        public static bool EqualsNoImage(BitmapImage image)
        {
            var noImage = GetNoImage();
            return image == noImage;
        }
    }
}
