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
            foreach (var item in items)
            {
                switch (item)
                {
                    case Restaurant restaurant:
                        _dbContext.Restaurants.Add(restaurant);
                        break;

                    case MenuItem menuItem:
                        _dbContext.MenuItems.Add(menuItem);
                        break;

                    default:
                        break;
                }
            }

            _dbContext.SaveChanges();
        }
    }
}