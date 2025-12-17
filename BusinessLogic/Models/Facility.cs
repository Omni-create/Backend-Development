using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string FacilityName { get; set; } = null!;

    public decimal Price { get; set; }
}
