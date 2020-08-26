using System;
using EnterpriseServerless.FunctionApp.Abstractions.Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EnterpriseServerless.FunctionApp
{
    public static class ProcessCall
    {
        [FunctionName(nameof(ProcessCall))]
        public static void Run(
            [QueueTrigger("%ProcessCallQueueName%", Connection = Constants.StorageAccount.ConnectionString)]string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"ProcessCall Queue trigger function processed: {myQueueItem}");

            // TODO: Add in logic to process the call details from the queue storage account
        }
    }
}
