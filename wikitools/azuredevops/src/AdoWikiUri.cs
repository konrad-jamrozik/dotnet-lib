using System;

namespace Wikitools.AzureDevOps
{
    // Example value:
    // https://dev.azure.com/mseng/1ES/_wiki/wikis/SaCE Team Wiki
    public class AdoWikiUri
    {
        private readonly Uri _uri;

        public AdoWikiUri(string uri)
        {
            _uri = new Uri(uri);
        }

        public string CollectionUri =>
            _uri.GetLeftPart(UriPartial.Scheme) 
            + _uri.Host 
            + _uri.Segments[0] 
            + _uri.Segments[1];

        public string ProjectName => _uri.Segments[2];

        public string WikiName => _uri.Segments[5];
    }
}