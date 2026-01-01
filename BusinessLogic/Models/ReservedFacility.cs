using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class ReservedFacility
{
    [Required]
    public int FacilityId { get; set; }
    [Required]
    public int ReservationId { get; set; }

    public virtual Facility Facility { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
