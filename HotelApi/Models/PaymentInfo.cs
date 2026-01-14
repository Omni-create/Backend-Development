using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("PaymentInfo")]
    public class PaymentInfo
    {
        [Key]
        [Column("paymentInfoID")]
        public int PaymentInfoId { get; set; }

        [Required]
        [Column("userID")]
        public int UserId { get; set; }

        [StringLength(4)]
        public string? LastFourDigits { get; set; }

        [StringLength(50)]
        public string? BankHolderName { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = null!;

        [StringLength(255)]
        public string? PaymentToken { get; set; }

        // Navigatie-eigenschappen
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
