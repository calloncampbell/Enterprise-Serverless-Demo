using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Abstractions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EnterpriseServerless.FunctionApp
{
    public class PostCallCleanupEternalOrchestrator
    {       
        private readonly ICallLoggingService _callLoggingService;
        private readonly IConfigurationRoot _configuration;

        public PostCallCleanupEternalOrchestrator(
            ICallLoggingService callLoggingService,
            IConfigurationRoot configuration)
        {
            _callLoggingService = callLoggingService;
            _configuration = configuration;
        }

        [FunctionName("PostCallCleanupEternalOrchestrator_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orchestrators/{orchestratorName}/{instanceId}")] HttpRequestMessage req,
            [DurableClient(TaskHub = "%PostCallCleanupDurableTaskHubName%")] IDurableOrchestrationClient starter,
            string orchestratorName,
            string instanceId,
            ILogger log)
        {
            try
            {
                // Check if an instance with the specified ID already exists.
                var instance = await starter.GetStatusAsync(instanceId);
                if (instance == null ||
                    !(instance.RuntimeStatus == OrchestrationRuntimeStatus.Pending ||
                    instance.RuntimeStatus == OrchestrationRuntimeStatus.Running ||
                    instance.RuntimeStatus == OrchestrationRuntimeStatus.ContinuedAsNew))
                {
                    // An instance with the specified ID doesn't exist, create one.
                    await starter.StartNewAsync(orchestratorName, instanceId);

                    log.LogInformation($"Started orchestration '{orchestratorName}' with ID = '{instanceId}'.");

                    return starter.CreateCheckStatusResponse(req, instanceId);
                }
                else
                {
                    log.LogInformation($"Orchestration '{orchestratorName}' with ID = '{instanceId}' already exists.");

                    // An instance with the specified ID exists, don't create one.
                    return req.CreateErrorResponse(
                        HttpStatusCode.Conflict,
                        $"An instance with ID '{instanceId}' already exists.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "PostCallCleanupEternalOrchestrator_HttpStart: An error occurred while starting orchestrator");
                return req.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        $"{ex.Message}");
            }
        }

        [FunctionName(nameof(PostCallCleanupEternalOrchestrator))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            try
            {
                //var parallelTasks = new List<Task<int>>();

                // Get a list of N callLog items to process in parallel.
                var workBatch = await context.CallActivityAsync<IList<CallLog>>("PostCallCleanupEternalOrchestrator_GetCallLog", null);

                log.LogInformation($"PostCallCleanupEternalOrchestrator: WorkBatch items to process: {JsonConvert.SerializeObject(workBatch)}");

                var tasks = new Task<CallLog>[workBatch.Count];
                for (int i = 0; i < workBatch.Count; i++)
                {
                    tasks[i] = context.CallActivityAsync<CallLog>("PostCallCleanupEternalOrchestrator_PostCallCleanup", workBatch[i]);
                }
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, $"PostCallCleanupEternalOrchestrator: An error occurred while processing orchestrator. Details: {ex.Message}");
            }
            finally
            {
                // sleep for n minutes before running again
                int minuteInterval = int.Parse(_configuration["PostCallCleanupCheckMinuteInterval"] ?? "5");
                DateTime nextCleanup = context.CurrentUtcDateTime.AddMinutes(minuteInterval);
                await context.CreateTimer(nextCleanup, CancellationToken.None);

                log.LogInformation("Restarting orchestrator");
                context.ContinueAsNew(null);
            }
        }

        [FunctionName("PostCallCleanupEternalOrchestrator_GetCallLog")]
        public async Task<ICollection<CallLog>> GetCallLogAsync(
            [ActivityTrigger] string name,
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting function PostCallCleanupEternalOrchestrator_GetCallLog...");
                return await _callLoggingService.GetCallLogsAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "PostCallCleanupEternalOrchestrator_GetCallLog: An error occurred while getting call logs to process");
            }
            return null;
        }

        [FunctionName("PostCallCleanupEternalOrchestrator_PostCallCleanup")]
        public async Task PostCallCleanupAsync(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Starting function PostCallCleanupEternalOrchestrator_PostCallCleanup...");

                var call = context.GetInput<CallLog>();

                // TODO: Add logic to process the call detail data                
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"PostCallCleanupEternalOrchestrator_PostCallCleanup: An error occurred while processing call log. Details: {ex.Message}");
            }
        }
    }
}