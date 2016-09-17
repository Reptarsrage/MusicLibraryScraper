namespace MusicLibraryScraper.Managers
{
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tasks;

    public class CacheManager : IDisposable
    {
        private  Dictionary<string, Task<AlbumArtResults>> _imageUrlCache;
        private  Dictionary<string, Task<AlbumArtResults>> _googleImageUrlCache;
        private  Dictionary<string, Task<FileInfo>> _imageFileCache;
        private  Dictionary<string, Task<FileInfo>> _imageOptimizedFileCache;
        private  Dictionary<string, ConcurrentImageTask> _imageLoadCache;
        private  Dictionary<string, ImageOptimizeWithoutFileTask> _imageOptimizedCache;
        private  Dictionary<string, ImageDownloadWithoutFileTask> _imageDownloadedCache;


        private  object _imageFileCacheLock = new object();
        private  object _googleImageUrlCacheLock = new object();
        private  object _imageUrlCacheLock = new object();
        private  object _imageLoadCacheLock = new object();
        private  object _imageOptimizedFileCacheLock = new object();
        private  object _imageOptimizedCacheLock = new object();
        private  object _imageDownloadedCacheLock = new object();


        private  ulong _imageCacheSize = 0;
        private const ulong MAX_SIZE = 100000000; // 100MB
        private  SortedList<long, string> _imageLoadCacheHistory;

        public void Dispose()
        {

            lock (_imageLoadCacheLock)
            {
                if (_imageLoadCache != null)
                {
                    foreach (var key in _imageLoadCache?.Keys)
                    {
                        _imageLoadCache[key].Result.Dispose();
                        _imageLoadCache[key].Dispose();
                    }
                    _imageLoadCache.Clear();
                    _imageLoadCache = null;
                }
            }

            lock (_imageOptimizedCacheLock)
            {
                if (_imageOptimizedCache != null)
                {
                    foreach (var key in _imageOptimizedCache.Keys)
                    {
                        _imageOptimizedCache[key].Dispose();
                    }
                    _imageOptimizedCache.Clear();
                    _imageOptimizedCache = null;
                }
            }


            lock (_imageDownloadedCacheLock)
            {
                if (_imageDownloadedCache != null)
                {
                    foreach (var key in _imageDownloadedCache.Keys)
                    {
                        _imageDownloadedCache[key].Dispose();
                    }
                    _imageDownloadedCache.Clear();
                    _imageDownloadedCache = null;
                }
            }

            lock (_imageUrlCacheLock)
            {
                if (_imageUrlCache != null)
                    _imageUrlCache.Clear();
                _imageUrlCache = null;
            }

            lock (_googleImageUrlCacheLock)
            {
                if (_googleImageUrlCache != null)
                    _googleImageUrlCache.Clear();
                _googleImageUrlCache = null;
            }

            lock (_imageFileCacheLock)
            {
                if (_imageFileCache != null)
                    _imageFileCache.Clear();
                _imageFileCache = null;
            }

            lock (_imageOptimizedFileCacheLock)
            {
                if (_imageOptimizedFileCache != null)
                    _imageOptimizedFileCache.Clear();
                _imageOptimizedFileCache = null;
            }
        }

        public  Task<FileInfo> GetOptimizedImage(FileInfo image, DirectoryInfo dir)
        {
            lock (_imageOptimizedFileCacheLock)
            {
                if (_imageOptimizedFileCache == null)
                {
                    _imageOptimizedFileCache = new Dictionary<string, Task<FileInfo>>();
                }

                if (_imageOptimizedFileCache.ContainsKey(image.FullName))
                {
                    return _imageOptimizedFileCache[image.FullName];
                }
                else
                {
                    var task = new ImageOptimizeTask(image, dir);
                    _imageOptimizedFileCache.Add(image.FullName, task);
                    return task;
                }
            }
        }

        public  ConcurrentImageTask GetOptimizedImage(string url, Image image)
        {
            lock (_imageOptimizedCacheLock)
            {
                if (_imageOptimizedCache == null)
                {
                    _imageOptimizedCache = new Dictionary<string, ImageOptimizeWithoutFileTask>();
                }

                if (_imageOptimizedCache.ContainsKey(url))
                {
                    return _imageOptimizedCache[url].Task;
                }
                else
                {
                    var task = new ImageOptimizeWithoutFileTask(image);
                    _imageOptimizedCache.Add(url, task);
                    return task.Task;
                }
            }
        }

        public  ConcurrentImageTask GetAlbumImage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException($"Tried to download image with NULL url.");
            }

            lock (_imageDownloadedCacheLock)
            {
                if (_imageDownloadedCache == null)
                {
                    _imageDownloadedCache = new Dictionary<string, ImageDownloadWithoutFileTask>();
                }

                if (_imageDownloadedCache.ContainsKey(url))
                {
                    return _imageDownloadedCache[url].Task;
                }
                else
                {
                    // Logger.WriteLineS("Creating task with key: " + url);
                    var task = new ImageDownloadWithoutFileTask(url);
                    _imageDownloadedCache.Add(url, task);
                    //AddToLoadImageCache(url);
                    //_imageCacheSize += (ulong)MAX_SIZE / 1000; // hold onto 1000 most recent tasks

                    // TODO
                    //if (_imageCacheSize >= MAX_SIZE)
                    //{
                    //    CleanupImageCache();
                    //}

                    return task.Task;
                }
            }
        }

        public  ConcurrentImageTask GetLoadedImage(FileInfo image)
        {
            lock (_imageLoadCacheLock)
            {
                if (_imageLoadCacheHistory == null)
                {
                    _imageLoadCacheHistory = new SortedList<long, string>();
                }
                if (_imageLoadCache == null)
                {
                    _imageLoadCache = new Dictionary<string, ConcurrentImageTask>();
                }

                if (_imageLoadCache.ContainsKey(image.FullName))
                {
                    return _imageLoadCache[image.FullName];
                }
                else
                {
                    var task = new ImageLoadTask(image);
                    _imageLoadCache.Add(image.FullName, task);
                    //AddToLoadImageCache(image.FullName);
                    // _imageCacheSize += (ulong)image.Length;

                    // TODO
                    //if (_imageCacheSize >= MAX_SIZE)
                    //{
                    //    CleanupImageCache();
                    //}
                    return task;
                }
            }
        }

        public  Task<AlbumArtResults> GetAlbumImageURLUsingGoogle(string query)
        {
            var pQuery = normalize(query);

            if (string.IsNullOrEmpty(pQuery))
            {
                throw new ArgumentNullException($"Tried to lookup album art on Google with query '{pQuery}'. Null value detected. Aborting.");
            }
            lock (_googleImageUrlCacheLock)
            {
                if (_googleImageUrlCache == null)
                {
                    _googleImageUrlCache = new Dictionary<string, Task<AlbumArtResults>>();
                }

                if (_googleImageUrlCache.ContainsKey(pQuery)) {
                    return _googleImageUrlCache[pQuery];
                } else {
                    // Logger.WriteLineS("Creating task with key: " + query);
                    var task = new GoogleImageUrlTask(pQuery);
                    _googleImageUrlCache.Add(pQuery, task);
                    return task;
                }
            }
        }

        public  Task<AlbumArtResults> GetAlbumImageURL(string artist, string album)
        {
            var pArtist = normalize(artist);
            var pAlbum = normalize(album);

            if (string.IsNullOrEmpty(pAlbum))
            {
                throw new ArgumentNullException($"Tried to lookup album art for album '{pArtist}' by artist '{pAlbum}'. Null value detected. Aborting.");
            }
            lock (_imageUrlCacheLock)
            {
                if (_imageUrlCache == null)
                {
                    _imageUrlCache = new Dictionary<string, Task<AlbumArtResults>>();
                }

                if (_imageUrlCache.ContainsKey(store(pArtist, pAlbum)))
                {
                    return _imageUrlCache[store(pArtist, pAlbum)];
                }
                else
                {
                    // Logger.WriteLineS("Creating task with key: " + store(pArtist, pAlbum));
                    var task = new ImageUrlTask(pArtist, pAlbum);
                    _imageUrlCache.Add(store(pArtist, pAlbum), task);
                    return task;
                }
            }
        }

        public  Task<FileInfo> GetAlbumImageFile(string url, string filetype, DirectoryInfo dir)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException($"Tried to download image with NULL url.");
            }

            lock (_imageFileCacheLock)
            {
                if (_imageFileCache == null)
                {
                    _imageFileCache = new Dictionary<string, Task<FileInfo>>();
                }

                if (_imageFileCache.ContainsKey(url))
                {
                    return _imageFileCache[url];
                }
                else
                {
                    // Logger.WriteLineS("Creating task with key: " + url);
                    var task = new ImageDownloadTask(url, dir, filetype);
                    _imageFileCache.Add(url, task);
                    return task;
                }
            }
        }

        private  string normalize(string s)
        {
            Regex r = new Regex("[^A-Za-z0-9 ]");
            return r.Replace(s?.Trim()?.ToLower() ?? "", "");
        }

        private  string store(string s, string s2)
        {
            return s + "/" + s2;
        }

        private  void CleanupImageCache()
        {
            // TODO
            //while (_imageCacheSize >= MAX_SIZE * 0.0)
            //{
            //    var firstTime = _imageLoadCacheHistory.Keys[0];
            //    var firstImage = _imageLoadCacheHistory[firstTime];
            //    var task2 = _imageLoadCache[firstImage];
            //    var size2 = (ulong)(new FileInfo(_imageLoadCacheHistory[firstTime])).Length;

            //    if (task2.IsCompleted)
            //    {
            //        _imageLoadCache.Remove(firstImage);
            //        _imageLoadCacheHistory.Remove(firstTime);
            //        _imageCacheSize -= size2;
            //        //try { task2.Dispose(); } catch { /* Someone's using this still */ }
            //    }
            //    else
            //    {
            //        _imageLoadCacheHistory.Remove(firstTime);
            //        AddToLoadImageCache(firstImage);
            //    }
            //}
        }

        private  void AddToLoadImageCache(string value)
        {
            var time = DateTime.Now.Ticks;
            while (_imageLoadCacheHistory.ContainsKey(time))
            {
                time = DateTime.Now.Ticks;
            }
            _imageLoadCacheHistory.Add(time, value);
        }
    }
}
