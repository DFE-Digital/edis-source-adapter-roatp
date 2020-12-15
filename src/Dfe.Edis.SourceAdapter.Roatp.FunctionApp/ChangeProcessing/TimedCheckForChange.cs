using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Roatp.Application;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Roatp.FunctionApp.ChangeProcessing
{
    public class TimedCheckForChange
    {
        private readonly IChangeProcessor _changeProcessor;
        private readonly ILogger<TimedCheckForChange> _logger;

        public TimedCheckForChange(
            IChangeProcessor changeProcessor,
            ILogger<TimedCheckForChange> logger)
        {
            _changeProcessor = changeProcessor;
            _logger = logger;
        }

        [FunctionName("TimedCheckForChange")]
        public async Task RunAsync(
            [TimerTrigger("%ChangeTimer%")] TimerInfo timerInfo,
            CancellationToken cancellationToken)
        {
            using (_logger.BeginScope(new Dictionary<string, object>()
            {
                {"TimerStartTime", DateTime.UtcNow},
                {"RequestId", Guid.NewGuid().ToString()},
            }))
            {
                try
                {
                    _logger.LogInformation("Timer executed. Schedule={TimerSchedule}, IsPastDue={IsPastDue}, LastRun={LastRun}, NextRun={NextRun}",
                        timerInfo.Schedule.ToString(),
                        timerInfo.IsPastDue,
                        timerInfo.ScheduleStatus.Last,
                        timerInfo.ScheduleStatus.Next);

                    _logger.LogDebug("Starting change processing...");
                    await _changeProcessor.ProcessChangesAsync(cancellationToken);
                    _logger.LogDebug("Finished change processing...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing change");
                    throw;
                }
            }
        }
    }
}