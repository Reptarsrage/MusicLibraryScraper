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
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Downloads an image to in-memory object.
    /// </summary>
    class DownloadImageWithoutFileTask : IDisposable
    {
        private Stream stream;
        public ConcurrentImageTask Task;

        private Image GetAlbumImageWithoutFile(string url, int tries = 0)
        {
            Logger.WriteLine($"Fetching image at {url}");
            Logger.IncrementDownlaodedImageCount();

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
                            this.stream = new MemoryStream(data);
                            var size = this.stream.Length;
                            var ret = Image.FromStream(this.stream);
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
                this.Task.Result.Dispose();
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

        /// <summary>
        /// Creates a new <see cref="DownloadImageWithoutFileTask"/>
        /// </summary>
        public DownloadImageWithoutFileTask(string url)
        {
            this.Task = new ConcurrentImageTask(() => GetAlbumImageWithoutFile(url));
        }
    }
}
