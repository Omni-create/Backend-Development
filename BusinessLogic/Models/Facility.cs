using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class Facility
{
    public int FacilityId { get; set; }
[Required]
    public string FacilityName { get; set; } = null!;
[Required]
[Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
}
