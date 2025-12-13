using DataAccess.Contexts;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;


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

        public List<Restaurant> GetApprovedRestaurants()
        {
            return _dbContext.Restaurants
                .Where(r => r.Status == "Approved")
                .ToList();
        }

        public List<MenuItem> GetApprovedMenuItemsByRestaurant(int restaurantId)
        {
            return _dbContext.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => m.Status == "Approved" && m.RestaurantId == restaurantId)
                .ToList();
        }
        public List<Restaurant> GetPendingRestaurants()
        {
            return _dbContext.Restaurants
                .Where(r => r.Status == "Pending")
                .ToList();
        }

        public List<Restaurant> GetOwnedRestaurants(string ownerEmail)
        {
            return _dbContext.Restaurants
                .Where(r => r.OwnerEmailAddress == ownerEmail)
                .ToList();
        }

        public List<MenuItem> GetPendingMenuItemsByRestaurant(int restaurantId)
        {
            return _dbContext.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => m.RestaurantId == restaurantId && m.Status == "Pending")
                .ToList();
        }

        public void ApproveRestaurants(IEnumerable<int> restaurantIds)
        {
            var restaurants = _dbContext.Restaurants
                .Where(r => restaurantIds.Contains(r.Id))
                .ToList();

            foreach (var r in restaurants)
            {
                r.Status = "Approved";
            }

            _dbContext.SaveChanges();
        }

        public void ApproveMenuItems(IEnumerable<Guid> menuItemIds)
        {
            var items = _dbContext.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .ToList();

            foreach (var m in items)
            {
                m.Status = "Approved";
            }

            _dbContext.SaveChanges();
        }

        public List<MenuItem> GetPendingMenuItemsByIds(IEnumerable<Guid> menuItemIds)
        {
            var ids = menuItemIds?.ToList() ?? new List<Guid>();

            return _dbContext.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => m.Status == "Pending" && ids.Contains(m.Id))
                .ToList();
        }

        public void Approve(IEnumerable<int> restaurantIds)
        {
            ApproveRestaurants(restaurantIds);
        }

        public void Clear()
        {
        }

    }
}