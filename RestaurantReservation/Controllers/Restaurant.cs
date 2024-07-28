using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestaurantReservation.Models;
using System.Text;

namespace RestaurantReservation.Controllers
{
    public class Restaurant : Controller
    {
        public IConfiguration Configuration { get; }

        public List<RestaurantModel> lstRestaurant = new List<RestaurantModel>();
        public Restaurant(IConfiguration configuration)

        {
            Configuration = configuration;
        }

        /// <summary>
        /// RestuarantList Detail
        /// </summary>
        /// <returns></returns>
        public ActionResult RestaurantList()
        {
            RestaurantModel restaurantList = new RestaurantModel();
            try
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    String sql = "Select * from tbl_Restaurant";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RestaurantModel resmodel = new RestaurantModel();
                                resmodel.Id = reader.GetString(0);
                                resmodel.RestaurantName = reader.GetString(1);
                                resmodel.Image = reader.GetString(2);
                                resmodel.OpenFromDate = reader.GetString(3);
                                resmodel.OpenToDate = reader.GetString(4);
                                resmodel.OpenFromTime = reader.GetString(5);
                                resmodel.OpenToTime = reader.GetString(6);
                                resmodel.NoOfPeople = reader.GetInt32(7);
                                resmodel.Remark = reader.GetString(8);

                                lstRestaurant.Add(resmodel);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return View(lstRestaurant);
        }

        /// <summary>
        /// Booking Process
        /// </summary>
        /// <param name="lstRestaurant"></param>
        [HttpPost]
        public void BookingData(List<RestaurantModel> lstRestaurant)
        {
            TempData["DangerMessage"] = "";
            TempData["InfoMessage"] = "";
            if (!String.IsNullOrEmpty(lstRestaurant[0].RID))
            {
                string bookingId = String.Empty;
                int restaurantNoOfPeople = 0, bookingNoOfPeople = 0;
                try
                {
                    var connectionString = Configuration.GetConnectionString("DefaultConnection");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        String sql = "SELECT r.NumberofPeople,ISNULL(b.NumberOfPeople,0) as NumberOfPeople FROM tbl_Restaurant r " +
                                        "FULL OUTER JOIN tbl_Booking b ON r.Id = b.RId WHERE r.Id = @rid;";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@rid", lstRestaurant[0].RID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    restaurantNoOfPeople = reader.GetInt32(0);
                                    bookingNoOfPeople += reader.GetInt32(1);
                                }
                            }
                        }
                        //Checking condition not to book out of people of restaurant limit
                        bookingNoOfPeople += lstRestaurant[0].BookingNumberOfPeople;
                        int result = restaurantNoOfPeople - bookingNoOfPeople;
                        if (result < 0)
                        {
                            int allowPeople = lstRestaurant[0].BookingNumberOfPeople - System.Math.Abs(result);
                            if  (allowPeople == 0)
                                TempData["DangerMessage"] = "Cannot book for this resturant. This restaurant is full of booking";
                            else
                                TempData["InfoMessage"] = "Cannot book all people for this resturant." +
                                                            "Can be book only " + allowPeople + " people.";

                            Response.Redirect("/Restaurant/RestaurantList");
                        }
                        else
                        {
                            sql = "SELECT count(*) FROM tbl_Booking";

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    //Identify Booking ID
                                    if (reader.Read())
                                    {
                                        int count = reader.GetInt32(0);
                                        count = count + 1;
                                        if (count > 0 && count < 10)
                                            bookingId = "B00000" + count;
                                        else if (count > 10 && count < 100)
                                            bookingId = "B0000" + count;
                                        else if (count > 100 && count < 1000)
                                            bookingId = "B000" + count;
                                        else if (count > 1000 && count < 10000)
                                            bookingId = "B00" + count;
                                    }
                                }
                            }

                            sql = "INSERT INTO tbl_Booking" +
                                     "(id,rid,uid,bookingdate,bookingtime,numberofpeople,createddatetime,updateddatetime) VALUES" +
                                     "(@id,@rid,@uid,@bookingdate,@bookingtime,@numberofpeople,@createddatetime,@updateddatetime);";

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@id", bookingId);
                                command.Parameters.AddWithValue("@rid", lstRestaurant[0].RID);
                                command.Parameters.AddWithValue("@uid", HttpContext.Session.GetString("_UserID"));
                                command.Parameters.AddWithValue("@bookingdate", lstRestaurant[0].BookingDate);
                                command.Parameters.AddWithValue("@bookingtime", lstRestaurant[0].BookingTime);
                                command.Parameters.AddWithValue("@numberofpeople", lstRestaurant[0].BookingNumberOfPeople);
                                command.Parameters.AddWithValue("@createddatetime", DateTime.Now);
                                command.Parameters.AddWithValue("@updateddatetime", DateTime.Now);

                                command.ExecuteNonQuery();
                            }

                            Response.Redirect("/User/BookedRestaurantList");
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
