using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;

namespace Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform
{
    public interface IRoatpDataReceiver
    {
        Task SendDataAsync(ApprenticeshipProvider provider, CancellationToken cancellationToken);
    }
}