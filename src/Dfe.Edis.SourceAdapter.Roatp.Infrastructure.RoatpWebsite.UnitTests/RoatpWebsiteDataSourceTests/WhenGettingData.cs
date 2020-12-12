using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite.UnitTests.RoatpWebsiteDataSourceTests
{
    public class WhenGettingData
    {
        private HttpClientMock _httpClientMock;
        private SourceDataConfiguration _configuration;
        private Uri _absoluteDownloadUri;
        private Mock<ILogger<RoatpWebsiteDataSource>> _loggerMock;
        private RoatpWebsiteDataSource _dataSource;
        

        [SetUp]
        public void Arrange()
        {
            _configuration = new SourceDataConfiguration
            {
                RoatpDownloadPageUrl = "https://roatp.website.url/place"
            };
            _absoluteDownloadUri = new Uri(new Uri(_configuration.RoatpDownloadPageUrl, UriKind.Absolute),
                new Uri("/data.csv", UriKind.Relative));
            
            _httpClientMock = new HttpClientMock();
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><head></head><body><div class=\"attachment-details\"><a href=\"/data.csv\">download link</a></div></body></html>")
                });
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.Equals(_absoluteDownloadUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "Ukprn,Name,ProviderType,ParentCompanyGuarantee,NewOrganisationWithoutFinancialTrackRecord,StartDate,ProviderNotCurrentlyStartingNewApprentices,ApplicationDeterminedDate\n" +
                        "10001001,Provider One,Main provider,False,False,10/12/2020,,30/07/2020\n" + 
                        "10001002,Provider One,Main provider,False,False,13/03/2017,,28/08/2019")
                });
            
            _loggerMock = new Mock<ILogger<RoatpWebsiteDataSource>>();
            
            _dataSource = new RoatpWebsiteDataSource(
                _httpClientMock.AsHttpClient(),
                _configuration,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldReadRoatpWebPage()
        {
            await _dataSource.GetDataAsync(CancellationToken.None);
            
            _httpClientMock.Verify(req => req.RequestUri.AbsoluteUri.Equals(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase),
                MockTheWeb.Times.Once());
        }

        [Test]
        public async Task ThenItShouldDownloadTheCsvReferencedOnTheRoatpWebPage()
        {
            await _dataSource.GetDataAsync(CancellationToken.None);
            
            _httpClientMock.Verify(req => req.RequestUri.AbsoluteUri.Equals(_absoluteDownloadUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase),
                MockTheWeb.Times.Once());
        }

        [Test]
        public async Task ThenItShouldReturnTheProviderInformationFromTheCsv()
        {
            var results = await _dataSource.GetDataAsync(CancellationToken.None);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(10001001, results[0].Ukprn);
            Assert.AreEqual(10001002, results[1].Ukprn);
        }

        [Test]
        public void ThenItShouldThrowExceptionIfCannotOpenRoatpWebPage()
        {
            var responseContent = "<html><head></head><body>Bad things have happened</body></html>";
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(responseContent)
                });

            var actual = Assert.ThrowsAsync<Exception>(async () => await _dataSource.GetDataAsync(CancellationToken.None));
            Assert.AreEqual($"Error trying to get CSV link from RoATP website. Http status code: 500{Environment.NewLine}{responseContent}", actual.Message);
        }

        [Test]
        public void ThenItShouldThrowExceptionIfCannotFindLinkContainerInRoatpWebPage()
        {
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><head></head><body><div class=\"not-attachment-details\"><a href=\"/data.csv\">download link</a></div></body></html>")
                });

            var actual = Assert.ThrowsAsync<Exception>(async () => await _dataSource.GetDataAsync(CancellationToken.None));
            Assert.AreEqual("Failed to find element with class attachment-details on RoATP download page", actual.Message);
        }

        [Test]
        public void ThenItShouldThrowExceptionIfCannotFindAnchorInLinkContainerInRoatpWebPage()
        {
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><head></head><body><div class=\"attachment-details\"><div>download link</div></div></body></html>")
                });

            var actual = Assert.ThrowsAsync<Exception>(async () => await _dataSource.GetDataAsync(CancellationToken.None));
            Assert.AreEqual("Link container did not contain anchor element on RoATP download page", actual.Message);
        }

        [Test]
        public void ThenItShouldThrowExceptionIfLinkOnInRoatpWebPageHasNoHref()
        {
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_configuration.RoatpDownloadPageUrl, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><head></head><body><div class=\"attachment-details\"><a>download link</a></div></body></html>")
                });

            var actual = Assert.ThrowsAsync<Exception>(async () => await _dataSource.GetDataAsync(CancellationToken.None));
            Assert.AreEqual("Anchor tag in link container did not have href attribute on RoATP download page", actual.Message);
        }

        [Test]
        public void ThenItShouldThrowExceptionIfCannotOpenCsvDownload()
        {
            var responseContent = "Bad things have happened";
            _httpClientMock
                .When(req => req.RequestUri.AbsoluteUri.StartsWith(_absoluteDownloadUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
                .Then(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(responseContent)
                });

            var actual = Assert.ThrowsAsync<Exception>(async () => await _dataSource.GetDataAsync(CancellationToken.None));
            Assert.AreEqual($"Error trying to download csv from RoATP website. Http status code: 500{Environment.NewLine}{responseContent}", actual.Message);
        }
    }
}