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
    using Managers;
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Tasks;

    /// <summary>
    /// Queries for images, downloads images, optimizes them, and finally tags files with album art.
    /// </summary>
    class MusicFileScraper
    {
        private TaskManager _taskMan;
        private RequestThrottler RequestThrottler;
        private CacheManager CacheManager;

        /// <summary>
        /// Creates a new instance of <<see cref="MusicFileScraper"/>
        /// </summary>
        public MusicFileScraper(RequestThrottler RequestThrottler, CacheManager CacheManager)
        {
            this._taskMan = new TaskManager();
            this.RequestThrottler = RequestThrottler;
            this.CacheManager = CacheManager;
        }

        #region public methods

        /// <summary>
        /// Queries for images, downloads images, optimizes them, and finally tags the file with album art.
        /// </summary>
        public void Scrape(ScraperArguments options, FileInfo file)
        {
            FileInfo art;
            TagLibManager tagLib;
            string artist;
            string albumArtist;
            string title;
            string album;
            string filename;
            string url = null;
            string fileType = null;
            Stopwatch watch = new Stopwatch();
            AlbumArtResults results;
            Image albumArt;

            Logger.AddOrigionalFileSize(file.Length);

            watch.Start();

            tagLib = new TagLibManager();
            artist = tagLib.GetArtist(file);
            albumArtist = tagLib.GetAlbumArtist(file);
            title = tagLib.GetTitle(file);
            album = tagLib.GetAlbum(file);
            filename = Path.GetFileNameWithoutExtension(file.FullName);

            Logger.WriteLine($"Scraping file ({Path.GetFileNameWithoutExtension(file.FullName)}).");

            // Try Amazon first
            var task = CacheManager.GetAlbumImageURLFromAmazon(string.IsNullOrEmpty(albumArtist) ? artist : albumArtist, string.IsNullOrEmpty(album) ? filename : album);

            if (_taskMan.RunTask(task, $"getting album art for music file: ({Path.GetFileNameWithoutExtension(file.FullName)}) using amazon.", true, RequestThrottler) && 
                task.Result != null && task.Result.Results != null)
            {
                results = task.Result;
                Logger.IncrementTaskFound();
            }
            else
            {
                // Try Google second
                results = UseAlternateGoogleScraper(options, file);
            }

            // loop through results until we've successfully downloaded and loaded the image
            if (results != null) {
                int i = 0;
                foreach (var result in results.Results ?? new List<AlbumArtResult>())
                {
                    url = result?.Url ?? null;
                    fileType = result?.ImageType ?? ""; // leave blank and let mime type decide
                    if (DownloadOptimizeAndTag(file, url, tagLib, fileType, options, true))
                    {
                        // Success!
                        Logger.IncrementTaskCompleted();
                        break;
                    }
                    else
                    {
                        // Failure!
                        if (i == results.Results.Count)
                        {
                            Logger.WriteError($"Failed to scrape file: { Path.GetFileNameWithoutExtension(file.FullName)}.");
                        }
                        else
                        {
                            Logger.WriteWarning($"Failed to scrape file: { Path.GetFileNameWithoutExtension(file.FullName)}. Trying another image...");
                        }
                    }
                    i++;
                }
            }

            watch.Stop();
            Logger.IncrementTaskTotal();
            Logger.AddFinalFileSize(file.Length);
            Logger.AddTaskTime(watch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Searches the directory given for all files matching extensions given for music files.
        /// </summary>
        /// <param name="options">Command line parameter options. <see cref="ScraperArguments"/></param>
        /// <returns>A Stack of <see cref="FileInfo"/> objects, one for each music file found. </returns>
        public Stack<FileInfo> DirSearch(ScraperArguments options)
        {
            var watch = new Stopwatch();


            if (!Directory.Exists(options.SourceDir))
            {
                throw new DirectoryNotFoundException($"Directory '{options.SourceDir}' not found.");
            }

            watch.Start();
            var root = new DirectoryInfo(options.SourceDir);
            Queue<DirectoryInfo> dirs = new Queue<DirectoryInfo>();
            Stack<FileInfo> files = new Stack<FileInfo>();
            dirs.Enqueue(root);
            var dirCt = 0;
            var total = Directory.GetFiles(options.SourceDir, "*.*", SearchOption.AllDirectories).Length;
            int fileCt = 0;
            int fileCt2 = 0;
            Dictionary<string, long> extensions = new Dictionary<string, long>();
            var patterns = options.Extensions.Split(",".ToCharArray());

            while (dirs.Count > 0)
            {
                root = dirs.Dequeue();
                dirCt++;

                foreach (var d in Directory.GetDirectories(root.FullName))
                {
                    if (options.Recurse)
                    {
                        dirs.Enqueue(new DirectoryInfo(d));
                    }
                }


                foreach (var f in Directory.GetFiles(root.FullName))
                {
                    fileCt++;

                    if (extensions.ContainsKey(Path.GetExtension(f).ToLower()))
                    {
                        extensions[Path.GetExtension(f).ToLower()]++;
                    }
                    else
                    {
                        extensions[Path.GetExtension(f).ToLower()] = 1;
                    }

                    if (patterns.Contains(Path.GetExtension(f), StringComparer.CurrentCultureIgnoreCase))
                    {
                        fileCt2++;
                        files.Push(new FileInfo(f));
                    }
                }
                Logger.drawTextProgressBar(fileCt, total);
            }

            // Print info
            watch.Stop();
            Logger.WriteLine($"\n\n{(watch.ElapsedMilliseconds > 1000 ? (watch.ElapsedMilliseconds / 1000) + "s" : watch.ElapsedMilliseconds + "ms")} elapsed.");
            Logger.WriteLine($"({dirCt}) directories scraped.");
            Logger.WriteLine($"({fileCt}) files scraped.");
            Logger.WriteLine($"({fileCt2}) music files found.");
            Logger.WriteLine($"Music files: ");
            List<string> list = extensions.Keys.ToList();
            list.Sort((key1, key2) => -extensions[key1].CompareTo(extensions[key2]));
            foreach (var x in list)
            {
                if (patterns.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                {
                    Logger.WriteLine($"\t({extensions[x]}) files with extension '{x}'");
                }
            }

            Logger.WriteLine($"Other files: ");
            foreach (var x in list)
            {
                if (!patterns.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                {
                    Logger.WriteLine($"\t({extensions[x]}) files with extension '{x}'");
                }
            }

            return files;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Downloads images, optimizes them, and finally tags files with album art
        /// </summary>
        private bool DownloadOptimizeAndTag(FileInfo file, string url, TagLibManager tagLib, string fileType, ScraperArguments options, bool WithoutFiles)
        {
            Image downloadedImage = null;
            Image albumArt = null;
            FileInfo art;
            bool result;

            if (WithoutFiles)
            {
                result = (url != null &&
                            (downloadedImage = DownloadButNotSaveImage(url)) != null &&                // Downloaded art
                            ((albumArt = OptimizeButNotSaveImage(url, downloadedImage)) != null) &&   // Optimize art
                            TagMusicFile(file, tagLib, albumArt));                                     // tag file
            }
            else
            {
                result = (url != null &&
                            (art = DownloadAndSaveImage(url, fileType, new DirectoryInfo(options.ImageOutDir), tagLib)) != null && // Downloaded art
                            ((art = OptimizeImageWithSave(art, options.OptimizedImageOutDir)) != null) &&                           // Optimize art
                            (albumArt = LoadImage(art)) != null &&                                                          // Loaded art
                            TagMusicFile(file, tagLib, albumArt));                                                          // tagged file
            }

            if (downloadedImage != null)
            {
                downloadedImage.Dispose();
            }

            if (albumArt != null)
            {
                albumArt.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Uses Google to retrieve album art URL's
        /// </summary>
        private AlbumArtResults UseAlternateGoogleScraper(ScraperArguments options, FileInfo file)
        {
            Logger.WriteLine($"Using Google as alternate image source for file: {Path.GetFileNameWithoutExtension(file.FullName)}");

            var query = CreateQuery(file);

            var task = CacheManager.GetAlbumImageURLUsingGoogle(query);
            
            var description = $"getting album art for music file: ({Path.GetFileNameWithoutExtension(file.FullName)}) using google.";
            if (_taskMan.RunTask(task, description, true, RequestThrottler))
            {
                if (task.Result != null && task.Result.Results != null)
                {
                    Logger.IncrementTaskFound();
                    return task.Result;
                }
                else
                {
                    Logger.WriteError($"Error: {description}.\nNo Results found for query '{query}'.");
                }
            }
            
            return null;
        }

        /// <summary>
        /// Tags file with artwork
        /// </summary>
        private bool TagMusicFile(FileInfo music, TagLibManager tagLib, Image image)
        {
            try
            {
                if (tagLib.TagFileWithCoverArtwork(music, image))
                {
                    Logger.WriteSuccess($"Successfully added artwork to file {music.FullName}");
                    return true;
                }
                else
                {
                    Logger.WriteError($"Unknown error adding artwork to file {music.FullName}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.WriteError($"Error tagging file {music.Name} {e.Message}\n {e.StackTrace}");
                return false;
            }
            finally
            {
                image.Dispose();
            }
        }

        /// <summary>
        /// Optimizes album artwork image to file
        /// </summary>
        private FileInfo OptimizeImageWithSave(FileInfo art, string @out)
        {
            if (!File.Exists(art.FullName))
            {
                Logger.WriteError($"Error optimizing image, album art {art.FullName} not found.");
                return null;
            }

            var dir = art.Directory;
            if (@out != null && Directory.Exists(Path.GetFullPath(@out)))
            {
                dir = new DirectoryInfo(Path.GetFullPath(@out));
            }

            var task = CacheManager.GetOptimizedImage(art, dir);
            if (_taskMan.RunTask(task, $"optimizing image: {art.FullName}.", false, RequestThrottler))
            {
                Logger.WriteLine($"Optimized image {art.FullName}.");
                return ((OptimizeImageTask)task).Result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads downloadedd image from file
        /// </summary>
        private Image LoadImage(FileInfo art)
        {
            if (!File.Exists(art.FullName))
            {
                Logger.WriteError($"Error loading image, album art {art.FullName} not found.");
                return null;
            }

            var task = CacheManager.GetLoadedImage(art);
            if (_taskMan.RunTask(task, $"loading image: {art.FullName}.", false, RequestThrottler))
            {
                Logger.WriteLine($"Loaded image {art.FullName}.");
                return task.Result; // Cast is crucial as the result is cloned here but only for the ImageLoadTask subclass.
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Downlaods album art to in-memory stream
        /// </summary>
        private Image DownloadButNotSaveImage(string url)
        {
            var task = CacheManager.GetAlbumImage(url);

            if (_taskMan.RunTask(task, $"downoading image: {url}.", false, RequestThrottler))
            {
                return task.Result;
            }

            return null;
        }
        /// <summary>
        /// Optimizes album art to in-memory stream
        /// </summary>
        private Image OptimizeButNotSaveImage(string url, Image image)
        {
            var task = CacheManager.GetOptimizedImage(url, image);

            if (_taskMan.RunTask(task, $"optimizing image: {url}.", false, RequestThrottler))
            {
                return task.Result;
            }

            return null;
        }
        /// <summary>
        /// Downlaods album art to on-disk file
        /// </summary>
        private FileInfo DownloadAndSaveImage(string url, string filetype, DirectoryInfo outDir, TagLibManager tagLib)
        {
            var task = CacheManager.DownloadAlbumImageFile(url, filetype, outDir);

            if (_taskMan.RunTask(task, $"downoading image: {url}.", false, RequestThrottler))
            {
                return task.Result;
            }
            
            return null;
        }

        /// <summary>
        /// Helper method to construct a Google Search query
        /// </summary>
        private string CreateQuery(FileInfo file)
        {
            var tagLib = new TagLibManager();
            var artist = tagLib.GetArtist(file);
            var albumArtist = tagLib.GetAlbumArtist(file);
            var title = tagLib.GetTitle(file);
            var album = tagLib.GetAlbum(file);

            var query = "album art";
            if (!string.IsNullOrWhiteSpace(albumArtist))
            {
                query = $"{album} by {albumArtist} album art";
            }
            else if (!string.IsNullOrWhiteSpace(artist))
            {
                query = $"{album} by {artist} album art";

            }
            else if (!string.IsNullOrWhiteSpace(album))
            {
                query = $"{album} album art";
            }
            else if (!string.IsNullOrWhiteSpace(title))
            {
                query = $"{title} album art";
            }

            Regex r = new Regex(@"\s+");

            return r.Replace(query, " "); // remove unnecessary spaces
        }
        #endregion
    }
}
