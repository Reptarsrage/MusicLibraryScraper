<h1>Music Library Scraper</h1>
<hr />

Author: Justin Robb <br>
Date: 9/25/2016

<h3>Description:</h3>
<p>
    Adds album art to each file in a library of music using online image sources.
    Currently uses <a href="https://taglib.github.io/">TagLib</a>, which supports the following formats:
    <ul>
        <li>ID3v1, ID3v2, APE, FLAC, Xiph, iTunes-style MP4 and WMA tag formats.</li>
        <li>MP3, MPC, FLAC, MP4, ASF, AIFF, WAV, TrueAudio, WavPack, Ogg FLAC, Ogg Vorbis, Speex and Opus file formats.</li>
    </ul>
</p>

<h3>Installation</h3>
<p>TBD</p>

<h3>Contents</h3>
<ol>
    <li><b>MusicLibraryScraper:</b> The core executable which contains all file scraping logic.</li>
    <li><b>AmazonAlbumArtLookup:</b> A class library which queries Amazon for album art.</li>
    <li><b>MusicLibraryScraperTests:</b> A suite of unit tests for the program. </li>
    <li>
        <b>PythonDownloaderScript:</b> A python project which compiles into an executable
        which can be used to get Google Image search results.
    </li>
</ol>

<h3>Logic for the curious</h3>
<p>
    There are several steps that can be broken out into very separate pieces. I will briefly cover them here.
</p>
<ol>
    <li>
        <h4>Parse music library</h4>
        <p>
            This step involves searching a directory (either recursive or not) for all supported music files.
            Once we have this list, we copy over the files to the output directory. Then we can sweep over the
            files in the output directory in parallel and retrieve album art for each file. Since we are doing
            this in parallel, there were some challenges in making sure that the same queries were not run twice,
            and that no images are downloaded twice when they could have been downloaded once.
        </p>
    </li>
    <li>
        <h4>Get possible album image URL's</h4>
        <p>
            Using AWS Advertising APIs, we try to fetch a matching result for the album and artist of the file.
            See <a href="http://docs.aws.amazon.com/AWSECommerceService/latest/DG/CHAP_ApiReference.html">here</a>,
            <a href="https://affiliate-program.amazon.com/home/account/tag/manage">here</a>,
            and <a href="https://console.aws.amazon.com/iam/home">here</a>
            for more info on using Amazon for album art for free.
            If this fails for any reason, such as the album not existing on Amazon, or the file not having the proper tags,
            then we use the Google Image Advanced Search feature of Google in order to find suitable artwork. Using a combination of the
            file's metadata, we can create a query to use on Google Search and then parse the results.
            Using these two methods, we are almost guaranteed to get a list of image URL's returned which are possible
            choices for the file's album art. This process short-circuits, so that if we get results using Amazon, Google is never used.
        </p>
    </li>
    <li>
        <h4>Download image and store</h4>
        <p>
            Using the list of URL’s, we can go down the list and find one that we can download. Once downloaded we can either choose to
            save the image bits in a cache, or save the image to a file and host the file's path in the cache. There are benefits of both, such
            as the image store being faster due to reduced I/O but taking up way more memory, and the file-store taking up more time to read/write but
            far less in-memory usage (in exchange for on-disk memory usage).
        </p>
    </li>
    <li>
        <h4>Optimize image and store</h4>
        <p>
            We only need the images to be 600x600, anything more is just overkill.
            Therefore, we can take the downloaded image and optimize it by reducing its sizer to
            fit the needed dimensions. We can convert everything to a lossy format like jpg and
            reduce the quality on the image itself to 90%. This quality reduction is unnoticeable
            and saves a bit of memory. This helps keep tagged image file size down to less than a 1% size gain.
            Again, we have to decide whether to save the optimized image to disk or keep it cached in memory.
        </p>
    </li>
    <li>
        <h4>Tag file with image</h4>
        <p>
            Now that we have an optimized image file and a music file ready to be tagged, we can use TagLib
            to insert the image into the music file, replacing any old artwork. And that's it, we're done!
        </p>
    </li>
</ol>

<h3>Technologies Used</h3>
<ul>
    <li><a href="https://taglib.github.io/">TagLib</a></li>
    <li><a href="http://docs.aws.amazon.com/AWSECommerceService/latest/DG/CHAP_ApiReference.html">Amazon AWS Advertising API</a></li>
    <li><a href="https://www.google.com/advanced_image_search">Google Advanced Image Search</a></li>
    <li><a href="http://www.py2exe.org/">py2exe</a></li>
    <li><a href="http://wwwsearch.sourceforge.net/mechanize/">mechanize</a></li>
    <li><a href="https://www.crummy.com/software/BeautifulSoup/">BeautifulSoup</a></li>
    <li><a href="https://www.nuget.org/packages/FluentCommandLineParser/">Fluent Command Line Parser</a></li>
    <li><a href="https://www.nuget.org/packages/NUnit/">NUnit Test Framework</a></li>
</ul>

<h3>TODO</h3>
<ul>
    <li>Write more unit tests</li>
    <li>Implement cache cleanup logic so that the program releases some of its older data. </li>
    <li>If no image is found for the 'artis + album' query, craete a queue of secondary queries to use so that nmop file ever has 0 results.</li>
    <li>Re-design OptimizeImageWithoutFileTask and ImageDownloadWithoutFileTask</li>
</ul>