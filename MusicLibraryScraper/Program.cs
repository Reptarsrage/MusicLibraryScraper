namespace MusicLibraryScraper
{
    using Fclp;
    using Managers;
    using Modals;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        static Stack<FileInfo> _files;
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
                Parallel.ForEach(
                     _files,
                     new ParallelOptions { MaxDegreeOfParallelism = options.ThreadCount },
                     file => { Scrape(options, file); }
                 );

                Logger.Write("\nDisposing of resources...");
                CacheManager.DisposeAll();
                Logger.Write("Done!\n");
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

        private static void Scrape(ScraperArguments options, FileInfo file) {
            try
            {

                var scraper = new MusicFileScraper();
                scraper.Scrape(options, file);
            }
            catch (Exception e)
            {
                Logger.WriteError($"Failed to scrape file {file.Name}. {e.InnerException?.Message ?? e.Message}");
            }
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
