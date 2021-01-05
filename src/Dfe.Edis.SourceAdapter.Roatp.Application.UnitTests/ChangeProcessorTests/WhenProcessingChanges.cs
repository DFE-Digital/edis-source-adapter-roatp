using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Roatp.Application.UnitTests.ChangeProcessorTests
{
    public class WhenProcessingChanges
    {
        private Mock<IRoatpDataSource> _roatpDataSourceMock;
        private Mock<IRoatpDataReceiver> _roatpDataReceiverMock;
        private Mock<ILogger<ChangeProcessor>> _loggerMock;
        private ChangeProcessor _changeProcessor;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _roatpDataSourceMock = new Mock<IRoatpDataSource>();
            _roatpDataSourceMock.Setup(s => s.GetDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApprenticeshipProvider[0]);

            _roatpDataReceiverMock = new Mock<IRoatpDataReceiver>();

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _changeProcessor = new ChangeProcessor(
                _roatpDataSourceMock.Object,
                _roatpDataReceiverMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldDownloadProvidersFromDataSource()
        {
            await _changeProcessor.ProcessChangesAsync(_cancellationToken);
            
            _roatpDataSourceMock.Verify(source => source.GetDataAsync(_cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldSendEachProviderReceivedFromSourceToReceiver()
        {
            var apprenticeshipProvider1 = new ApprenticeshipProvider();
            var apprenticeshipProvider2 = new ApprenticeshipProvider();
            _roatpDataSourceMock.Setup(source => source.GetDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    apprenticeshipProvider1,
                    apprenticeshipProvider2,
                });
            
            await _changeProcessor.ProcessChangesAsync(_cancellationToken);
            
            _roatpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(It.IsAny<ApprenticeshipProvider>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _roatpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(apprenticeshipProvider1, _cancellationToken),
                Times.Once);
            _roatpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(apprenticeshipProvider2, _cancellationToken),
                Times.Once);
        }
    }
}