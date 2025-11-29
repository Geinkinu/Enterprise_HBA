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
        [MaxLength(50)]
        public string ImportId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999)]
        public decimal Price { get; set; }

        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(300)]
        public string? ImagePath { get; set; }

        [NotMapped]
        public string? RestaurantImportId { get; set; }

        public List<string> GetValidators()
        {
            if (Restaurant != null && !string.IsNullOrWhiteSpace(Restaurant.OwnerEmailAddress))
                return new List<string> { Restaurant.OwnerEmailAddress };

            return new List<string>();
        }

        public string GetCardPartial()
        {
            return "_MenuItemRow";
        }
    }
}