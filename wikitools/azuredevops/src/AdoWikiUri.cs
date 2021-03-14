using System;

namespace Wikitools.AzureDevOps
{
    // Example value:
    // https://dev.azure.com/adoOrg/adoProject/_wiki/wikis/wikiName
    public record AdoWikiUri(Uri Uri)
    {
        public AdoWikiUri(string uri) : this(new Uri(uri)) { }

        public string CollectionUri =>
            Uri.GetLeftPart(UriPartial.Scheme) 
            + Uri.Host 
            + Uri.Segments[0] 
            + Uri.Segments[1];

        public string ProjectName => Uri.Segments[2];

        public string WikiName => Uri.Segments[5];
    }
}