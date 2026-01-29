using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace GiteApi.Models;

public partial class ReservedExtraOption
{
    [Required]
    public int ExtraOptionId { get; set; }
    [Required]
    public int ReservationId { get; set; }

    public virtual ExtraOption ExtraOption { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
