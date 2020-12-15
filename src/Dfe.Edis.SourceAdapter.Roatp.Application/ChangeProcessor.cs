using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Roatp.Application
{
    public interface IChangeProcessor
    {
        Task ProcessChangesAsync(CancellationToken cancellationToken);
    }

    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IRoatpDataSource _roatpDataSource;
        private readonly IRoatpDataReceiver _roatpDataReceiver;
        private readonly ILogger<ChangeProcessor> _logger;

        public ChangeProcessor(
            IRoatpDataSource roatpDataSource,
            IRoatpDataReceiver roatpDataReceiver,
            ILogger<ChangeProcessor> logger)
        {
            _roatpDataSource = roatpDataSource;
            _roatpDataReceiver = roatpDataReceiver;
            _logger = logger;
        }
        
        public async Task ProcessChangesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting data from RoATP");
            var providers = await _roatpDataSource.GetDataAsync(cancellationToken);
            if (providers == null || providers.Length == 0)
            {
                _logger.LogInformation("Data source did not return an providers. Assuming no changes.");
                return;
            }

            foreach (var provider in providers)
            {
                await _roatpDataReceiver.SendDataAsync(provider, cancellationToken);
            }
        }
    }
}