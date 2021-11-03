using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ContainerManager
{
    class Program
    {
        static string CosmosDbConnectionStringSectionName = "ToDoConnectionString";
        static string DatabaseId = "sampledatabase123";
        static string ContainerId = "sample-container";
        static string AutoscaleContainerId = "sample-autoscale-container";
        static string PartitionKeyPath = "/activeId";
        static int DefaultThroughput = 400;
        static int AutoscaleMaxThroughput = 10000;

        static Database _database = null;

        static IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync("Start the demo.");

            try
            {
                var connectionString = configurationRoot.GetSection(CosmosDbConnectionStringSectionName).Value;

                using (var cosmosClient = 
                    new CosmosClient(connectionString,
                                                           new CosmosClientOptions
                                                           {
                                                               SerializerOptions = new CosmosSerializationOptions
                                                               {
                                                                   PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                                                               }
                                                           }))
                {
                    await Program.RunDemoAsync(cosmosClient);
                }
            }
            catch (CosmosException ce)
            {
                await Console.Out.WriteLineAsync(ce.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                await Console.Out.WriteLineAsync($"Error: {e.Message}, Message: {baseException.Message}");
            }
            finally 
            {
                await Console.Out.WriteLineAsync("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        static async Task RunDemoAsync(CosmosClient cosmosClient)
        {
            await Program.SetupDatabaseAsync(cosmosClient);

            var simpleContainer = await CreateSimpleContainerAsync();
            await GetAndChangeContainerPerformance(simpleContainer);

            await CreateAndUpdateAutoscaleContainer();

            await Program.CreateContainerWithCustomIndexingPolicy();

            await ListContainersInDatabase();

        }

        static async Task SetupDatabaseAsync(CosmosClient cosmosClient)
        {
            _database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            await Console.Out.WriteLineAsync($"{Environment.NewLine}1.0 Database {DatabaseId} has been set successfuly.");
        }

        static async Task<Container> CreateSimpleContainerAsync()
        {
            var containerProperties = new ContainerProperties(ContainerId, PartitionKeyPath);
             
            var containerResponse = await _database.CreateContainerIfNotExistsAsync(
                                                    containerProperties, 
                                                    throughput: DefaultThroughput);

            await Console.Out.WriteLineAsync($"{Environment.NewLine}1.1 Simple container {ContainerId} has been created.");

            return containerResponse;
        }

        static async Task CreateAndUpdateAutoscaleContainer()
        {
            var containerProperties = new ContainerProperties(AutoscaleContainerId, PartitionKeyPath);

            Container autoscaleContainer = await _database.CreateContainerIfNotExistsAsync(
                 containerProperties,
                 ThroughputProperties.CreateAutoscaleThroughput(AutoscaleMaxThroughput)
                );

            Console.WriteLine($"{Environment.NewLine}1.2. Created autoscale container :{autoscaleContainer.Id}");

            //*********************************************************************************************
            // Get configured performance of a CosmosContainer
            //**********************************************************************************************
            ThroughputResponse throughputResponse = await autoscaleContainer.ReadThroughputAsync(requestOptions: null);

            Console.WriteLine($"{Environment.NewLine}1.2.1. Found autoscale throughput {Environment.NewLine}The current throughput: {throughputResponse.Resource.Throughput} Max throughput: {throughputResponse.Resource.AutoscaleMaxThroughput} " +
                $"using container's id: {autoscaleContainer.Id}");

            //Get current throughput
            int? currentThroughput = await autoscaleContainer.ReadThroughputAsync();
            Console.WriteLine($"{Environment.NewLine}1.2.2. Found autoscale throughput {Environment.NewLine}The current throughput: {currentThroughput} using container's id: {autoscaleContainer.Id}");

            //******************************************************************************************************************
            // Change performance (reserved throughput) of CosmosContainer
            //    Let's change the performance of the autoscale container to a maximum throughput of 15000 RU/s
            //******************************************************************************************************************
            ThroughputResponse throughputUpdateResponse = await autoscaleContainer.ReplaceThroughputAsync(ThroughputProperties.CreateAutoscaleThroughput(15000));

            Console.WriteLine($"{Environment.NewLine}1.2.3. Replaced autoscale throughput. {Environment.NewLine}The current throughput: {throughputUpdateResponse.Resource.Throughput} Max throughput: {throughputUpdateResponse.Resource.AutoscaleMaxThroughput} " +
                $"using container's id: {autoscaleContainer.Id}");

            // Get the offer again after replace
            throughputResponse = await autoscaleContainer.ReadThroughputAsync(requestOptions: null);

            Console.WriteLine($"{Environment.NewLine}1.2.4. Found autoscale throughput {Environment.NewLine}The current throughput: {throughputResponse.Resource.Throughput} Max throughput: {throughputResponse.Resource.AutoscaleMaxThroughput} " +
                $"using container's id: {autoscaleContainer.Id}{Environment.NewLine}");

            // Delete the container
            await autoscaleContainer.DeleteContainerAsync();
        }

        static async Task CreateContainerWithCustomIndexingPolicy()
        {
            var containerProperties = new ContainerProperties { 
                Id = "cont1",
                PartitionKeyPath = PartitionKeyPath,
                IndexingPolicy = new IndexingPolicy() { IndexingMode = IndexingMode.Consistent }
            };

            Container container = await _database.CreateContainerAsync(containerProperties, DefaultThroughput);
            Console.WriteLine($"1.3 Created Container {container.Id}, with custom index policy {Environment.NewLine}");
            await container.DeleteContainerAsync();
        }

        private static async Task GetAndChangeContainerPerformance(Container simpleContainer)
        {

            //*********************************************************************************************
            // Get configured performance (reserved throughput) of a CosmosContainer
            //**********************************************************************************************
            int? throughputResponse = await simpleContainer.ReadThroughputAsync();

            Console.WriteLine($"{Environment.NewLine}2. Found throughput {Environment.NewLine}{throughputResponse}{Environment.NewLine}using container's id {Environment.NewLine}{simpleContainer.Id}");

            //******************************************************************************************************************
            // Change performance (reserved throughput) of CosmosContainer
            //    Let's change the performance of the container to 500 RU/s
            //******************************************************************************************************************

            await simpleContainer.ReplaceThroughputAsync(500);

            Console.WriteLine($"{Environment.NewLine}3. Replaced throughput. Throughput is now 500.{Environment.NewLine}");

            // Get the offer again after replace
            throughputResponse = await simpleContainer.ReadThroughputAsync();

            Console.WriteLine($"3. Found throughput {Environment.NewLine}{throughputResponse}{Environment.NewLine} using container's ResourceId {simpleContainer.Id}.{Environment.NewLine}");
        }

        static async Task ListContainersInDatabase()
        {
            using var queryIterator = _database.GetContainerQueryIterator<ContainerProperties>(); 
            while(queryIterator.HasMoreResults)
            {
                foreach(var properties in await queryIterator.ReadNextAsync())
                {
                    Console.WriteLine(properties.Id);
                }

            }    


        }

    }
}
