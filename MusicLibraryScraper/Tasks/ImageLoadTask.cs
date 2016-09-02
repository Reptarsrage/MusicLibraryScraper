namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;


    class ImageLoadTask : Task<Image>
    {
        private object lockMe = new object();

        public static Image LoadImage(FileInfo image) {
            Logger.WriteLine($"loading image {image}");
            try
            {
                Image ret = Image.FromFile(image.FullName); // can fail if corrupt image or invalid pixel format, will fail with OOM Exception
                return ret;
            }
            catch (OutOfMemoryException e)
            {
                Logger.WriteError($"Image {image} is invalid. Unable to load.");
                return null;
            }
            
        }

        public ImageLoadTask(FileInfo image) : base(() => LoadImage(image))
        {
        }

        public new Image Result
        {
            get
            {
                lock (lockMe) {
                    if (base.Result != null)
                    {
                        return (Image)base.Result.Clone();
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
