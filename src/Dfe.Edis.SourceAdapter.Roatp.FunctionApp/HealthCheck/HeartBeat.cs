using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Dfe.Edis.SourceAdapter.Roatp.FunctionApp.HealthCheck
{
    public class HeartBeat
    {
        [FunctionName("HeartBeat")]
        public IActionResult RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            return new OkResult();
        }
    }
}