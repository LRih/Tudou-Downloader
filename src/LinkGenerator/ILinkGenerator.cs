using System;

namespace TudouDownloader
{
    public interface ILinkGenerator
    {
        //===================================================================== FUNCTIONS
        string[] GetDownloadLinks(string videoUrl, Quality targetQuality);
    }
}
