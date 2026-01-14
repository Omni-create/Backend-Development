using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("Facility")]
    public class Facility
    {
        [Key]
        [Column("facilityID")]
        public int FacilityId { get; set; }

        [Required]
        [StringLength(50)]
        public string FacilityName { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Optionele navigatie-eigenschap:
        public ICollection<ReservedFacility> ReservedFacilities { get; set; } = new List<ReservedFacility>();
    }
}
