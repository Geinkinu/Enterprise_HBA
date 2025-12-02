using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Domain.Factories;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
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

                    zipArchive.CreateEntry(entryPath);
                }
            }

            memoryStream.Position = 0;
            const string fileName = "items-template.zip";

            return File(memoryStream.ToArray(), "application/zip", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Commit(
            IFormFile imagesZip,
            [FromKeyedServices("memory")] IItemsRepository tempRepository,
            [FromKeyedServices("db")] IItemsRepository dbRepository,
            [FromServices] IWebHostEnvironment env)
        {
            if (imagesZip == null || imagesZip.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a ZIP file with images.");
                return BadRequest(ModelState);
            }

            var items = tempRepository.GetAll();
            if (items == null || items.Count == 0)
            {
                return BadRequest("No items in memory. Please perform bulk import first.");
            }

            var imagesRoot = Path.Combine(env.WebRootPath, "images", "items");
            Directory.CreateDirectory(imagesRoot);

            using (var zipStream = imagesZip.OpenReadStream())
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName) ||
                        !entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var parts = entry.FullName.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        continue;

                    var folderSegment = parts.FirstOrDefault(p =>
                        p.StartsWith("item-", StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrEmpty(folderSegment))
                        continue;

                    var importId = folderSegment.Substring("item-".Length);

                    var item = items.FirstOrDefault(i =>
                        (i is Restaurant r && r.ImportId == importId) ||
                        (i is MenuItem m && m.ImportId == importId));

                    if (item == null)
                        continue;

                    var uniqueFileName = $"{Guid.NewGuid():N}.jpg";
                    var filePath = Path.Combine(imagesRoot, uniqueFileName);

                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.Create(filePath))
                    {
                        await entryStream.CopyToAsync(fileStream);
                    }

                    var relativePath = $"/images/items/{uniqueFileName}";

                    if (item is Restaurant rItem)
                    {
                        rItem.ImagePath = relativePath;
                    }
                    else if (item is MenuItem mItem)
                    {
                        mItem.ImagePath = relativePath;
                    }
                }
            }

            dbRepository.Save(items);
            tempRepository.Clear();

            return RedirectToAction("BulkImport", "BulkImport");
        }
    }
}
