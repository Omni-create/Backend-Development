namespace BookingOrchestrationApi.Models.DTOs.Restaurant
{
    public class RestaurantReservationDto
    {
        public int ReserveringID { get; set; }
        public int BoekingID { get; set; }
        public DateTime DatumTijd { get; set; }
        public bool Cancelled { get; set; }
        public int AantalVolwassenen { get; set; }
        public int AantalJongeKinderen { get; set; }
        public int AantalOudereKinderen { get; set; }
        public int TafelID { get; set; }
        public string Tafel { get; set; } = string.Empty;
        public RekeningDto? Rekening { get; set; }
        
        // Additional properties from GET endpoint
        public int Tafelnummer { get; set; }
        public int AantalPersonen { get; set; }
        public bool IsGeannuleerd { get; set; }
        public string RekeningStatus { get; set; } = string.Empty;
    }

    public class RekeningDto
    {
        public int RekeningID { get; set; }
        public string BetaalMethode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotaalBetaald { get; set; }
        public int ReserveringID { get; set; }
        public string Reservering { get; set; } = string.Empty;
        public List<BestellingDto> Bestellingen { get; set; } = new();
    }

    public class BestellingDto
    {
        public int BestellingID { get; set; }
        public int RekeningID { get; set; }
        public string Rekening { get; set; } = string.Empty;
        public List<BestelRegelDto> BestelRegels { get; set; } = new();
    }

    public class BestelRegelDto
    {
        public int BestelRegelID { get; set; }
        public int Aantal { get; set; }
        public string Aanpassing { get; set; } = string.Empty;
        public int BestellingID { get; set; }
        public string Bestelling { get; set; } = string.Empty;
        public int GerechtID { get; set; }
        public GerechtDto Gerecht { get; set; } = new();
    }

    public class GerechtDto
    {
        public int GerechtID { get; set; }
        public string Naam { get; set; } = string.Empty;
        public string Omschrijving { get; set; } = string.Empty;
        public decimal Prijs { get; set; }
        public string Allergenen { get; set; } = string.Empty;
    }

    public class TafelDto
    {
        public int TafelID { get; set; }
        public int Tafelnummer { get; set; }
        public int AantalPlaatsen { get; set; }
        public List<RestaurantReservationDto> Reserveringen { get; set; } = new();
    }
}