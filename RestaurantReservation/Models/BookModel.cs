namespace RestaurantReservation.Models
{
    public class BookModel
    {
        public string RestaurantName { get; set; }
        public string BookingDate { get; set; }
        public string BookingTime { get; set; }
        public int NumberOfPeople { get; set; }

        public string BookedDate { get; set; }
    }
}
