/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Modals
{
    /// <summary>
    /// Object for parsing and holding onto the cmd line parameters passed into this program.
    /// See Usage for details.
    /// </summary>
    public class ScraperArguments
    {
        public string SourceDir { get; set; }
        public string OutputDir { get; set; }
        public string ImageOutDir { get; set; }
        public string OptimizedImageOutDir { get; set; }
        public int ThreadCount { get; set; }
        public bool Recurse { get; set; }
        public bool Help { get; set; }
        public string Extensions { get; set; }
        public bool Clean { get; set; }
    }
}
