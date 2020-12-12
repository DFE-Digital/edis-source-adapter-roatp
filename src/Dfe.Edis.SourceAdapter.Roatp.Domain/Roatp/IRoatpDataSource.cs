using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp
{
    public interface IRoatpDataSource
    {
        Task<ApprenticeshipProvider[]> GetDataAsync(CancellationToken cancellationToken);
    }
}