using System.Threading;
using System.Threading.Tasks;
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
        private readonly ILogger<ChangeProcessor> _logger;

        public ChangeProcessor(
            IRoatpDataSource roatpDataSource,
            ILogger<ChangeProcessor> logger)
        {
            _roatpDataSource = roatpDataSource;
            _logger = logger;
        }
        
        public async Task ProcessChangesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting data from RoATP");
            var thing = await _roatpDataSource.GetDataAsync(cancellationToken);

            // TODO: Publish changes
        }
    }
}