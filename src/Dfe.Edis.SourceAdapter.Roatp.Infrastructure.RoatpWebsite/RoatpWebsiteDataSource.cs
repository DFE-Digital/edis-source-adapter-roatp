using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Dfe.Edis.SourceAdapter.Roatp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite.Csv;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite
{
    public class RoatpWebsiteDataSource : IRoatpDataSource
    {
        private const string StateKeyDownloadLink = "website-download-link";
        
        private readonly HttpClient _httpClient;
        private readonly IStateStore _stateStore;
        private readonly SourceDataConfiguration _configuration;
        private readonly ILogger<RoatpWebsiteDataSource> _logger;

        public RoatpWebsiteDataSource(
            HttpClient httpClient,
            IStateStore stateStore,
            SourceDataConfiguration configuration,
            ILogger<RoatpWebsiteDataSource> logger)
        {
            _httpClient = httpClient;
            _stateStore = stateStore;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<ApprenticeshipProvider[]> GetDataAsync(CancellationToken cancellationToken)
        {
            var downloadLink = await GetDownloadLinkAsync(cancellationToken);
            var isNewDownloadLink = await IsNewDownloadLink(downloadLink, cancellationToken);
            if (!isNewDownloadLink)
            {
                _logger.LogInformation("Download link has not changed");
                return new ApprenticeshipProvider[0];
            }
            
            var csv = await DownloadCsvAsync(downloadLink, cancellationToken);

            var apprenticeshipProviders = ParseCsv(csv);

            await _stateStore.SetStateAsync(StateKeyDownloadLink, downloadLink, cancellationToken);
            
            return apprenticeshipProviders;
        }

        
        private async Task<string> GetDownloadLinkAsync(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_configuration.RoatpDownloadPageUrl, cancellationToken);
            var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"Error trying to get CSV link from RoATP website. Http status code: {(int) response.StatusCode}";
                if (!string.IsNullOrEmpty(content))
                {
                    errorMessage += $"{Environment.NewLine}{content}";
                }
                throw new Exception(errorMessage);
            }
            
            var page = new HtmlDocument();
            page.LoadHtml(content);

            var attachmentLinkContainer = page.DocumentNode.Descendants()
                .SingleOrDefault(e => e.HasClass("attachment-details"));
            if (attachmentLinkContainer == null)
            {
                throw new Exception("Failed to find element with class attachment-details on RoATP download page");
            }

            var link = attachmentLinkContainer.Descendants("a").SingleOrDefault();
            if (link == null)
            {
                throw new Exception("Link container did not contain anchor element on RoATP download page");
            }

            var href = link.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(href))
            {
                throw new Exception("Anchor tag in link container did not have href attribute on RoATP download page");
            }

            return new Uri(new Uri(_configuration.RoatpDownloadPageUrl, UriKind.Absolute), new Uri(href, UriKind.Relative)).AbsoluteUri;
        }

        private async Task<bool> IsNewDownloadLink(string downloadLink, CancellationToken cancellationToken)
        {
            var previousState = await _stateStore.GetStateAsync(StateKeyDownloadLink, cancellationToken);
            
            return string.IsNullOrEmpty(previousState) || 
                   !previousState.Equals(downloadLink, StringComparison.InvariantCultureIgnoreCase);
        }
        
        private async Task<string> DownloadCsvAsync(string downloadLink, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(downloadLink, cancellationToken);
            var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"Error trying to download csv from RoATP website. Http status code: {(int) response.StatusCode}";
                if (!string.IsNullOrEmpty(content))
                {
                    errorMessage += $"{Environment.NewLine}{content}";
                }
                throw new Exception(errorMessage);
            }

            return content;
        }

        private ApprenticeshipProvider[] ParseCsv(string csv)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            using var reader = new StreamReader(stream);
            using var parser = new PublishedRoatpCsvParser(reader);

            var rows = parser.GetRecords();

            return rows;
        }
    }
}