using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using Model;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ToDoWeb.Interface;
using ToDoWeb.Models;

namespace ToDoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataAccessService dataAccessService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IDataAccessService dataAccessService, ILogger<HomeController> logger)
        {
            this.dataAccessService = dataAccessService;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await dataAccessService.GetItemsAsync("SELECT * FROM c"));
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Item item)
        {
            var isValid = TryValidateModel(item);

            item.Id = Guid.NewGuid().ToString();
            await dataAccessService.AddItemAsync(item);
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if ( string.IsNullOrWhiteSpace(id) )
            {
                return BadRequest();
            }

            try
            {
                return View( await dataAccessService.GetItemAsync(id));
            }
            catch(CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            throw new Exception();
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync([Bind("id")]string id)
        {
            await dataAccessService.DeleteItemAsync(id);
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            try
            {
                return View(await dataAccessService.GetItemAsync(id));
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            throw new Exception();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditTask([Bind("Id,Name,Description,IsCompleted")] Item item)
        {
            await dataAccessService.UpdateItemAsync(item.Id, item);
            return Redirect("/");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
