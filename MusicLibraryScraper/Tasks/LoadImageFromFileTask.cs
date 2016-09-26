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
    using System;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Loads an image into memory from a file
    /// </summary>
    class LoadImageFromFileTask : ConcurrentImageTask
    {
        private static Image LoadImageFromFile(FileInfo image)
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

        /// <summary>
        /// Creates a new instance of <see cref="LoadImageFromFileTask"/>
        /// </summary>
        public LoadImageFromFileTask(FileInfo image) : base(() => LoadImageFromFile(image))
        {
        }        
    }
}
