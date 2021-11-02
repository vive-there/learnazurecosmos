using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Model;

namespace ToDoHttpFunc
{
    public  class NewTaskFunction
    {

        private CosmosClient cosmosClient;
        public NewTaskFunction(CosmosClient cosmosClient)
        {
            this.cosmosClient = cosmosClient;
        }

        [FunctionName("NewTask")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var item = JsonConvert.DeserializeObject<Item>(requestBody);

            if(item == null)
            {
                return new BadRequestResult();
            }

            var container = this.cosmosClient.GetContainer("Tasks", "Item");
            try
            {
                var result = await container.CreateItemAsync<Item>(item, new PartitionKey(item.Id));
                return new OkObjectResult($"Created {result.Resource.Id}. Charge request {result.RequestCharge}");
            }
            catch(CosmosException cosmosException)
            {
                log.LogError("Creating item failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to create item. Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }

        }
    }
}
