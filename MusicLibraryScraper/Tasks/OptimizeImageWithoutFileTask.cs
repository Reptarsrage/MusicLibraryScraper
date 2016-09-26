/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using System;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Optimizes an in-memory image
    /// </summary>
    class OptimizeImageWithoutFileTask : IDisposable
    {
        public ConcurrentImageTask Task { get; }
        private MemoryStream stream;

        private Image OptimizeImage(Image image)
        {

            var _imageMan = new ImageManager();
            using (var resizedImage = _imageMan.ScaleImage(image))
            {
                long newSize = 0;
                //var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize);
                this.stream = new MemoryStream();
                var outImage = _imageMan.ConvertImagetoQuality(resizedImage, out newSize, ref stream);
                Logger.AddOptimizedImageSize(newSize);
                return outImage;
            }
        }

        /// <summary>
        /// Disposes of all resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Task.Result.Dispose();
            }
            catch { }

            try
            {
                this.stream.Dispose();
            }
            catch { }

            try
            {
                this.Task.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// Creates a new <see cref="OptimizeImageWithoutFileTask"/>.
        /// </summary>
        public OptimizeImageWithoutFileTask(Image image)
        {
            this.Task = new ConcurrentImageTask(() => OptimizeImage(image));
        }
    }
}
