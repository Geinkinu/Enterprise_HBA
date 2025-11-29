using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Interfaces;

namespace Domain.Interfaces
{
    public interface IItemsRepository
    {
        /// <summary>
        /// Returns all items currently held by this repository.
        /// For the in-memory repository this will be the items from the last bulk import.
        /// For the DB repository this can be refined later if needed.
        /// </summary>
        IReadOnlyList<IItemValidating> GetAll();

        /// <summary>
        /// Saves the given items.
        /// In-memory: stores a copy in cache.
        /// DB: persists restaurants + menu items.
        /// </summary>
        void Save(IEnumerable<IItemValidating> items);
    }
}