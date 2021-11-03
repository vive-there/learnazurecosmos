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
    }
}
