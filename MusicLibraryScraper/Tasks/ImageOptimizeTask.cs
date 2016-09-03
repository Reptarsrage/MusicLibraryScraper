namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;

    class ImageOptimizeWithoutFileTask : IDisposable
    {
        public ConcurrentImageTask Task;
        private MemoryStream stream;
        private Image image;
        private object lockMe = new object();

        public Image OptimizeImage(Image image)
        {
            var _imageMan = new ImageManager();
            using (var resizedImage = _imageMan.ScaleImage(image, 600))
            {
                long @out = 0;
                //var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize);
                this.stream = new MemoryStream();
                var outImage = _imageMan.ConvertImagetoQuality(resizedImage, 90, out @out, ref stream);
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

    class ImageOptimizeTask : Task<FileInfo>
    {
        public static FileInfo OptimizeImage(FileInfo filename, DirectoryInfo dir) {
            var _imageMan = new ImageManager();
            using (var image = _imageMan.loadImage(filename))
            using (var resizedImage = _imageMan.ScaleImage(image, 600))
            {
                //var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize);
                var outFile = _imageMan.SaveImageWithQuality(resizedImage, dir.FullName, Path.GetFileNameWithoutExtension(filename.Name), 90);

                return outFile;
            }
        }

        public ImageOptimizeTask(FileInfo filename, DirectoryInfo dir) : base(() => OptimizeImage(filename, dir))
        {
        }
    }
}
