using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.RestProxy;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Times = MockTheWeb.Times;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.UnitTests.RestProxyTests.KafkaRestProxyRoatpDataReceiverTests
{
    public class WhenSendingDataToRestProxy
    {
        private HttpClientMock _httpClientMock;
        private DataServicePlatformConfiguration _configuration;
        private Mock<ILogger<KafkaRestProxyRoatpDataReceiver>> _loggerMock;
        private KafkaRestProxyRoatpDataReceiver _receiver;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _httpClientMock = new HttpClientMock();
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Json(
                    new RestProxyPublishMessageResponse
                    {
                        Offsets = new[]
                        {
                            new RestProxyResponseOffset
                            {
                                Partition = 0,
                                Offset = 1,
                            },
                        },
                    }));

            _configuration = new DataServicePlatformConfiguration
            {
                KafkaRestProxyUrl = "https://localhost:9876",
                RoatpProviderTopic = "some-topic",
            };

            _loggerMock = new Mock<ILogger<KafkaRestProxyRoatpDataReceiver>>();

            _receiver = new KafkaRestProxyRoatpDataReceiver(
                _httpClientMock.AsHttpClient(),
                _configuration,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test, AutoData]
        public async Task ThenItShouldSendProviderToRestProxyTopic(ApprenticeshipProvider provider)
        {
            await _receiver.SendDataAsync(provider, _cancellationToken);

            var expectedUrl = new Uri(
                new Uri(_configuration.KafkaRestProxyUrl, UriKind.Absolute),
                new Uri($"topics/{_configuration.RoatpProviderTopic}", UriKind.Relative));
            _httpClientMock.Verify(request => request.RequestUri.AbsoluteUri == expectedUrl.AbsoluteUri,
                Times.Once());

            var expectedContent = JsonConvert.SerializeObject(new RestProxyPublishMessage<long, ApprenticeshipProvider>
            {
                Records = new[]
                {
                    new RestProxyPublishMessageRecord<long, ApprenticeshipProvider>
                    {
                        Key = provider.Ukprn,
                        Value = provider,
                    },
                },
            });
            _httpClientMock.Verify(request => request.Content.ReadAsStringAsync().Result == expectedContent,
                Times.Once());
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheResponseIsNotASuccessCode()
        {
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Response().WithStatus(HttpStatusCode.InternalServerError));

            var actual = Assert.ThrowsAsync<RestProxyException>(async () =>
                await _receiver.SendDataAsync(new ApprenticeshipProvider(), _cancellationToken));
            Assert.AreEqual(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.AreEqual($"Error posting message to {_configuration.RoatpProviderTopic}, http status 500 returned.", actual.Message);
        }

        [TestCase("code001", "error details")]
        [TestCase(null, "error details")]
        [TestCase("code001", null)]
        public void ThenItShouldThrowAnExceptionIfTheOffsetContainsError(string errorCode, string error)
        {
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Json(
                    new RestProxyPublishMessageResponse
                    {
                        Offsets = new[]
                        {
                            new RestProxyResponseOffset
                            {
                                Partition = 0,
                                Offset = 1,
                                Error = error,
                                ErrorCode = errorCode,
                            },
                        },
                    }, new MockTheWebNewtonsoftSerializer()));

            var actual = Assert.ThrowsAsync<RestProxyException>(async () =>
                await _receiver.SendDataAsync(new ApprenticeshipProvider(), _cancellationToken));
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual($"Offset reports an error. Partition=0, Offset=1, Code={errorCode}" +
                            $"{Environment.NewLine}{error}", actual.Message);
        }
    }
}