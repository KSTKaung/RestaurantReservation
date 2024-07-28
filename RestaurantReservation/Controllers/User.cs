using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantReservation.Controllers
{
    public class User : Controller
    {
        public IConfiguration Configuration { get; }
        public User(IConfiguration configuration)

        {
            Configuration = configuration;
        }

        /// <summary>
        /// Login Controller
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Register Controller
        /// </summary>
        /// <returns></returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Login Process
        /// </summary>
        /// <param name="user"></param>
        [HttpPost]
        public void LoginData(UserModel user)
        {
            TempData["DangerMessage"] = "";
            try
            {
                //Encrypt Password
                var sha = SHA256.Create();
                var asByteArray = Encoding.Default.GetBytes(user.Password);
                var hashedPassword = sha.ComputeHash(asByteArray);

                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    String sql = "SELECT * FROM tbl_User WHERE email = @username AND password = @password";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", user.Email);
                        command.Parameters.AddWithValue("@password", Convert.ToBase64String(hashedPassword));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                HttpContext.Session.SetString("_UserID", reader.GetString(0));
                                Response.Redirect("/Restaurant/RestaurantList");
                            }
                            else
                            {
                                TempData["DangerMessage"] = "UserName And Password is Invalid!";
                                Response.Redirect("/User/Login");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        /// <summary>
        /// Register Process
        /// </summary>
        /// <param name="user"></param>
        [HttpPost]
        public void RegisterData(UserModel user)
        {
            try
            {
                //Encrypt Password
                var sha = SHA256.Create();
                var asByteArray = Encoding.Default.GetBytes(user.Password);
                var hashedPassword = sha.ComputeHash(asByteArray);

                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    String sql = "SELECT count(*) FROM tbl_User";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //Identify User ID
                                int count = reader.GetInt32(0);
                                count = count + 1;
                                if (count > 0 && count< 10)
		                            user.Id = "U00000" + count;
                                else if (count > 10 && count < 100)
                                    user.Id = "U0000" + count;
                                else if (count > 100 && count < 1000)
                                    user.Id = "U000" + count;
                                else if (count > 1000 && count < 10000)
                                    user.Id = "U00" + count;
                            }
                        }
                    }

                   sql = "INSERT INTO tbl_User" +
                                 "(id,email,password,name,phoneno,address,createddatetime,updateddatetime) VALUES" +
                                 "(@id,@username,@password,@name,@phoneno,@address,@createddatetime,@updateddatetime);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", user.Id);
                        command.Parameters.AddWithValue("@username", user.Email);
                        command.Parameters.AddWithValue("@password", Convert.ToBase64String(hashedPassword));
                        command.Parameters.AddWithValue("@name", user.Name);
                        command.Parameters.AddWithValue("@phoneno", user.PhoneNo);
                        command.Parameters.AddWithValue("@address", user.Address);
                        command.Parameters.AddWithValue("@createddatetime", DateTime.Now);
                        command.Parameters.AddWithValue("@updateddatetime", DateTime.Now);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }

            Response.Redirect("/User/Login");
        }

        /// <summary>
        /// Display Booked Restaurant Process
        /// </summary>
        /// <returns></returns>
        public ActionResult BookedRestaurantList()
        {
            List<BookModel> lstBookedRestaurant = new List<BookModel>();
            @TempData["InfoMessage"] = "";
            try
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    String sql = "SELECT r.RestaurantName, b.BookingDate, b.BookingTime, b.NumberOfPeople, b.CreatedDateTime FROM tbl_Restaurant r " +
                                    "INNER JOIN tbl_Booking b ON r.Id = b.RId WHERE b.UId = @uid";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@uid", HttpContext.Session.GetString("_UserID"));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BookModel bookModel = new BookModel();
                                bookModel.RestaurantName = reader.GetString(0);
                                bookModel.BookingDate = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                                bookModel.BookingTime = reader.GetTimeSpan(2).ToString();
                                bookModel.NumberOfPeople = reader.GetInt32(3);
                                bookModel.BookedDate = reader.GetDateTime(4).ToString();

                                lstBookedRestaurant.Add(bookModel);
                            }
                            if(lstBookedRestaurant.IsNullOrEmpty())
                            {
                                @TempData["InfoMessage"] = "You Haven't Book Any Restaurant Yet!";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return View(lstBookedRestaurant);
        }

        /// <summary>
        /// LogOut Process
        /// </summary>
        public void LogOut()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/User/Login");
        }
    }
}
