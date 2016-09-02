namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using System.IO;
    using System.Threading.Tasks;

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
