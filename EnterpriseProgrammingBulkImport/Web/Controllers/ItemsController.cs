using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DataAccess.Repositories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Web.Filters;

namespace Web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemsDbRepository _itemsDbRepository;
        private readonly IConfiguration _configuration;

        public ItemsController(ItemsDbRepository itemsDbRepository, IConfiguration configuration)
        {
            _itemsDbRepository = itemsDbRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Catalog(string mode = "restaurants", int? restaurantId = null)
        {
            IEnumerable<IItemValidating> items;

            if (string.Equals(mode, "menuitems", StringComparison.OrdinalIgnoreCase) && restaurantId.HasValue)
            {
                var menuItems = _itemsDbRepository.GetApprovedMenuItemsByRestaurant(restaurantId.Value);
                items = menuItems.Cast<IItemValidating>();
                ViewBag.Mode = "menuitems";
                ViewBag.RestaurantId = restaurantId.Value;
            }
            else
            {
                var restaurants = _itemsDbRepository.GetApprovedRestaurants();
                items = restaurants.Cast<IItemValidating>();
                ViewBag.Mode = "restaurants";
                ViewBag.RestaurantId = null;
            }

            return View(items);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Verification(int? restaurantId = null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Challenge();
            }

            email = email.Trim();
            var siteAdminEmail = _configuration["Approval:SiteAdminEmail"]?.Trim();
            if (!string.IsNullOrWhiteSpace(siteAdminEmail) &&
                string.Equals(email, siteAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                var pendingRestaurants = _itemsDbRepository.GetPendingRestaurants()
                    .Cast<IItemValidating>()
                    .ToList();

                ViewBag.Mode = "restaurants";
                ViewBag.ApproveMode = true;
                ViewBag.ItemType = "restaurant";
                ViewBag.RestaurantId = null;

                return View("Catalog", pendingRestaurants);
            }

            if (!restaurantId.HasValue)
            {
                var owned = _itemsDbRepository.GetOwnedRestaurants(email);
                ViewBag.Owned = true;
                return View("OwnedRestaurants", owned);
            }

            // Step 2: show pending menu items for selected restaurant
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

        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ApprovalFilter))]
        public IActionResult Approve(string itemType, List<int>? restaurantIds, List<Guid>? menuItemIds)
        {
            restaurantIds ??= new List<int>();
            menuItemIds ??= new List<Guid>();

            if (string.Equals(itemType, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                if (restaurantIds.Any())
                {
                    _itemsDbRepository.ApproveRestaurants(restaurantIds);
                }
            }
            else if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase))
            {
                if (menuItemIds.Any())
                {
                    _itemsDbRepository.ApproveMenuItems(menuItemIds);
                }
            }
            if (Request.Form.ContainsKey("restaurantId") &&
                int.TryParse(Request.Form["restaurantId"].ToString(), out var rid) &&
                rid > 0)
            {
                return RedirectToAction(nameof(Verification), new { restaurantId = rid });
            }

            return RedirectToAction(nameof(Verification));
        }
    }
}
