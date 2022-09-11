namespace AirbnbTest.Models
{
    public class Property
    {
        public string ListingId { get; set; }
        public string Title { get; set; }
        public decimal Rating { get; set; }
       // public decimal Price { get; set; }
        public int ReviewsCount { get; set; }
        public string Address { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string RoomType { get; set; }
        public bool IsSuperHost { get; set; }
        public decimal LocationRating { get; set; }
        public decimal CheckingRating { get; set; }
        public decimal AccuracyRating { get; set; }
        public decimal CleanlinessRating { get; set; }
        public decimal CommunicationRating { get; set; }
        public string Guests { get; set; }
        public string Bedrooms { get; set; }
        public string Beds { get; set; }
        public string Baths { get; set; }
        public string Desc { get; set; }
        public string Images { get; set; }
        public string Amenities { get; set; }
        public string Availability { get; set; }
    }
}