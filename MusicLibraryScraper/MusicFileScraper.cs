namespace MusicLibraryScraper
{
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Tasks;

    class MusicFileScraper
    {
        public void Scrape(ScraperArguments options, FileInfo file)
        {
            Logger.AddOrigionalFileSize(file.Length);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            var tagLib = new TagLibUtilities();

            var artist = tagLib.GetArtist(file);
            var albumArtist = tagLib.GetAlbumArtist(file);
            var title = tagLib.GetTitle(file);
            var album = tagLib.GetAlbum(file);
            var filename = Path.GetFileNameWithoutExtension(file.FullName);
            string url = null;
            string fileType = null;

            using (var task = CacheManager.GetAlbumImageURL(string.IsNullOrEmpty(albumArtist) ? artist : albumArtist, string.IsNullOrEmpty(album) ? filename : album))
            {

                if (!task.IsCompleted)
                {
                    RequestThrottler.AddTask(task);
                }

                while (!task.IsCompleted) { /* Spin spin spin */ Thread.Sleep(500); }

                if (task.IsFaulted)
                {
                    Logger.WriteError($"Error getting image on amazon for file: {Path.GetFileNameWithoutExtension(file.FullName)}.\n{task.Exception.InnerExceptions[0].Message}\n{task.Exception.InnerExceptions[0].StackTrace}");
                    var result = UseAlternateGoogleScraper(options, file);
                    url = result.Url;
                    fileType = result.Type;
                }
                else if (task.IsCanceled || string.IsNullOrEmpty(task.Result))
                {
                    Logger.WriteWarning($"No image found on amazon for file: {Path.GetFileNameWithoutExtension(file.FullName)}.");
                    var result = UseAlternateGoogleScraper(options, file);
                    url = result?.Url ?? null;
                    fileType = result.Type;
                }
                else
                {
                    url = task.Result;
                    Logger.IncrementTaskFound();
                }
            }

            if (url != null)
            {
                fileType = string.IsNullOrWhiteSpace(fileType) ? "png" : fileType;
                FileInfo art;
                if ((art = DownloadImage(url, fileType, new DirectoryInfo(options.ImageOutDir), tagLib)) != null)
                {
                    TagFile(options, file, tagLib, art);
                }
            }

            watch.Stop();
            Logger.IncrementTaskTotal();
            Logger.AddTaskTime(watch.ElapsedMilliseconds);
        }

        private GoogleResult UseAlternateGoogleScraper(ScraperArguments options, FileInfo file)
        {
            Logger.WriteLine($"Using Google as alternate image source for file: {Path.GetFileNameWithoutExtension(file.FullName)}");

            var tagLib = new TagLibUtilities();
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

            query = r.Replace(query, " "); // remove unnecessary spaces

            using (var task = CacheManager.GetAlbumImageURLUsingGoogle(query))
            {

                if (!task.IsCompleted)
                {
                    RequestThrottler.AddTask(task);
                }

                while (!task.IsCompleted) { /* Spin spin spin */ Thread.Sleep(500); }

                if (task.IsFaulted)
                {
                    Logger.WriteError($"Error getting image URL from Google for file: {Path.GetFileNameWithoutExtension(file.FullName)}.\n{task.Exception.InnerExceptions[0].Message}");
                }
                else if (task.IsCanceled || task.Result == null)
                {
                    Logger.WriteError($"No image found on Google for file: {Path.GetFileNameWithoutExtension(file.FullName)}");
                }
                else
                {
                    Logger.IncrementTaskFound();
                    return task.Result;
                }
            }
            return null;
        }

        private void TagFile(ScraperArguments options, FileInfo music, TagLibUtilities tagLib, FileInfo art)
        {
            if (!File.Exists(art.FullName))
            {
                Logger.WriteError($"Error tagging file {music.Name}, album art {art.FullName} not found.");
                return;
            }
            using (var task = CacheManager.GetLoadedImage(art))
            {
                try
                {
                    if (!task.IsCompleted)
                    {
                        RequestThrottler.AddTask(task);
                    }

                    while (!task.IsCompleted) { /* Spin spin spin */ Thread.Sleep(500); }

                    if (task.IsFaulted)
                    {
                        throw new Exception($"Error loading image: {art.FullName}.\n{task.Exception.InnerExceptions[0].Message}");
                    }
                    else if (task.IsCanceled || task.Result == null)
                    {
                        throw new Exception($"Error loading image: {art.FullName}.");
                    }
                    else
                    {
                        using (var image = ((ImageLoadTask)task).Result)
                        {
                            if (tagLib.TagFileWithCoverArtwork(music, image))
                            {
                                Logger.IncrementTaskCompleted();
                                Logger.WriteSuccess($"Successfully added artwork to file {music.FullName}");
                            }
                            else
                            {
                                Logger.WriteError($"Unknown error adding artwork to file {music.FullName}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteError($"Error tagging file {music.Name} {e.Message}\n {e.StackTrace}");
                    return;
                }
                finally
                {
                    Logger.AddFinalFileSize(music.Length);
                }
            }
        }


        private FileInfo DownloadImage(string url, string filetype, DirectoryInfo outDir, TagLibUtilities tagLib)
        {
            using (var task = CacheManager.GetAlbumImageFile(url, filetype, outDir))
            {

                if (!task.IsCompleted)
                {
                    RequestThrottler.AddTask(task);
                }

                while (!task.IsCompleted || task.Status == TaskStatus.Running) { /* Spin spin spin */ Thread.Sleep(500); }

                if (task.IsFaulted)
                {
                    Logger.WriteError($"Error downoading image: {task.Exception.InnerExceptions[0].Message}");
                }
                else if (task.IsCanceled || task.Result == null)
                {
                    Logger.WriteError($"Unable to download image as {url}.");
                }
                else
                {
                    return task.Result;
                }
            }
            return null;
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
            foreach (var x in list) {
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
    }
}
