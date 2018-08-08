using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace michaelherbert.org.ViewModels
{

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Security Question")]
        public string Question { get; set; }

        [Display(Name = "Security Answer")]
        public string Answer { get; set; }

    }

    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
    }

    public class Security
    {

        public static string HashSHA512(string value)
        {
            var SHA512 = System.Security.Cryptography.SHA512.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hash = SHA512.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }


        public static int GetUserID(string UserName, string Password)
        {
            int userID = 0;
            string conn = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
            using (SqlConnection newConn = new SqlConnection(conn))
            {
                using (SqlCommand newComm = new SqlCommand("SELECT UserIDkey, UserName, UserPassword, UserGUID " +
                    "FROM [UserLogin] WHERE UserName=@UserName", newConn))
                {
                    newComm.Parameters.AddWithValue("UserName", UserName);
                    newConn.Open();
                    SqlDataReader dr = newComm.ExecuteReader();
                    while (dr.Read())
                    {
                        int dbUserId = Convert.ToInt32(dr["UserIDkey"]);
                        string dbPassword = Convert.ToString(dr["UserPassword"]);
                        string dbUserGuid = Convert.ToString(dr["UserGUID"]);

                        string hashedPassword = Security.HashSHA512((Password) + dbUserGuid);

                        if (dbPassword == hashedPassword)
                        {
                            userID = dbUserId;
                        }
                    }
                    newConn.Close();
                }
            }


            return userID;
        }
    }
}