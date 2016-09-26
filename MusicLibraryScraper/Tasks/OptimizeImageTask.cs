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
    using System.IO;

    /// <summary>
    /// Task which optimizes on-disk image files.
    /// </summary>
    class OptimizeImageTask : BaseTask<FileInfo>
    {
        private static FileInfo OptimizeImage(FileInfo filename, DirectoryInfo dir) {
            var _imageMan = new ImageManager();
            using (var image = _imageMan.loadImage(filename))
            using (var resizedImage = _imageMan.ScaleImage(image))
            {
                //var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize);
                var outFile = _imageMan.SaveImageWithQuality(resizedImage, dir.FullName, Path.GetFileNameWithoutExtension(filename.Name), 90);
                Logger.AddOptimizedImageSize(outFile.Length);
                return outFile;
            }
        }

        /// <summary>
        /// Creates a new <see cref="OptimizeImageTask"/>.
        /// </summary>
        public OptimizeImageTask(FileInfo filename, DirectoryInfo dir) : base(() => OptimizeImage(filename, dir))
        {
        }
    }
}
