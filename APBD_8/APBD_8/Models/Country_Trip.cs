namespace APBD_8.Models
{
    public class Country_Trip
    {
        public int IdCountry { get; set; }
        public int IdTrip { get; set; }

        public Country Country { get; set; }
        public Trip Trip { get; set; }
    }
}
