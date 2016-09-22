/// <summary>
/// Author: Justin Robb
/// Date: 8/6/2016
/// 
/// Description:
/// Utility for logging and output
/// 
/// </summary>

namespace MusicLibraryScraper
{
    using System;
    using System.Threading;

    /// Singleton utility for logging and output
    public static class Logger
    {
        #region private fields
        private static int _progress = int.MaxValue;
        private static int _ImageURLRequestCounter = 0;
        private static int _ImageRequestCounter = 0;
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
        public static int GoogleRequestCounter
        {
            get
            {
                return _googleRequestCounter;
            }
        }

        public static int TotalFilesScraping { get; set; }

        public static long ImageDownloadSize
        {
            get
            {
                return _imageDownloadSize;
            }
        }
        public static long LoadedImageCount
        {
            get
            {
                return _loadedImages;
            }
        }
        public static long OrigFileSize
        {
            get
            {
                return _origFileSize;
            }
        }
        public static long OptimalImageSize
        {
            get
            {
                return _imageOptimizedSize;
            }
        }

        public static long FinalFileSize
        {
            get
            {
                return _finalFileSize;
            }
        }

        public static long ImageCount
        {
            get
            {
                return _ImageRequestCounter;
            }
        }

        public static long ImageUrlCount
        {
            get
            {
                return _ImageURLRequestCounter;
            }
        }

        public static long TaskTime
        {
            get
            {
                return _taskTimeTotal;
            }
        }
        public static long TaskCount
        {
            get
            {
                return _taskTotal;
            }
        }

        public static int TaskCompleted
        {
            get
            {
                return _taskCompleted;
            }
        }

        public static long TaskFoundCount
        {
            get
            {
                return _taskFoundTotal;
            }
        }
        #endregion
        #region Incrementers
        public static void IncrementUrlCount()
        {
            Interlocked.Increment(ref _ImageURLRequestCounter);
        }
        public static void IncrementLoadedImageCount()
        {
            Interlocked.Increment(ref _loadedImages);
        }

        public static void IncrementTaskTotal()
        {
            Interlocked.Increment(ref _taskTotal);
        }

        public static void IncrementTaskCompleted()
        {
            Interlocked.Increment(ref _taskCompleted);
        }

        public static void IncrementTaskFound()
        {
            Interlocked.Increment(ref _taskFoundTotal);
        }

        public static void AddTaskTime(long ms)
        {
            Interlocked.Add(ref _taskTimeTotal, ms);
        }

        public static void IncrementImageCount()
        {
            Interlocked.Increment(ref _ImageRequestCounter);
        }
        public static void AddFinalFileSize(long size)
        {
            Interlocked.Add(ref _finalFileSize, size);
        }
        public static void AddOrigionalFileSize(long size)
        {
            Interlocked.Add(ref _origFileSize, size);
        }
        public static void AddOptimizedImageSize(long size)
        {
            Interlocked.Add(ref _imageOptimizedSize, size);
        }
        public static void AddImageDownloadSize(long size)
        {
            Interlocked.Add(ref _imageDownloadSize, size);
        }
        public static void IncrementGooglerequestCounter()
        {
            Interlocked.Increment(ref _googleRequestCounter);
        }
        #endregion

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
            _ImageRequestCounter = 0;
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
