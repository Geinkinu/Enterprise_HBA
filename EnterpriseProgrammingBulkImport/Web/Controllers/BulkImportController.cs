using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Factories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ImportItemFactory _factory;

        public BulkImportController(ImportItemFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public IActionResult BulkImport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkImport(
            IFormFile jsonFile,
            [FromKeyedServices("memory")] IItemsRepository tempRepository)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a JSON file.");
                return View();
            }

            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = await reader.ReadToEndAsync();
            }

            var items = _factory.Create(json);

            tempRepository.Save(items);
            return View("Preview", items);
        }
    }
}
