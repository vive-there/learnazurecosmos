using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ToDoWeb.Interface;
using ToDoWeb.Models;
using Microsoft.Azure.Cosmos;
using Model;

namespace ToDoWeb.Implementation
{
    public class DataAccessService : IDataAccessService
    {
        private readonly Container _container;
        public DataAccessService(CosmosClient cosmosClient,string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
        }
        public async Task AddItemAsync(Item item)
        {
            await _container.CreateItemAsync<Item>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<Item>(id, new PartitionKey(id));
        }

        public async Task<Item> GetItemAsync(string id)
        {
            ItemResponse<Item> itemResponse = await _container.ReadItemAsync<Item>(id, new PartitionKey(id));
            return itemResponse;
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(string query)
        {
            List<Item> items = new List<Item>();

            var queryIterator = _container.GetItemQueryIterator<Item>(
                new QueryDefinition(query)
                );

            while(queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                items.AddRange(response.ToList());
            }    

            return items;
        }

        public async Task UpdateItemAsync(string id, Item item)
        {
            await this._container.UpsertItemAsync<Item>(item, new PartitionKey(id));
        }
    }
}
