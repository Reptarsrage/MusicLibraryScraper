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
    using System;
    using System.Threading;

    /// <summary>
    /// Utility for logging and output
    /// </summary>
    public static class Logger
    {
        #region private fields
        private static int _progress = int.MaxValue;
        private static int _ImageURLRequestCounter = 0;
        private static int _ImagesDownloadedCounter = 0;
        private static long _taskTimeTotal = 0;
        private static int _taskTotal = 0;
        private static int _taskCompleted = 0;
        private static int _taskFoundTotal = 0;
        private static int _loadedImages = 0;

        private static int _googleRequestCounter = 0;
        private static long _imageDownloadSize = 0;
        private static long _imageOptimizedSize = 0;
        private static long _origFileSize = 0;
        private static long _finalFileSize = 0;

        private static object consoleLock = new object();
        #endregion

        #region public fields
        /// <summary>
        /// Count of Google Search Requests made
        /// </summary>
        public static int GoogleRequestCounter
        {
            get
            {
                return _googleRequestCounter;
            }
        }
        /// <summary>
        /// Count of total files to be scraped
        /// </summary>
        public static int TotalFilesScraping { get; set; }
        /// <summary>
        /// Size of all downlaoded images
        /// </summary>
        public static long ImageDownloadSize
        {
            get
            {
                return _imageDownloadSize;
            }
        }
        /// <summary>
        /// Count of loaded images from files
        /// </summary>
        public static long LoadedImageCount
        {
            get
            {
                return _loadedImages;
            }
        }
        /// <summary>
        /// Size of origional scraped files
        /// </summary>
        public static long OrigFileSize
        {
            get
            {
                return _origFileSize;
            }
        }
        /// <summary>
        /// Size of images after optimization
        /// </summary>
        public static long OptimalImageSize
        {
            get
            {
                return _imageOptimizedSize;
            }
        }
        /// <summary>
        /// Size of final scraped files
        /// </summary>
        public static long FinalFileSize
        {
            get
            {
                return _finalFileSize;
            }
        }
        /// <summary>
        /// Count of downloaded images
        /// </summary>
        public static long ImageDownloadCount
        {
            get
            {
                return _ImagesDownloadedCounter;
            }
        }
        /// <summary>
        /// Count of requests made to Amazon API
        /// </summary>
        public static long AmazonRequestCounter
        {
            get
            {
                return _ImageURLRequestCounter;
            }
        }
        /// <summary>
        /// Total time for each file to be scraped
        /// </summary>
        public static long TaskTime
        {
            get
            {
                return _taskTimeTotal;
            }
        }
        /// <summary>
        /// Total number of files scraped (completed)
        /// </summary>
        public static long TaskCount
        {
            get
            {
                return _taskTotal;
            }
        }
        /// <summary>
        /// Couint of files suceesfully scraped (success)
        /// </summary>
        public static int TaskCompleted
        {
            get
            {
                return _taskCompleted;
            }
        }
        /// <summary>
        /// Count of files for which art was sucessfully found
        /// </summary>
        public static long TaskFoundCount
        {
            get
            {
                return _taskFoundTotal;
            }
        }
        #endregion

        #region Incrementers
        /// <summary>
        /// Increments count of requests to Amazon APIs
        /// </summary>
        public static void IncrementUrlCount()
        {
            Interlocked.Increment(ref _ImageURLRequestCounter);
        }
        /// <summary>
        /// Increments count of images loaded from file
        /// </summary>
        public static void IncrementLoadedImageCount()
        {
            Interlocked.Increment(ref _loadedImages);
        }
        /// <summary>
        /// Increments count of scaped files (completed)
        /// </summary>
        public static void IncrementTaskTotal()
        {
            Interlocked.Increment(ref _taskTotal);
        }
        /// <summary>
        /// Increments count of successfully scaped files (success)
        /// </summary>
        public static void IncrementTaskCompleted()
        {
            Interlocked.Increment(ref _taskCompleted);
        }
        /// <summary>
        /// Increments count of found files
        /// </summary>
        public static void IncrementTaskFound()
        {
            Interlocked.Increment(ref _taskFoundTotal);
        }
        /// <summary>
        /// Adds to total time spent scraping files
        /// </summary>
        public static void AddTaskTime(long ms)
        {
            Interlocked.Add(ref _taskTimeTotal, ms);
        }
        /// <summary>
        /// Increments count of downlaoded images
        /// </summary>
        public static void IncrementDownlaodedImageCount()
        {
            Interlocked.Increment(ref _ImagesDownloadedCounter);
        }
        /// <summary>
        /// Adds to total final file size
        /// </summary>
        public static void AddFinalFileSize(long size)
        {
            Interlocked.Add(ref _finalFileSize, size);
        }
        /// <summary>
        /// Adds to total origional file size
        /// </summary>
        public static void AddOrigionalFileSize(long size)
        {
            Interlocked.Add(ref _origFileSize, size);
        }
        /// <summary>
        /// Adds to total optimized image size
        /// </summary>
        public static void AddOptimizedImageSize(long size)
        {
            Interlocked.Add(ref _imageOptimizedSize, size);
        }
        /// <summary>
         /// Adds to total downloaded image size
         /// </summary>
        public static void AddImageDownloadSize(long size)
        {
            Interlocked.Add(ref _imageDownloadSize, size);
        }
        /// <summary>
        /// Increments count of Google requests made
        /// </summary>
        public static void IncrementGooglerequestCounter()
        {
            Interlocked.Increment(ref _googleRequestCounter);
        }
        #endregion

        /// <summary>
        /// Draws progress to console. Avoid writing to console during this time.
        /// </summary>
        public static void drawTextProgressBar(int progress, int total)
        {
            float onechunk = total / 30.0f;
            if (progress < _progress)
            {
                // RESET
                _progress = 1;
                for (int i = 0; i < 31; i++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.CursorLeft = i;
                    Console.Write(" ");
                }

                //draw empty progress bar
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = 32;
                Console.Write("]"); //end
            }

            if (progress > _progress + onechunk) // made one chunck of progress
            {
                

                //draw filled part
                int oldPos = (int)Math.Ceiling((_progress / (float)total) * 31.0F);
                int position = (int)Math.Ceiling((progress / (float)total) * 31.0F);

                for (int i = oldPos; i <= position; i++)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.CursorLeft = i;
                    Console.Write(" ");
                }
                _progress = progress;
            }
           
            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }

        /// <summary>
        /// Resets all stats
        /// </summary>
        public static void ResetAll()
        {
            _ImageURLRequestCounter = 0;
            _ImagesDownloadedCounter = 0;
            _taskTimeTotal = 0;
            _taskTotal = 0;
            _taskCompleted = 0;
            _taskFoundTotal = 0;
            _loadedImages = 0;

            _googleRequestCounter = 0;
            _imageDownloadSize = 0;
            _imageOptimizedSize = 0;
            _origFileSize = 0;
            _finalFileSize = 0;
        }

        /// <summary>
        /// Utility function to print red lines.
        /// </summary>
        /// <param name="s">string to print</param>
        public static void WriteError(string s)
        {
            lock (consoleLock)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(s);
                Console.ForegroundColor = c;
            }
        }

        /// <summary>
        /// Utility function to print green lines.
        /// </summary>
        /// <param name="s">string to print</param>
        public static void WriteSuccess(string s)
        {
            lock (consoleLock)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(s);
                Console.ForegroundColor = c;
            }
        }

        /// <summary>
        /// Utility function to print yellow lines.
        /// </summary>
        /// <param name="s">string to print</param>
        public static void WriteWarning(string s)
        {
            lock (consoleLock)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(s);
                Console.ForegroundColor = c;
            }
        }

        /// <summary>
        /// Utility function to print normal lines.
        /// </summary>
        /// <param name="s">string to print</param>
        public static void Write(string s)
        {
            lock (consoleLock)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// Utility function to print normal lines.
        /// </summary>
        /// <param name="s">string to print</param>
        public static void WriteLine(string s)
        {
            lock (consoleLock)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// Utility function to print special blue lines.
        /// </summary>
        /// <param name="s">string to print</param>
        internal static void WriteLineS(string s)
        {
            lock (consoleLock)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(s);
                Console.ForegroundColor = c;
            }
        }
    }
}
