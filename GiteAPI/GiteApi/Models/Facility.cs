namespace GiteApi.Models
{
    public class Facility
    {
        public int FacilityID { get; set; }
        public string FacilityName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
