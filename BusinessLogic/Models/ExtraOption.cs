using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class ExtraOption
{
    public int ExtraOptionId { get; set; }

    public string OptionName { get; set; } = null!;

    public decimal Price { get; set; }
}
