namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    class ImageDownloadTask : BaseTask<FileInfo>
    {
        public static FileInfo GetAlbumImage(string url, DirectoryInfo dir, string ext = "png", int tries = 0) {
            Logger.WriteLine($"Fetching image at {url}");
            Logger.IncrementImageCount();

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

        public ImageDownloadTask(string url, DirectoryInfo dir, string filetype = "png") : base(() => GetAlbumImage(url, dir, filetype))
        {
        }
    }
}
