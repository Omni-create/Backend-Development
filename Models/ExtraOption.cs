using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HotelApi.Models;

public partial class ExtraOption
{
    public int ExtraOptionId { get; set; }
[Required]
    public string OptionName { get; set; } = null!;
[Required]
[Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
}
