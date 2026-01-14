using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("ExtraOption")]
    public class ExtraOption
    {
        [Key]
        [Column("extraOptionID")]
        public int ExtraOptionId { get; set; }

        [Required]
        [StringLength(50)]
        public string OptionName { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Optionele navigatie-eigenschap:
        public ICollection<ReservedExtraOption> ReservedExtraOptions { get; set; } = new List<ReservedExtraOption>();
    }
}
