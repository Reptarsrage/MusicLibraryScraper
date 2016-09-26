/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Managers
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Loads, converts, scales and encodes images.
    /// </summary>
    public class ImageManager
    {
        public static double MinSize { get { return MIN_SIZE; } }
        private const double MIN_SIZE = 600.00d;
        public static long DefaultQuality { get { return DEF_QUALITY; } }
        private const long DEF_QUALITY = 90L;

        /// <summary>
        /// Scales the given image so that the minumum 
        /// of its height and width will be equal to the given size.
        /// </summary>
        public Image ScaleImage(Image image,  double size = MIN_SIZE)
        {
            var width = image.Width;
            var height = image.Height;

            var ratio = width < height ? size / width: size / height;
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

        /// <summary>
        /// Loads image from image file.
        /// </summary>
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

        /// <summary>
        /// Converts the image to a lossy format jpg with the given quality.
        /// </summary>
        public Image ConvertImagetoQuality(Image image, out long newSize, ref MemoryStream ms, long quality = DEF_QUALITY)
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
            EncoderParameter myEncoderParameter = new EncoderParameter(qualityEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            image.Save(ms, jpgEncoder, myEncoderParameters);
            newSize = ms.Length;
            return Image.FromStream(ms);
        }

        /// <summary>
        /// Saves the image as a lossy format jpg to a file with the given quality.
        /// </summary>
        public FileInfo SaveImageWithQuality(Image image, string directory, string filename, long quality = DEF_QUALITY)
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
            EncoderParameter myEncoderParameter = new EncoderParameter(qualityEncoder, quality);
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
