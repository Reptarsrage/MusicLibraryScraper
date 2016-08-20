namespace MusicLibraryScraper
{
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tasks;

    public static class CacheManager
    {
        private static Dictionary<string, Task<string>> _imageUrlCache;
        private static Dictionary<string, Task<GoogleResult>> _googleImageUrlCache;
        private static Dictionary<string, Task<FileInfo>> _imageFileCache;
        private static object _imageFileCacheLock = new object();
        private static object _googleImageUrlCacheLock = new object();
        private static object _imageUrlCacheLock = new object();
        private static object _imageLoadCacheLock = new object();
        private static ulong _imageCacheSize = 0;
        private const ulong MAX_SIZE = 100000000; // 100MB
        private static SortedList<long, string> _imageLoadCacheHistory;
        private static Dictionary<string, Task<Image>> _imageLoadCache;

        private static string store(string s, string s2)
        {
            return s + "/" + s2;
        }

        public static Task<Image> GetLoadedImage(FileInfo image)
        {
            lock (_imageLoadCacheLock)
            {
                if (_imageLoadCacheHistory == null)
                {
                    _imageLoadCacheHistory = new SortedList<long, string>();
                }
                if (_imageLoadCache == null)
                {
                    _imageLoadCache = new Dictionary<string, Task<Image>>();
                }

                if (_imageLoadCache.ContainsKey(image.FullName))
                {
                    return _imageLoadCache[image.FullName];
                }
                else
                {
                    var task = new ImageLoadTask(image);
                    _imageLoadCache.Add(image.FullName, task);
                    AddToLoadImageCache(image.FullName);
                     _imageCacheSize += (ulong)image.Length;

                    if (_imageCacheSize >= MAX_SIZE)
                    {
                        while (_imageCacheSize >= MAX_SIZE * 0.70)
                        {
                            var firstTime = _imageLoadCacheHistory.Keys[0];
                            var firstImage = _imageLoadCacheHistory[firstTime];
                            var task2 = _imageLoadCache[firstImage];
                            var size2 = (ulong)(new FileInfo(_imageLoadCacheHistory[firstTime])).Length;

                            if (task2.IsCompleted)
                            {
                                _imageLoadCache.Remove(firstImage);
                                _imageLoadCacheHistory.Remove(firstTime);
                                _imageCacheSize -= size2;
                                //try { task2.Dispose(); } catch { /* Someone's using this still */ }
                            }
                            else
                            {
                                _imageLoadCacheHistory.Remove(firstTime);
                                AddToLoadImageCache(firstImage);
                            }
                        }
                    }
                    return task;
                }
            }
        }

        private static void AddToLoadImageCache(string value)
        {
            var time = DateTime.Now.Ticks;
            while (_imageLoadCacheHistory.ContainsKey(time))
            {
                time = DateTime.Now.Ticks;
            }
            _imageLoadCacheHistory.Add(time, value);
        }

        public static Task<GoogleResult> GetAlbumImageURLUsingGoogle(string query)
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
                    _googleImageUrlCache = new Dictionary<string, Task<GoogleResult>>();
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

        public static Task<string> GetAlbumImageURL(string artist, string album)
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
                    _imageUrlCache = new Dictionary<string, Task<string>>();
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

        private static string normalize(string s)
        {
            Regex r = new Regex("[^A-Za-z0-9 ]");
            return r.Replace(s?.Trim()?.ToLower() ?? "", "");
        }

        public static Task<FileInfo> GetAlbumImageFile(string url, string filetype, DirectoryInfo dir)
        {
            if (string.IsNullOrWhiteSpace(filetype))
            {
                throw new System.Exception("HELP");
            }

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
                    var task = new ImageTask(url, dir, filetype);
                    _imageFileCache.Add(url, task);
                    return task;
                }
            }
        }
    }
}
