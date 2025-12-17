using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class ReservedExtraOption
{
    public int ExtraOptionId { get; set; }

    public int ReservationId { get; set; }

    public virtual ExtraOption ExtraOption { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
