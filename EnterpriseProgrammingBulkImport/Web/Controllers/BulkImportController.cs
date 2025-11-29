using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Factories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using Domain.Models;

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
        [HttpGet]
        public IActionResult DownloadTemplateZip(
        [FromKeyedServices("memory")] IItemsRepository tempRepository)
        {
            var items = tempRepository.GetAll();

            if (items == null || items.Count == 0)
            {
                return BadRequest("No items to generate ZIP for. Please upload JSON first.");
            }

            using var memoryStream = new MemoryStream();

            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in items)
                {
                    string? importId = null;

                    if (item is Restaurant r)
                    {
                        importId = r.ImportId;
                    }
                    else if (item is MenuItem m)
                    {
                        importId = m.ImportId;
                    }

                    if (string.IsNullOrWhiteSpace(importId))
                    {
                        continue;
                    }

                    var folderName = $"item-{importId}";

                    var entryPath = $"{folderName}/default.jpg";
                    var entry = zipArchive.CreateEntry(entryPath);
                }
            }

            memoryStream.Position = 0;
            var fileName = "items-template.zip";

            return File(memoryStream.ToArray(), "application/zip", fileName);
        }
    }
}
