using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend_Dev.Transfer;

public class CreateReservationDto
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public DateOnly StartDate { get; set; }
    
    [Required]
    public DateOnly EndDate { get; set; }
    
    public List<int> RoomIds { get; set; } = new();
    
    public List<int> ExtraOptionIds { get; set; } = new();
    
    public List<int> FacilityIds { get; set; } = new();
}