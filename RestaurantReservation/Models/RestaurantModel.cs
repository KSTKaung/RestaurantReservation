namespace RestaurantReservation.Models
{
    public class RestaurantModel
    {
        /*
         * For Restaurant
         */
        public string Id { get; set; }
        public string RestaurantName { get; set; }
        public string Image { get; set; }

        public string OpenFromDate { get; set; }

        public string OpenToDate { get; set; }
        public string OpenFromTime { get; set; }

        public string OpenToTime { get; set; }
        public int NoOfPeople { get; set; }
        public string Remark { get; set; }

        /*
         * For Booking
         */
        public string BID { get; set; }
        public string RID { get; set; }
        public string UID { get; set; }
        public DateOnly BookingDate { get; set; }
        public TimeOnly BookingTime { get; set; }
        public int BookingNumberOfPeople { get; set; }
    }
}
