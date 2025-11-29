using DataAccess.Contexts;
using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly AppDbContext _dbContext;

        public ItemsDbRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<IItemValidating> GetAll()
        {
            var restaurants = _dbContext.Restaurants.Cast<IItemValidating>().ToList();
            var menuItems = _dbContext.MenuItems.Cast<IItemValidating>().ToList();

            return restaurants.Concat(menuItems).ToList();
        }

        public void Save(IEnumerable<IItemValidating> items)
        {
            var restaurants = items.OfType<Restaurant>().ToList();
            var menuItems = items.OfType<MenuItem>().ToList();

            if (restaurants.Any())
            {
                _dbContext.Restaurants.AddRange(restaurants);
                _dbContext.SaveChanges();
            }
            var restaurantMap = restaurants
                .GroupBy(r => r.ImportId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Id
                );

            foreach (var menuItem in menuItems)
            {
                var importKey = menuItem.RestaurantImportId?.Trim();

                if (!string.IsNullOrWhiteSpace(importKey) &&
                    restaurantMap.TryGetValue(importKey, out var restaurantId))
                {
                    menuItem.RestaurantId = restaurantId;
                    _dbContext.MenuItems.Add(menuItem);
                }
            }

            if (menuItems.Any())
            {
                _dbContext.SaveChanges();
            }
        }

        public void Clear()
        {
            // Nothing to clear for the DB repository
        }

    }
}