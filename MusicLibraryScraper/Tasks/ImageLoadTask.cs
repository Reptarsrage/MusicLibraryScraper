namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Drawing;
    using System.IO;

    class ImageLoadTask : ConcurrentImageTask
    {
        public static Image LoadImageFromFile(FileInfo image)
        {
            Logger.WriteLine($"loading image {image}");
            try
            {
                Image ret = Image.FromFile(image.FullName); // can fail if corrupt image or invalid pixel format, will fail with OOM Exception
                Logger.IncrementLoadedImageCount();
                return ret;
            }
            catch (OutOfMemoryException e)
            {
                Logger.WriteError($"Image {image} is invalid. Unable to load.");
                return null;
            }
        }

        public ImageLoadTask(FileInfo image) : base(() => LoadImageFromFile(image))
        {
        }        
    }
}
