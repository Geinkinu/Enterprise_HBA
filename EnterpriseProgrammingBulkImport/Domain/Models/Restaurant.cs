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
    public class Restaurant : IItemValidating
    {  
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ImportId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string OwnerEmailAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(300)]
        public string? ImagePath { get; set; }

        public ICollection<MenuItem>? MenuItems { get; set; }

        // TODO

        public List<string> GetValidators()
        {
            return new List<string>
        {
            "siteadmin@example.com"
        };
        }

        public string GetCardPartial()
        {
            return "_RestaurantCard";
        }

    }
}