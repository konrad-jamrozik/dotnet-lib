﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public class AdoWiki
    {
        private readonly IAdoApi _adoApi;
        private readonly AdoWikiUri _adoWikiUri;
        private readonly string _patEnvVar;
        private readonly int _pageViewsForDays;

        public AdoWiki(IAdoApi adoApi, AdoWikiUri adoWikiUri, string patEnvVar, int pageViewsForDays)
        {
            _adoApi = adoApi;
            _adoWikiUri = adoWikiUri;
            _patEnvVar = patEnvVar;
            _pageViewsForDays = pageViewsForDays;
        }

        public async Task<List<WikiPageStats>> GetPagesStats()
        {
            if (!(_pageViewsForDays > 0))
                throw new ArgumentOutOfRangeException(nameof(_pageViewsForDays),
                    $"Expected value > 0; value = {_pageViewsForDays}");
            if (!(_pageViewsForDays <= 30))
                throw new ArgumentOutOfRangeException(nameof(_pageViewsForDays),
                    $"Expected value <= 30 as this is the ADO API limit; value = {_pageViewsForDays}");

            List<WikiPageStats> pagesStats = await _adoApi.GetWikiPagesStats(_adoWikiUri, _patEnvVar, _pageViewsForDays);
            return pagesStats;
        }

    }
}