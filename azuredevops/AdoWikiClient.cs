namespace Wikitools.AzureDevOps;

public class AdoWikiClient
{
    public readonly IWikiHttpClient HttpClient;
    private readonly AdoWikiUri _uri;

    public AdoWikiClient(IWikiHttpClient httpClient, AdoWikiUri uri)
    {
        HttpClient = httpClient;
        _uri = uri;
    }
}