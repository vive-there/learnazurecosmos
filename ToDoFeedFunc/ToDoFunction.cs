using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ToDoFeedFunc
{
    public static class ToDoFunction
    {
        [FunctionName("Feed")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Tasks",
            collectionName: "Item",
            CreateLeaseCollectionIfNotExists = true,
            ConnectionStringSetting = "ToDoConnectionString",
            LeaseCollectionName = "leases"
            )]IReadOnlyList<Document> input, ILogger log)
        {
            
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
                //input[0].
            }
        }
    }
}
