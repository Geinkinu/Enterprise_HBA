using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Factories
{
    public class ImportItemFactory
    {
        public List<IItemValidating> Create(string json)
        {
            var dtos = JsonSerializer.Deserialize<List<ImportItemDto>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ImportItemDto>();

            var items = new List<IItemValidating>();

            foreach (var dto in dtos)
            {
                if (string.Equals(dto.Type, "restaurant", System.StringComparison.OrdinalIgnoreCase))
                {
                    var restaurant = new Restaurant
                    {
                        ImportId = dto.Id ?? string.Empty,
                        Name = dto.Name ?? string.Empty,
                        OwnerEmailAddress = dto.OwnerEmailAddress ?? string.Empty,
                        Status = "Pending"
                    };

                    items.Add(restaurant);
                }
                else if (string.Equals(dto.Type, "menuItem", System.StringComparison.OrdinalIgnoreCase))
                {
                    var menuItem = new MenuItem
                    {
                        ImportId = dto.Id ?? string.Empty,
                        Title = dto.Title ?? string.Empty,
                        Price = dto.Price ?? 0m,
                        Status = "Pending"
                    };
                    items.Add(menuItem);
                }
            }

            return items;
        }

        private class ImportItemDto
        {
            public string? Type { get; set; }
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? OwnerEmailAddress { get; set; }

            public string? Title { get; set; }
            public decimal? Price { get; set; }
            public string? RestaurantId { get; set; }
        }
    }
}