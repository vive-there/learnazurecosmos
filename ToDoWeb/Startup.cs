using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ToDoWeb.Implementation;
using ToDoWeb.Interface;

namespace ToDoWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSingleton<IDataAccessService>(
                InitializeDataAccessServiceAsync(Configuration.GetSection("CosmosDb"))
                .GetAwaiter()
                .GetResult()
                );
        }

        private static async Task<DataAccessService> InitializeDataAccessServiceAsync(IConfigurationSection configurationSection)
        {
            var cosmosClient = new CosmosClient(configurationSection["AccountEndpoint"].ToString(),
                configurationSection["AccountKey"].ToString(),
                new CosmosClientOptions {  
                    SerializerOptions = new CosmosSerializationOptions 
                    { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } 
                });



            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(configurationSection["DatabaseId"].ToString());
            await database.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties {
                Id = configurationSection["ContainerId"].ToString(),
                PartitionKeyPath = configurationSection["PartitionPathKey"].ToString()
                }
                );

            return new DataAccessService(cosmosClient,
                configurationSection["DatabaseId"].ToString(),
                configurationSection["ContainerId"].ToString()
                );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
