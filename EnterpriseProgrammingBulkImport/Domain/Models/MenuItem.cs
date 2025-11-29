using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999)]
        public decimal Price { get; set; }

        // Foreign key to Restaurant
        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        // --- IItemValidating implementation (logic later) ---

        public List<string> GetValidators()
        {
            // TODO: later this will return the restaurant owner's email.
            // We'll use Restaurant.OwnerEmailAddress when available.
            throw new NotImplementedException();
        }

        public string GetCardPartial()
        {
            // TODO: later we’ll return the partial view name, e.g. "_MenuItemRow"
            throw new NotImplementedException();
        }
    }
}