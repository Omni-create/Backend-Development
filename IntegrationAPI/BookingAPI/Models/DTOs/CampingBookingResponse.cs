using System.Text.Json.Serialization;

namespace BookingOrchestrationApi.Models.DTOs
{
    public class CampingBookingResponse
    {
        public int BoekingID { get; set; }
        public int GebruikerID { get; set; }
        public DateTime Datum { get; set; }
        public int AccommodatieID { get; set; }
        public DateTime CheckInDatum { get; set; }
        public DateTime CheckOutDatum { get; set; }
        public int AantalVolwassenen { get; set; }
        public int AantalJongeKinderen { get; set; }
        public int AantalOudereKinderen { get; set; }
        public string? Opmerking { get; set; }
        public bool Cancelled { get; set; }
        
        // Optional nested objects
        public GebruikerDto? Gebruiker { get; set; }
        public AccommodatieDto? Accommodatie { get; set; }
        public List<BetalingDto> Betalingen { get; set; } = new();
        
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GebruikerDto
    {
        public int GebruikerID { get; set; }
        public string Naam { get; set; } = string.Empty;
        public string Emailadres { get; set; } = string.Empty;
        public string Telefoon { get; set; } = string.Empty;
        public string Autokenteken { get; set; } = string.Empty;
        public string Taal { get; set; } = string.Empty;
    }

    public class AccommodatieDto
    {
        public int AccommodatieID { get; set; }
        public int CampingID { get; set; }
        public CampingDto? Camping { get; set; }
    }

    public class CampingDto
    {
        public int CampingID { get; set; }
        public string Regels { get; set; } = string.Empty;
        public decimal Lengte { get; set; }
        public decimal Breedte { get; set; }
        public decimal Stroom { get; set; }
        public bool Huisdieren { get; set; }
        public string Accommodatie { get; set; } = string.Empty;
    }

    public class BetalingDto
    {
        public int BetalingID { get; set; }
        public int BoekingID { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Bedrag { get; set; }
        public string Methode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Korting { get; set; }
        public DateTime DatumOrigine { get; set; }
        public DateTime? DatumBetaald { get; set; }
    }
}