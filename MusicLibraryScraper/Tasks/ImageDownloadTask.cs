namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    class ImageDownloadWithoutFileTask : IDisposable
    {
        private Image image;
        private Stream stream;
        public ConcurrentImageTask Task;

        protected Image GetAlbumImageWithoutFile(string url, int tries = 0)
        {
            Logger.WriteLine($"Fetching image at {url}");
            Logger.IncrementImageCount();

            try
            {
                using (AutoResetEvent waitHandle = new AutoResetEvent(false))
                using (WebClient webClient = new WebClient())
                {


                    byte[] data = null;
                    webClient.DownloadDataCompleted += (o, e) =>
                    {
                        try
                        {
                            data = e.Result;
                            waitHandle.Set();

                        }
                        catch { /* Shoot! we timed out before we could complete. */ }

                    };

                    webClient.DownloadDataAsync(new Uri(url));

                    if (waitHandle.WaitOne(30000))
                    {
                        if (data != null)
                        {
                            Logger.WriteSuccess($"Downloaded {url}.");
                            Logger.AddImageDownloadSize(data.Length);
                            stream = new MemoryStream(data);
                            var size = stream.Length;
                            var ret = Image.FromStream(stream);
                            Logger.AddImageDownloadSize(size);
                            return ret;
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
                if (tries >= 3) { return null; } else { return GetAlbumImageWithoutFile(url, ++tries); }
            }
        }

        public void Dispose()
        {
            try
            {
                this.image.Dispose();
            }
            catch { }

            try
            {
                this.stream.Dispose();
            }
            catch { }

            try
            {
                this.Task.Dispose();
            }
            catch { }
        }

        public ImageDownloadWithoutFileTask(string url)
        {
            this.Task = new ConcurrentImageTask(() => GetAlbumImageWithoutFile(url));
        }
    }

    class ImageDownloadTask : Task<FileInfo>
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
