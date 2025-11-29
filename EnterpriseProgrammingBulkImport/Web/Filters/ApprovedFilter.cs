using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters
{
    public class ApprovalFilter : IAsyncActionFilter
    {
        private readonly ItemsDbRepository _itemsDbRepository;

        public ApprovalFilter(ItemsDbRepository itemsDbRepository)
        {
            _itemsDbRepository = itemsDbRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpUser = context.HttpContext.User;

            if (!httpUser.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ForbidResult();
                return;
            }

            var email = httpUser.FindFirstValue(ClaimTypes.Email) ?? httpUser.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                context.Result = new ForbidResult();
                return;
            }
            var itemType = context.ActionArguments.ContainsKey("itemType")
                ? context.ActionArguments["itemType"]?.ToString()
                : "";

            var restaurantIds = context.ActionArguments.ContainsKey("restaurantIds")
                ? context.ActionArguments["restaurantIds"] as List<int>
                : new List<int>();

            var menuItemIds = context.ActionArguments.ContainsKey("menuItemIds")
                ? context.ActionArguments["menuItemIds"] as List<Guid>
                : new List<Guid>();

            var itemsToCheck = new List<IItemValidating>();

            if (string.Equals(itemType, "restaurant", StringComparison.OrdinalIgnoreCase) && restaurantIds.Any())
            {
                var pending = _itemsDbRepository.GetPendingRestaurants()
                    .Where(r => restaurantIds.Contains(r.Id))
                    .Cast<IItemValidating>();
                itemsToCheck.AddRange(pending);
            }
            else if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase) && menuItemIds.Any())
            {
                var allPendingForAll = _itemsDbRepository.GetPendingMenuItemsByRestaurant(0);
            }

            if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase) && menuItemIds.Any())
            {
                var items = _itemsDbRepository
                    .GetApprovedMenuItemsByRestaurant(0);

                var query = _itemsDbRepository
                    .GetPendingMenuItemsByRestaurant;
            }

            if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase) && menuItemIds.Any())
            {
                var allPendingMenu = _itemsDbRepository.GetApprovedMenuItemsByRestaurant(0);

                context.Result = new ForbidResult();
                return;
            }

            if (string.Equals(itemType, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                var pending = _itemsDbRepository.GetPendingRestaurants()
                    .Where(r => restaurantIds.Contains(r.Id))
                    .Cast<IItemValidating>();

                itemsToCheck.AddRange(pending);
            }

            foreach (var item in itemsToCheck)
            {
                var validators = item.GetValidators() ?? new List<string>();

                if (!validators.Any(v => string.Equals(v, email, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
