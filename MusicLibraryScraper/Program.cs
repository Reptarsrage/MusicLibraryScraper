namespace MusicLibraryScraper
{
    using Fclp;
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private static List<Task> _runningThreads = new List<Task>();
        static Stack<FileInfo> _files;
        static Dictionary<int, string> _taskDict = new Dictionary<int, string>();
        private static FluentCommandLineParser<ScraperArguments> _parser;

        static void Main(string[] args)
        {
            try
            {
                var options = ParseCommandLineOptions(args);
                if (options == null)
                {
                    throw new ArgumentException();
                }
                else if (options.Help)
                {
                    return;
                }

                // Initialize
                MusicFileScraper scraper = new MusicFileScraper();
                _files = scraper.DirSearch(options);

                //Copy files to final dest
                Logger.WriteLine($"\n\nCopying music files to {options.OutputDir}");
                var temp = new Stack<FileInfo>();
                var ct = _files.Count;
                while (_files.Count > 0)
                {
                    Logger.drawTextProgressBar(ct - _files.Count, ct);
                    File.Copy(_files.Peek().FullName, Path.Combine(options.OutputDir, _files.Peek().Name), true);

                    temp.Push(new FileInfo(Path.Combine(options.OutputDir, _files.Peek().Name)));
                    _files.Pop();
                }

                while (temp.Count > 0)
                {
                    _files.Push(temp.Pop());
                }
                Logger.WriteLine($"\nFinished copying files.\n\n");

                Logger.TotalFilesScraping = _files.Count;

                // Start Parrallel Work
                while (_files.Any() || _runningThreads.Any())
                {
                    var completed = _runningThreads.Where(task => task.IsCompleted).ToArray();
                    ThreadExited(completed);

                    _runningThreads = _runningThreads.Where(task => !task.IsCompleted).ToList();
                    var availableThreads = options.ThreadCount - _runningThreads.Count;

                    StartParallelScraperProcesses(options, availableThreads);


                   //Logger.WriteLine($"({Math.Round(Logger.TaskCount / (double)Logger.TotalFilesScraping * 100.00,2)}%) Complete");
                   Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Logger.WriteError(e.Message + "\n" + e.InnerException + "\n" + e.StackTrace);
                return;
            }
            finally
            {
                Logger.WriteLine($"{Logger.ImageUrlCount} amazon API calls made.");
                Logger.WriteLine($"{Logger.GoogleRequestCounter} google searches made.");
                Logger.WriteLine($"{Logger.ImageCount} images fetched.");
                Logger.WriteLine($"{Math.Round(Logger.ImageDownloadSize / 1024.0 / 1024.0, 2)}MB downloaded.");
                Logger.WriteLine($"{Math.Round(Logger.OrigFileSize / 1024.0 / 1024.0, 2)}MB origional files scraped.");
                Logger.WriteLine($"{Math.Round(Logger.FinalFileSize / 1024.0 / 1024.0, 2)}MB total final files size.");
                Logger.WriteLine($"{Math.Round(Logger.FinalFileSize / (double)Logger.OrigFileSize, 2)}% total files size increase.");
                Logger.WriteLine($"{Logger.TaskCount} files processed.");
                Logger.WriteLine($"{Logger.TaskFoundCount} album arts found successfully ({Math.Round(Logger.TaskFoundCount / (double)Logger.TaskCount * 100.00, 2)}%).");
                Logger.WriteLine($"{Logger.TaskCount - Logger.TaskFoundCount} album arts not found ({Math.Round((Logger.TaskCount - (double)Logger.TaskFoundCount) / (double)Logger.TaskCount * 100.00, 2)}%).");
                Logger.WriteLine($"{Logger.TaskCompleted} files successfully processed ({Math.Round(Logger.TaskCompleted / (double)Logger.TaskCount * 100.00, 2)}%).");
                Logger.WriteLine($"{Logger.TaskCount - Logger.TaskCompleted} files failed to process ({Math.Round((Logger.TaskCount - (double)Logger.TaskCompleted) / (double)Logger.TaskCount * 100.00, 2)}%).");
                Logger.WriteLine($"{Math.Round((Logger.TaskTime / 1000.00), 2)} total seconds elapsed.");
                Logger.WriteLine($"{Math.Round((Logger.TaskTime / (double)Logger.TaskCount) / 1000.00, 2)}s per file.");
                Logger.WriteLine("\nPress any key to exit");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Run when a scraper thread has finished running either due to error, success, or abortion.
        /// </summary>
        /// <param name="tasks">Array of completed tasks</param>
        private static void ThreadExited(Task[] tasks)
        {
            foreach (var task in tasks)
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    //Logger.WriteSuccess($"Successfully scraped file {_taskDict[task.Id]}");
                }
                else if (task.Status == TaskStatus.Faulted)
                {
                    Logger.WriteError($"Failed to scrape file {_taskDict[task.Id]}. {task.Exception.InnerException?.Message}");
                }
                else
                {
                    Logger.WriteError($"Failed to scrape file {_taskDict[task.Id]}. Task Aborted.");
                }
            }
        }

        /// <summary>
        /// Starts as many parallel scrapers as specified in the thread cmd line parameter.
        /// </summary>
        /// <param name="options">Command line parameter options. <see cref="ScraperArguments"/></param>
        /// <param name="availableThreads">number of slots for parallel threads</param>
        private static void StartParallelScraperProcesses(ScraperArguments options, int availableThreads)
        {
            var taskFactory = new TaskFactory();
            for (var numStarted = 0; numStarted < availableThreads && _files.Count > 0; numStarted++)
            {
                var file = _files.Pop();
                var task = taskFactory.StartNew(() => Scrape(options, file));
                _runningThreads.Add(task);
                _taskDict.Add(task.Id, file.Name);
            }
        }

        private static void Scrape(ScraperArguments options, FileInfo file) {
            var scraper = new MusicFileScraper();
            scraper.Scrape(options, file);
        }

        /// <summary>
        /// Prints usage information
        /// </summary>
        static void Usage()
        {
            // triggers the SetupHelp Callback which writes the text to the console
            _parser.HelpOption.ShowHelp(_parser.Options);
        }

        /// <summary>
        /// Fluent Command Line Parser configuration. Take cmd line parameters and parses them.
        /// </summary>
        /// <param name="args">Raw cmd line arguments.</param>
        /// <returns>A <see cref="ScraperArguments"/> object containing all pcmd line parameter information.</returns>
        private static ScraperArguments ParseCommandLineOptions(string[] args)
        {
            _parser = new FluentCommandLineParser<ScraperArguments>();
            var recurse = false;
            var clean = false;

            _parser.Setup<bool>(arguments => arguments.Recurse)
                .As('r', "recursive")
                .SetDefault(true)
                .WithDescription("Recursively search the Music directory. Default is true.");

            _parser.Setup<bool>(arguments => arguments.Clean)
                .As('c', "clearall")
                .SetDefault(false)
                .WithDescription("Remove all previous album art images from the music files. Default is false.");

            _parser.Setup<string>(arguments => arguments.SourceDir)
                .As('d', "directory")
                .Required()
                .WithDescription("Path to the Music directory containing the files.");

            _parser.Setup<int>(arguments => arguments.ThreadCount)
                .As('t', "threads")
                .SetDefault(4)
                .WithDescription("Number of concurrent threads to run. Default is 4.");

            _parser.Setup<string>(arguments => arguments.OutputDir)
                .As('o', "output")
                .Required()
                .WithDescription("Path to the directory where tagged music files should be saved.");

            _parser.Setup<string>(arguments => arguments.ImageOutDir)
                .As('i', "imageout")
                .Required()
                .WithDescription("Path to the directory where image files should be saved.");

            _parser.Setup<string>(arguments => arguments.OptimizedImageOutDir)
                .As('p', "optimizedimageout")
                .Required()
                .WithDescription("Path to the directory where optimized image files should be saved.");

            _parser.Setup<string>(arguments => arguments.Extensions)
                .As('e', "extensions")
                .SetDefault(".asf,.wma,.aif,.aiff,.flac,.mpc,.mp+,.mpp,.ape,.mp3,.m4a,.m4p,.ogg,.wav,.wv")
                .WithDescription("The comma separated file patterns to scrape for when finding music files. (e.g. \"*.mp3,*.flac\"). Default is all supported file types.");

            _parser.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine("\nUsage:\n" + text));

            var result = _parser.Parse(args);

            if (result.HasErrors)
            {
                Logger.WriteError(result.Errors.Aggregate("Errors parsing the command line arguments:",
                    (seed, error) =>
                    {
                        var commandLineOption = error.Option;
                        var erroredItem = commandLineOption.HasLongName ? commandLineOption.LongName : commandLineOption.ShortName;
                        return $"{seed}{Environment.NewLine}  \"{erroredItem}:{{{commandLineOption.Description}}}\"";
                    }));
                return null;
            }

            return _parser.Object;
        } 
    }
}
