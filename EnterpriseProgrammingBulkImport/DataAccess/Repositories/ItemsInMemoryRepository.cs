using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "BulkImportItems";

        public ItemsInMemoryRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IReadOnlyList<IItemValidating> GetAll()
        {
            if (_cache.TryGetValue(CacheKey, out List<IItemValidating>? items) && items != null)
            {
                return items;
            }

            return new List<IItemValidating>();
        }

        public void Save(IEnumerable<IItemValidating> items)
        {
            var list = new List<IItemValidating>(items);
            _cache.Set(CacheKey, list);
        }
        public void Clear()
        {
            _cache.Remove(CacheKey);
        }
    }
}