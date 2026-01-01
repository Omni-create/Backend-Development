using System.Collections.Generic;

namespace Presentation.Transfer
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = null!;
        public ICollection<int> RoomIds { get; set; } = new List<int>();
        public ICollection<int> ExtraOptionIds { get; set; } = new List<int>();
        public ICollection<int> FacilityIds { get; set; } = new List<int>();
    }
}
