using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using ToDoHttpFunc;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ToDoHttpFunc
{
    public class Startup : FunctionsStartup
    {
        public static IConfigurationRoot configurationRoot = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", true)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
               .AddEnvironmentVariables()
               .Build();
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((s)=> {
                var cosmoBuilder = new CosmosClientBuilder(configurationRoot.GetSection("ToDoConnectionString").Value);
                return cosmoBuilder
                        .WithSerializerOptions(
                                new CosmosSerializationOptions
                                { 
                                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase 
                                })
                        .Build();
            });
        }
    }
}
