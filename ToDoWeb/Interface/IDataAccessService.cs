using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ToDoWeb.Models;

namespace ToDoWeb.Interface
{
    public interface IDataAccessService
    {
        Task<IEnumerable<Item>> GetItemsAsync(string query);
        Task<Item> GetItemAsync(string id);
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(string id, Item item);
        Task DeleteItemAsync(string id);
    }
}
