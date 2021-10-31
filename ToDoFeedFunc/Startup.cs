using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using ToDoFeedFunc;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ToDoFeedFunc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
                   // local dev no Key Vault
                builder.ConfigurationBuilder
                   .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("local.settings.json", true)
                   .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                   .AddEnvironmentVariables()
                   .Build();
            
        }
    }
}
