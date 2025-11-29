using System.Collections.Generic;
using System.Linq;
using DataAccess.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemsDbRepository _itemsDbRepository;

        public ItemsController(ItemsDbRepository itemsDbRepository)
        {
            _itemsDbRepository = itemsDbRepository;
        }

        [HttpGet]
        public IActionResult Catalog(string mode = "restaurants", int? restaurantId = null)
        {
            IEnumerable<IItemValidating> items;

            if (mode == "menuitems" && restaurantId.HasValue)
            {
                var menuItems = _itemsDbRepository.GetApprovedMenuItemsByRestaurant(restaurantId.Value);
                items = menuItems.Cast<IItemValidating>();
            }
            else
            {
                var restaurants = _itemsDbRepository.GetApprovedRestaurants();
                items = restaurants.Cast<IItemValidating>();
                mode = "restaurants";
            }

            ViewBag.Mode = mode;
            ViewBag.RestaurantId = restaurantId;

            return View(items);
        }
    }
}
