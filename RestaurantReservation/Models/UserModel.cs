namespace RestaurantReservation.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }

        public int PhoneNo { get; set; }
        public string Address { get; set; }
    }
}
