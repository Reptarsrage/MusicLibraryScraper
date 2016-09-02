namespace MusicLibraryScraper.Managers
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    public class ImageManager
    {
        public Image ScaleImage(Image image, int size)
        {
            var width = image.Width;
            var height = image.Height;

            var ratio = width < height ? 600.00 / width: 600.00 / height;
            var newWidth = (int)(ratio * width);
            var newHeight = (int)(ratio * height);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }

            return newImage;
        }

        public Image loadImage(FileInfo path)
        {
            try
            {
                return Image.FromFile(path.FullName);
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        public Image ConvertImagetoQuality(Image image, int quality, out long newSize)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder qualityEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(qualityEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, jpgEncoder, myEncoderParameters);
                newSize = ms.Length;
                return Image.FromStream(ms);
            }
        }

        public FileInfo SaveImageWithQuality(Image image, string directory, string filename, int quality)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder qualityEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(qualityEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            var newFile = $"{Path.Combine(directory, Path.GetFileNameWithoutExtension(filename))}.jpg";
            image.Save(newFile, jpgEncoder, myEncoderParameters);
            return new FileInfo(newFile);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
