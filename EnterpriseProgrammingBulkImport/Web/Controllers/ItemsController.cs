using System.Collections.Generic;
using System.Linq;
using DataAccess.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Web.Filters;


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

        [Authorize]
        public IActionResult Verification(int? restaurantId = null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                return Challenge();
            }

            const string siteAdminEmail = "siteadmin@example.com";

            if (string.Equals(email, siteAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                var pendingRestaurants = _itemsDbRepository.GetPendingRestaurants()
                    .Cast<IItemValidating>()
                    .ToList();

                ViewBag.Mode = "restaurants";
                ViewBag.ApproveMode = true;
                ViewBag.ItemType = "restaurant";

                return View("Catalog", pendingRestaurants);
            }
            else
            {
                if (!restaurantId.HasValue)
                {
                    var owned = _itemsDbRepository.GetOwnedRestaurants(email);
                    ViewBag.Owned = true;
                    return View("OwnedRestaurants", owned);
                }
                else
                {
                    var pendingMenu = _itemsDbRepository
                        .GetPendingMenuItemsByRestaurant(restaurantId.Value)
                        .Cast<IItemValidating>()
                        .ToList();

                    ViewBag.Mode = "menuitems";
                    ViewBag.ApproveMode = true;
                    ViewBag.ItemType = "menuitem";
                    ViewBag.RestaurantId = restaurantId.Value;

                    return View("Catalog", pendingMenu);
                }
            }
        }
        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ApprovalFilter))]
        public IActionResult Approve(string itemType, List<int>? restaurantIds, List<Guid>? menuItemIds)
        {
            restaurantIds ??= new List<int>();
            menuItemIds ??= new List<Guid>();

            if (string.Equals(itemType, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                _itemsDbRepository.ApproveRestaurants(restaurantIds);
            }
            else if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase))
            {
                _itemsDbRepository.ApproveMenuItems(menuItemIds);
            }

            return RedirectToAction(nameof(Verification));
        }
    }
}
