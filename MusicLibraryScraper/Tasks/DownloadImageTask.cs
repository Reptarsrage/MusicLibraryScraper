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
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// Downloads an image and saves to on-disk file
    /// </summary>
    class DownloadImageTask : BaseTask<FileInfo>
    {
        public static FileInfo GetAlbumImage(string url, DirectoryInfo dir, string ext = "png", int tries = 0) {
            Logger.WriteLine($"Fetching image at {url}");
            Logger.IncrementDownlaodedImageCount();

            string myUniqueFileName = Path.Combine(dir.FullName, string.Format($"{Guid.NewGuid()}.{ext}"));

            while (File.Exists(myUniqueFileName))
            {
                myUniqueFileName = Path.Combine(dir.FullName, string.Format($"{Guid.NewGuid()}.{ext}"));
            }

            try
            {
                using (AutoResetEvent waitHandle = new AutoResetEvent(false))
                using (WebClient webClient = new WebClient())
                {
                    Regex r = new Regex(@"\?.*$");
                    url = r.Replace(url, "");

                    webClient.DownloadFileCompleted += (o, e) =>
                    {
                        try { waitHandle.Set(); } catch { /* Shoot! we timed out before we could complete. */ }

                    };

                    webClient.DownloadFileAsync(new Uri(url), myUniqueFileName);

                    if (waitHandle.WaitOne(30000))
                    {
                        if (File.Exists(myUniqueFileName))
                        {
                            Logger.WriteSuccess($"Downloaded {url} to {myUniqueFileName}.");
                            var f = new FileInfo(myUniqueFileName);
                            Logger.AddImageDownloadSize(f.Length);
                            return f;
                        }
                        else
                        {
                            throw new FileNotFoundException("Could not access the downloaded image.");
                        }

                    }
                    else
                    {
                        webClient.CancelAsync();
                        webClient.Dispose();
                        throw new TimeoutException("Download timed out.");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteWarning($"Error downloading from url {url}\n{e.Message}");
                if (tries >= 3) { return null; } else { return GetAlbumImage(url, dir, ext, ++tries);  }
            }
        }

        /// <summary>
        /// creates a new <see cref="DownloadImageTask"/>
        /// </summary>
        public DownloadImageTask(string url, DirectoryInfo dir, string filetype = "png") : base(() => GetAlbumImage(url, dir, filetype))
        {
        }
    }
}
