using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using michaelherbert.org.ViewModels;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net;
using System.Threading;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Data;
using System.Web.Security;

namespace michaelherbert.org.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel login, string ReturnUrl = "")
        {
            var UserName = login.UserName;
            var Password = login.Password;
            string message = "";


            if (UserName != null && Password != null)
            {
                int userId = Security.GetUserID(UserName, Password);
                if (userId > 0)
                {
                    FormsAuthentication.SetAuthCookie(userId.ToString(), true);
                    Session["userID"] = userId;
                    Session["userName"] = login.UserName;
                    Session["password"] = login.Password;
                    // Now you can put users id in a session-variable or what you prefer
                    // and redirect the user to the protected area of your website.
                    string sessionUName = Session["userName"].ToString();
                    int userActivated = 0;
                    string con = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
                    using (SqlConnection newCon = new SqlConnection(con))
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT Active FROM [UserLogin] WHERE UserName=@UserName", newCon))
                        {
                            cmd.Parameters.AddWithValue("UserName", sessionUName);
                            newCon.Open();
                            SqlDataReader dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                int activeState = Convert.ToInt32(dr["Active"]);
                                userActivated = activeState;
                            }
                            newCon.Close();
                        }
                    }
                    if (userActivated == 1)
                    {
                        sessionUName = Session["userName"].ToString();
                        //LoginBtn.Text = "Account Settings";
                        //LoginBtn.OnClientClick = "javascript:void(0); return false;";
                        //LoginBtn.Attributes.Add("OnClick", "AccountSettingsRedirect_Click");
                    }

                    if (Url.IsLocalUrl(ReturnUrl))
                    { 
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("/Index", "Home");
                    }
                }
                else
                {
                    message = "Invalid Credentials Provided";
                }
            }
            else if (UserName == null || Password == null)
            {
                message = "User Name & Password Required!";
             
            }
            ViewBag.Message = message;
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }

        public ActionResult VerifyAccount()
        {
            return View();
        }

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            string connStr = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
            string activationCode = !string.IsNullOrEmpty(Request.QueryString["ActivationCode"]) ? Request.QueryString["ActivationCode"] : Guid.Empty.ToString();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT UserID FROM UserActivation WHERE ActivationCode = @ActivationCode", con))
                {
                    int userID = 0;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            userID = Convert.ToInt32(dr["UserID"]);
                        }
                    }
                    con.Close();

                    if (userID != 0)
                    {
                        using (SqlCommand cmdUpdate = new SqlCommand("UPDATE UserLogin SET Active = @Active WHERE UserIDkey=@UserID", con))
                        {
                            cmdUpdate.Parameters.AddWithValue("@UserID", userID);
                            cmdUpdate.Parameters.AddWithValue("@Active", 1);
                            cmdUpdate.Connection = con;
                            con.Open();
                            cmdUpdate.ExecuteNonQuery();
                            con.Close();
                        }

                        using (SqlCommand cmdDel = new SqlCommand("DELETE FROM UserActivation WHERE ActivationCode = @ActivationCode", con))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmdDel.CommandType = CommandType.Text;
                                cmdDel.Parameters.AddWithValue("@ActivationCode", activationCode);
                                cmdDel.Connection = con;
                                con.Open();
                                int rowsAffected = cmdDel.ExecuteNonQuery();
                                con.Close();
                                if (rowsAffected == 1)
                                {
                                    TempData["msg"] = "Account Activated!";
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["msg"] = "Account Activation Error, please contact site admin!";
                    }
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult UserRegistration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserRegistration(RegisterViewModel vm)
        {
            if(ModelState.IsValid)
            {
                int userID = 0;
                string conn = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
                using (SqlConnection myConn = new SqlConnection(conn))
                {
                    using (SqlCommand myComm = new SqlCommand("Insert_User", myConn))
                    {

                        Guid userGuid = System.Guid.NewGuid();
                        string hashedPW = Security.HashSHA512((vm.Password) + userGuid.ToString());

                        myComm.CommandType = System.Data.CommandType.StoredProcedure;
                        myComm.Parameters.AddWithValue("UserName", vm.UserName.Trim());
                        myComm.Parameters.AddWithValue("Password", hashedPW.Trim());
                        myComm.Parameters.AddWithValue("Email", vm.Email.Trim());
                        myComm.Parameters.AddWithValue("Question", vm.Question.Trim());
                        myComm.Parameters.AddWithValue("Answer", vm.Answer.Trim());
                        myComm.Parameters.AddWithValue("Active", 0);
                        myComm.Parameters.AddWithValue("UserGUID", userGuid);

                        myConn.Open();
                        userID = Convert.ToInt32(myComm.ExecuteScalar());
                        myComm.CommandTimeout = 4 * 10;
                        myConn.Close();
                    }
                }

                string message = string.Empty;
                switch (userID)
                {
                    case -1:
                        message = "Username already exists.\\nPlease choose a different username.";
                        break;
                    case -2:
                        message = "Email address already used.";
                        break;
                    default:
                        message = "Registration successful.  Activation email has been sent.";
                        SendActivationEmail(userID, vm.Email, vm.UserName);
                        break;
                }
            }

            return View();
        }

        private void SendActivationEmail(int userID, string email, string UserName)
        {
            string connStr = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
            string activationCode = Guid.NewGuid().ToString();
            string emailPassword = ConfigurationManager.AppSettings.Get("emailPassword").ToString();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO UserActivation VALUES(@UserID, @ActivationCode)"))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }

            using (MailMessage mail = new MailMessage())
            {
                var verifyUrl = "/Login/VerifyAccount?ActivationCode=" + activationCode;
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                mail.From = new MailAddress("Michael@michaelherbert.org");
                mail.To.Add(new MailAddress(email));

                mail.Subject = "Account Activation";
                string body = "Hello " + UserName.Trim() + ",";
                body += "<br /><br />Please click the following link to activate your account";
                body += "<br /><a href = '"+link+"'>"+link+"</a>";
                body += "<br /><br />Thanks";
                body += "<br /><br />If you believe you have received this email in error, please disregard.";
                mail.Body = body;
                mail.IsBodyHtml = true;
                
                SmtpClient smtp = new SmtpClient();
                NetworkCredential credentials = new NetworkCredential("Michael.Herbert.84@gmail.com", emailPassword);
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = credentials;
                smtp.Port = 587;
                smtp.Send(mail);
            }
        }
    }
}