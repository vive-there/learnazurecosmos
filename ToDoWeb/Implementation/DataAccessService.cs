using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ToDoWeb.Interface;
using ToDoWeb.Models;
using Microsoft.Azure.Cosmos;

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

        public Task DeleteItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Item>> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemAsync(string id, Item item)
        {
            throw new NotImplementedException();
        }
    }
}
