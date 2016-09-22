namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using System;
    using System.Drawing;
    using System.IO;

    class ImageOptimizeWithoutFileTask : IDisposable
    {
        public ConcurrentImageTask Task;
        private MemoryStream stream;
        private Image image;
        private object lockMe = new object();

        public Image OptimizeImage(Image image)
        {

            var _imageMan = new ImageManager();
            using (var resizedImage = _imageMan.ScaleImage(image, ImageManager.MinSize))
            {
                long newSize = 0;
                //var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize);
                this.stream = new MemoryStream();
                var outImage = _imageMan.ConvertImagetoQuality(resizedImage, 90, out newSize, ref stream);
                Logger.AddOptimizedImageSize(newSize);
                return outImage;
            }
        }

        public void Dispose()
        {
            try
            {
                this.image.Dispose();
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

        public ImageOptimizeWithoutFileTask(Image image)
        {
            this.Task = new ConcurrentImageTask(() => OptimizeImage(image));
        }

        public Image Result
        {
            get
            {
                lock (lockMe)
                {
                    if (this.Task.Result != null)
                    {
                        return (Image)this.Task.Result.Clone();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
