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
        IReadOnlyList<IItemValidating> GetAll();

        void Save(IEnumerable<IItemValidating> items);
    }
}