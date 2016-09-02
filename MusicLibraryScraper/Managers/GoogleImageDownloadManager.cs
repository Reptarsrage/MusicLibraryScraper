namespace MusicLibraryScraper.Managers
{
    using Modals;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml.Serialization;

    class GoogleImageDownloadManager
    {
        private int PROC_TIMOUT = 3000;

        public GoogleResults GetGoogleResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new System.NullReferenceException("Can't run a command with invalid query and params.");
            }

            query = "\"" + WebUtility.UrlEncode(query) + "\"";

            var results = runProcess(query);

            if (results.Error != null) {
                throw new Exception($"Error getting image '{query}' from google: {results.Error}");
            }

            return results;
        }


        private GoogleResults runProcess(string query)
        {
            // runs python script
            bool kosher;
            string outString = null;
            string outErr = null;
            int exitcode;


            Logger.WriteLine($"Qerying google for {query}");
            using (Process process = new Process())
            {
                process.StartInfo.FileName = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName, @"PythonDownloaderScript\build\dist\GetImageFromGoogle.exe");
                process.StartInfo.Arguments = query;
                process.StartInfo.WorkingDirectory = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName, @"PythonDownloaderScript\build\dist\");
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data != null)
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (process.WaitForExit(PROC_TIMOUT))
                {
                    process.WaitForExit(); // http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/
                    // Process completed. Check process.ExitCode here.
                    kosher = true;
                    exitcode = process.ExitCode;
                    outString = output.ToString();
                    outErr = error.ToString();
                }
                else
                {
                    process.WaitForExit();
                    // Timed out.
                    kosher = false;
                    exitcode = 1;
                }
            }

            //Logger.WriteLineS($"({exitcode}) ({kosher})");

            if (kosher)
            {
                // Process completed. Check process.ExitCode here.
                if (exitcode == 0 && !string.IsNullOrEmpty(outString))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(GoogleResults));
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(outString ?? "")))
                    {
                        var results = (GoogleResults)xml.Deserialize(stream);
                        return results;
                    }
                }
                else
                {
                    throw new Exception($"Error getting image '{query}' from google: {outErr}");
                }
            }
            else
            {
                if (PROC_TIMOUT >= 30000)
                {
                    // Timed out.
                    throw new TimeoutException("Timed out getting google image '{query}'.");
                }
                else
                {
                    PROC_TIMOUT *= 2;
                    return runProcess(query);
                }
            }
        }
    }
}
