using System;

namespace Wikitools.AzureDevOps
{
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