using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public IActionResult Index()
        {
            return View();
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
