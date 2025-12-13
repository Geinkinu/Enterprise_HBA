using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters
{
    public class ApprovalFilter : IAsyncActionFilter
    {
        private readonly ItemsDbRepository _itemsDbRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public ApprovalFilter(ItemsDbRepository itemsDbRepository, UserManager<IdentityUser> userManager)
        {
            _itemsDbRepository = itemsDbRepository;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var principal = context.HttpContext.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                context.Result = new ForbidResult();
                return;
            }

            var identityUser = await _userManager.GetUserAsync(principal);
            var email = (identityUser?.Email ?? identityUser?.UserName)?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                context.Result = new ForbidResult();
                return;
            }

            var itemType = context.ActionArguments.TryGetValue("itemType", out var itemTypeObj)
                ? itemTypeObj?.ToString() ?? ""
                : "";

            var restaurantIds = context.ActionArguments.TryGetValue("restaurantIds", out var restObj)
                ? (restObj as List<int>) ?? new List<int>()
                : new List<int>();

            var menuItemIds = context.ActionArguments.TryGetValue("menuItemIds", out var menuObj)
                ? (menuObj as List<Guid>) ?? new List<Guid>()
                : new List<Guid>();

            var itemsToCheck = new List<IItemValidating>();

            if (string.Equals(itemType, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                if (!restaurantIds.Any())
                {
                    context.Result = new ForbidResult();
                    return;
                }

                var pending = _itemsDbRepository.GetPendingRestaurants()
                    .Where(r => restaurantIds.Contains(r.Id))
                    .Cast<IItemValidating>();

                itemsToCheck.AddRange(pending);
            }
            else if (string.Equals(itemType, "menuitem", StringComparison.OrdinalIgnoreCase))
            {
                if (!menuItemIds.Any())
                {
                    context.Result = new ForbidResult();
                    return;
                }

                var pendingMenuItems = _itemsDbRepository.GetPendingMenuItemsByIds(menuItemIds)
                    .Cast<IItemValidating>();

                itemsToCheck.AddRange(pendingMenuItems);
            }
            else
            {
                context.Result = new ForbidResult();
                return;
            }

            foreach (var item in itemsToCheck)
            {
                var validators = item.GetValidators() ?? new List<string>();

                var isAllowed = validators.Any(v =>
                    string.Equals(v?.Trim(), email, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
