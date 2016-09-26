/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Exposes the TagLib library and it's utilities.
    /// </summary>
    class TagLibManager
    {
        /// <summary>
        /// Gets artist from music file
        /// </summary>
        public string GetArtist(FileInfo musicFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);
            return file.Tag?.FirstPerformer ?? null;
        }
        /// <summary>
        /// Gets album artist from music file
        /// </summary>
        public string GetAlbumArtist(FileInfo musicFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);
            return file.Tag?.FirstAlbumArtist ?? null;
        }
        /// <summary>
        /// Gets album from music file
        /// </summary>
        public string GetAlbum(FileInfo musicFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);
            return file.Tag?.Album ?? null;
        }
        /// <summary>
        /// Gets title from music file
        /// </summary>
        public string GetTitle(FileInfo musicFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);
            return file.Tag?.Title ?? null;
        }
        /// <summary>
        /// Gets artworks from music file
        /// </summary>
        public List<Image> GetTaggedArtwork(FileInfo musicFile)
        {
            try
            {
                var file = TagLib.File.Create(musicFile.FullName);
                var images = new List<Image>();
                foreach (var imageData in file.Tag.Pictures)
                {
                    var bin = (byte[])(imageData.Data.Data);
                    var image = Image.FromStream(new MemoryStream(bin));//.GetThumbnailImage(100, 100, null, IntPtr.Zero);
                    images.Add(image);
                }

                return images;
            } catch {
                return null;
            }
        }
        /// <summary>
        /// Removes artworks from music file
        /// </summary>
        public bool RemoveTaggedArtwork(FileInfo musicFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);

            file.Tag.Pictures = new TagLib.IPicture[] { };
            file.Save();

            if ((GetTaggedArtwork(musicFile)?.Count ??  1) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds atwork to music file. returns true on success.
        /// </summary>
        public bool TagFileWithCoverArtwork(FileInfo musicFile, Image imageFile)
        {
            var file = TagLib.File.Create(musicFile.FullName);

            using (var ms = new MemoryStream())
            {
                imageFile.Save(ms, imageFile.RawFormat);
                TagLib.Picture pic = new TagLib.Picture();
                pic.Type = TagLib.PictureType.FrontCover;
                pic.Description = "Cover";
                pic.MimeType = GetMimeType(imageFile);
                ms.Position = 0;
                pic.Data = TagLib.ByteVector.FromStream(ms);
                file.Tag.Pictures = new TagLib.IPicture[] { pic };
                file.Save();
            }

            if (GetTaggedArtwork(musicFile).Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Helper method for getting image mime type.
        /// </summary>
        private string GetMimeType(Image i)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == i.RawFormat.Guid)
                    return codec.MimeType;
            }

            return "image/unknown";
        }
    }
}
